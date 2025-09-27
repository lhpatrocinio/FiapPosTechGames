using Games.Application.Repository;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Games.Application.Consumers
{
    public class UserCreatedConsumer : BackgroundService
    {
        private readonly ILogger<UserCreatedConsumer> _logger;
        private IConnection _connection;
        private IModel _channel;

        public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory() { HostName = "rabbitmq" }; // ou nome do container no docker-compose
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: "user-created-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                _logger.LogInformation($"[GamesApi] Novo usuário detectado: {userEvent?.Name} - {userEvent?.Email}");

             

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume("user-created-queue", false, consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }

    public record UserCreatedEvent(Guid UserId, string Name, string Email, DateTime CreatedAt);
}
