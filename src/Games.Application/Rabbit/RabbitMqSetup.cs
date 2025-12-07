using RabbitMQ.Client;

namespace Games.Application.Rabbit
{
    public class RabbitMqSetup
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMqSetup()
        {
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq",
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        private void ConfigureRabbitMq(string eventQueue)
        {
            var ExchangeMain = $"{eventQueue}_exchange";
            var ExchangeDLX = $"{eventQueue}_dlx";

            var QueueMain = $"{eventQueue}_queue";
            var QueueRetry = $"{eventQueue}_retry";
            var QueueDLQ = $"{eventQueue}_dlq";

            var RoutingMain = $"{eventQueue}_key";
            var RoutingRetry = $"{eventQueue}_retry_key";
            var RoutingDLQ = $"{eventQueue}_dlq_key";

            // Exchanges
            _channel.ExchangeDeclare(ExchangeMain, ExchangeType.Direct, durable: true);
            _channel.ExchangeDeclare(ExchangeDLX, ExchangeType.Direct, durable: true);

            // Queue principal
            _channel.QueueDeclare(
                queue: QueueMain,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", ExchangeDLX },
                    { "x-dead-letter-routing-key", RoutingRetry }
                });

            // Queue retry
            _channel.QueueDeclare(
                queue: QueueRetry,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", ExchangeMain },
                    { "x-dead-letter-routing-key", RoutingMain },
                    { "x-message-ttl", 10000 }
                });

            // Queue DLQ FINAL
            _channel.QueueDeclare(
                queue: QueueDLQ,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null // IMPORTANTE: não pode ter DLX
            );

            // Bindings

            // Main exchange → Main queue
            _channel.QueueBind(QueueMain, ExchangeMain, RoutingMain);

            // DLX → Retry (para segundo consumo)
            _channel.QueueBind(QueueRetry, ExchangeDLX, RoutingRetry);

            // DLX → DLQ (final)
            _channel.QueueBind(QueueDLQ, ExchangeDLX, RoutingDLQ);
        }

        public IModel CreateChannel(string eventQueue)
        {
            ConfigureRabbitMq(eventQueue);
            return _connection.CreateModel();
        }
    }
}
