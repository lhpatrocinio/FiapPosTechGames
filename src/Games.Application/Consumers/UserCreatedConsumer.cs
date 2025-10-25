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
        public BasicDeliverEventArgs _basicDeliver;

        public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger, IServiceScopeFactory serviceScopeFactory, RabbitMqSetup setup)
        {
            _logger = logger;
            _setup = setup;
            _channel = _setup.CreateChannel("create");
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                _basicDeliver = ea;
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                _logger.LogInformation($"[GamesApi] Novo usuário detectado: {userEvent?.Name} - {userEvent?.Email}");

                using var scope = _serviceScopeFactory.CreateScope();

                await AdicionarBiblioteca(scope, userEvent);

                PublicarEvento(scope, userEvent);
            };

            _channel.BasicConsume("user_create_queue", false, consumer);

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            base.Dispose();
        }

        private async Task AdicionarBiblioteca(IServiceScope scope, UserCreatedEvent userEvent)
        {
            var gamesRepository = scope.ServiceProvider.GetRequiredService<IGamesRepository>();
            var games = await gamesRepository.ListGamesFree();

            await gamesRepository.BeginTransactionAsync();
            try
            {
                var library = new Domain.Entities.Library()
                {
                    CreateAt = DateTime.UtcNow,
                    IdUser = userEvent.UserId,
                    IsActive = true,
                    Name = "Biblioteca Padrão"
                };

                await gamesRepository.AddLibraryAsync(library);

                foreach (var game in games)
                {

                    await gamesRepository.AddGameLibraryAsync(new Domain.Entities.GameLibrary()
                    {
                        IdGame = game.Id,
                        IdLibrary = library.Id
                    });

                    _channel.BasicAck(_basicDeliver.DeliveryTag, false);
                }

                await gamesRepository.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar evento novo usuário: {userEvent?.Name} - {userEvent?.Email}", ex);
                await gamesRepository.RollbackTransactionAsync();

                int retryCount = GetRetryCount(_basicDeliver.BasicProperties);

                if (retryCount >= MaxRetries)
                {
                    _logger.LogInformation($"Enviando para DLQ após {retryCount} tentativas");
                    SendToDlq(_basicDeliver.Body.ToArray());
                    _channel.BasicAck(_basicDeliver.DeliveryTag, false);
                }
                else
                {
                    _logger.LogInformation($"Retry {retryCount + 1} de {MaxRetries}");
                    SendToRetryQueue(_basicDeliver.Body.ToArray(), retryCount + 1);
                    _channel.BasicAck(_basicDeliver.DeliveryTag, false);
                }
            }
        }

        private void PublicarEvento(IServiceScope scope, UserCreatedEvent userEvent)
        {
            var userProducer = scope.ServiceProvider.GetRequiredService<producer.IUserActiveProducer>();
            userProducer.PublishUserActiveEvent(new producer.UserEvent()
            {
                Id = userEvent.UserId,
                Name = userEvent.Name,
                Email = userEvent.Email,
                EventType = producer.EventUser.active
            });
        }

        private int GetRetryCount(IBasicProperties props)
        {
            if (props.Headers != null && props.Headers.TryGetValue("x-retry-count", out var value))
            {
                return int.Parse(Encoding.UTF8.GetString((byte[])value));
            }
            return 0;
        }

        private void SendToRetryQueue(byte[] body, int retryCount)
        {
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            props.Headers = new Dictionary<string, object>
            {
                { "x-retry-count", Encoding.UTF8.GetBytes(retryCount.ToString()) }
            };

            _channel.BasicPublish(
                exchange: "",
                routingKey: "user_create_queue",
                basicProperties: props,
                body: body);
        }

        private void SendToDlq(byte[] body)
        {
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;

            _channel.BasicPublish(
                exchange: "",
                routingKey: "user_create_dlq",
                basicProperties: props,
                body: body);
        }
    }

    public record UserCreatedEvent(Guid UserId, string Name, string Email, DateTime CreatedAt);
}
