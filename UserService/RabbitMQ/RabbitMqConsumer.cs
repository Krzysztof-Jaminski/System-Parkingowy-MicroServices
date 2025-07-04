using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UserService.Models;
using Microsoft.EntityFrameworkCore;

namespace UserService.RabbitMQ
{
    public class RabbitMqConsumer
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public RabbitMqConsumer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void StartConsuming()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "reservationQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var reservationEvent = JsonSerializer.Deserialize<ReservationEvent>(message);
                if (reservationEvent != null)
                {
                    using var scope = _scopeFactory.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                    var user = db.Users.FirstOrDefault(u => u.Id == reservationEvent.UserId);
                    if (user != null)
                    {
                        user.ReservationCount++;
                        db.SaveChanges();
                    }
                }
            };
            channel.BasicConsume(queue: "reservationQueue", autoAck: true, consumer: consumer);
            // Konsument działa w nieskończonej pętli
            while (true) { Thread.Sleep(1000); }
        }
    }

    public class ReservationEvent
    {
        public int UserId { get; set; }
        // inne pola jeśli potrzebne
    }
} 