using Games.Application.producer;
using Games.Application.Rabbit;
using Games.Application.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static System.Formats.Asn1.AsnWriter;

namespace Games.Application.Consumers
{
    public class UserCreatedConsumer : BackgroundService
    {
        private readonly ILogger<UserCreatedConsumer> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly RabbitMqSetup _setup;
        private readonly IModel _channel;
        private const int MaxRetries = 3;

        public UserCreatedConsumer(
            ILogger<UserCreatedConsumer> logger,
            IServiceScopeFactory serviceScopeFactory,
            RabbitMqSetup setup)
        {
            _logger = logger;
            _setup = setup;
            _channel = _setup.CreateChannel("user_create");
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (ch, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                using var scope = _serviceScopeFactory.CreateScope();

                try
                {
                    bool ok = await ProcessarUsuario(scope, userEvent);

                    if (ok)
                    {
                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
                catch (Exception ex)
                {
                    await TratarErro(ea, scope, userEvent, ex);
                }
            };

            _channel.BasicConsume("user_create_queue", autoAck: false, consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            base.Dispose();
        }

        private async Task<bool> ProcessarUsuario(IServiceScope scope, UserCreatedEvent userEvent)
        {
            var repo = scope.ServiceProvider.GetRequiredService<IGamesRepository>();
            var games = await repo.ListGamesFree();

            await repo.BeginTransactionAsync();

            // Simula erro
            throw new Exception("Teste RETRY");

            var library = new Domain.Entities.Library()
            {
                CreateAt = DateTime.UtcNow,
                IdUser = userEvent.UserId,
                IsActive = true,
                Name = "Biblioteca Padrão"
            };

            await repo.AddLibraryAsync(library);

            foreach (var game in games)
            {
                await repo.AddGameLibraryAsync(new Domain.Entities.GameLibrary()
                {
                    IdGame = game.Id,
                    IdLibrary = library.Id
                });
            }

            await repo.CommitTransactionAsync();
            return true;
        }

        private async Task TratarErro(BasicDeliverEventArgs ea, IServiceScope scope, UserCreatedEvent userEvent, Exception ex)
        {
            _logger.LogError($"Erro ao processar evento: {ex.Message}");

            int retryCount = GetRetryCount(ea.BasicProperties);
            var json = JsonSerializer.Serialize(userEvent);

            if (retryCount >= MaxRetries)
            {
                SendToDlq(json);
            }
            else
            {
                SendToRetryQueue(json, retryCount + 1);
            }

            _channel.BasicAck(ea.DeliveryTag, false);
        }

        private void PublicarEvento(IServiceScope scope, UserCreatedEvent userEvent)
        {
            var userProducer = scope.ServiceProvider.GetRequiredService<producer.IUserActiveProducer>();
            var message = JsonSerializer.Serialize(new producer.UserEvent()
            {
                Id = userEvent.UserId,
                Name = userEvent.Name,
                Email = userEvent.Email,
                EventType = producer.EventUser.active
            });

            userProducer.PublishUserActiveEvent("user_active_queue", message);
        }

        private int GetRetryCount(IBasicProperties props)
        {
            if (props.Headers != null && props.Headers.TryGetValue("x-retry-count", out var value))
            {
                return int.Parse(Encoding.UTF8.GetString((byte[])value));
            }
            return 0;
        }

        private void SendToRetryQueue(string message, int retryCount)
        {
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            props.Headers = new Dictionary<string, object>
        {
            { "x-retry-count", Encoding.UTF8.GetBytes(retryCount.ToString()) }
        };

            _channel.BasicPublish(
                 exchange: "user_create_dlx",
                 routingKey: "user_create_retry_key",
                 basicProperties: props,
                 body: Encoding.UTF8.GetBytes(message));
        }

        private void SendToDlq(string message)
        {
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;

            _channel.BasicPublish(
       exchange: "user_create_dlx",
       routingKey: "user_create_dlq_key",
       basicProperties: props,
       body: Encoding.UTF8.GetBytes(message));
        }
    }

    public record UserCreatedEvent(Guid UserId, string Name, string Email, DateTime CreatedAt);
}
