using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using PredictionService.Models;
using Microsoft.EntityFrameworkCore;

namespace PredictionService.RabbitMQ
{
    public class RabbitMqConsumer
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "reservation_events";
        private readonly IServiceScopeFactory _scopeFactory;
        public RabbitMqConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void StartConsuming()
        {
            var factory = new ConnectionFactory() { HostName = _hostname };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"[PredictionService] Otrzymano zdarzenie rezerwacji: {message}");
                try
                {
                    var reservationEvent = JsonSerializer.Deserialize<ReservationEvent>(message);
                    if (reservationEvent != null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var db = scope.ServiceProvider.GetRequiredService<PredictionDbContext>();
                        // Szukaj predykcji dla danego miejsca i dnia
                        var date = reservationEvent.StartTime.Date;
                        var prediction = db.Predictions.FirstOrDefault(p => p.ParkingSpot == reservationEvent.ParkingSpot && p.Date == date);
                        if (prediction == null)
                        {
                            // Tworzenie nowej predykcji
                            prediction = new Prediction
                            {
                                Date = date,
                                ParkingSpot = reservationEvent.ParkingSpot,
                                ReservationCount = 1,
                                PredictedOccupancy = 1.0, // przykładowo 1 rezerwacja = 100%
                                UserId = reservationEvent.UserId,
                                StartTime = reservationEvent.StartTime,
                                EndTime = reservationEvent.EndTime,
                                Status = reservationEvent.Status,
                                CreatedAt = DateTime.UtcNow
                            };
                            db.Predictions.Add(prediction);
                        }
                        else
                        {
                            // Aktualizacja istniejącej predykcji
                            prediction.ReservationCount++;
                            prediction.PredictedOccupancy = Math.Min(1.0, prediction.ReservationCount / 10.0); // przykładowa logika
                            prediction.Status = reservationEvent.Status;
                            prediction.EndTime = reservationEvent.EndTime;
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PredictionService] Błąd przetwarzania zdarzenia: {ex.Message}");
                }
            };
            channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }
    }

    public class ReservationEvent
    {
        public int UserId { get; set; }
        public string ParkingSpot { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; }
    }
} 