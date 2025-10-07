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
        private IConnection _connection;
        private IModel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;

            var factory = new ConnectionFactory() { HostName = "rabbitmq" }; // ou nome do container no docker-compose
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: "create-queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var gamesRepository = scope.ServiceProvider.GetRequiredService<IGamesRepository>();    

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);

                _logger.LogInformation($"[GamesApi] Novo usuário detectado: {userEvent?.Name} - {userEvent?.Email}");

                var games = await gamesRepository.ListGamesFree();

                await gamesRepository.BeginTransactionAsync();

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
                    try
                    {
                        await gamesRepository.AddGameLibraryAsync(new Domain.Entities.GameLibrary()
                        {
                            IdGame = game.Id,
                            IdLibrary = library.Id
                        });

                        _channel.BasicAck(ea.DeliveryTag, false);

                        await gamesRepository.CommitTransactionAsync();

                    }
                    catch (Exception ex)



                    {
                        _logger.LogError($"Erro ao processar evento novo usuário: {userEvent?.Name} - {userEvent?.Email}", ex);

                        await gamesRepository.RollbackTransactionAsync();
                    }
                }

                var userProducer = scope.ServiceProvider.GetRequiredService<producer.IUserActiveProducer>();
                userProducer.PublishUserActiveEvent(new producer.UserEvent()
                {
                    Id = userEvent.UserId,
                    Name = userEvent.Name,
                    Email = userEvent.Email,
                    EventType = producer.EventUser.active
                });

            };

            _channel.BasicConsume("create-queue", false, consumer);       

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
