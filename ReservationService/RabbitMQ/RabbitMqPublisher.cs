using RabbitMQ.Client;
using System.Text;

namespace ReservationService.RabbitMQ
{
    public interface IRabbitMqPublisher
    {
        void Publish(string message);
    }

    public class RabbitMqPublisher : IRabbitMqPublisher
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "reservation_events";
        public void Publish(string message)
        {
            var factory = new ConnectionFactory() { HostName = _hostname };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var body = System.Text.Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
        }
    }
} 