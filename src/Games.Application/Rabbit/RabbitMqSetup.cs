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
                HostName = "localhost",
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
            var QueueRetry = $"{eventQueue}_queue";
            var QueueDLQ = $"{eventQueue}_dlq";
            var RoutingKey = $"{eventQueue}_key";

            _channel.ExchangeDeclare(ExchangeMain, ExchangeType.Direct, durable: true);

            _channel.ExchangeDeclare(ExchangeDLX, ExchangeType.Direct, durable: true);

            _channel.QueueDeclare(
                queue: QueueMain,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", ExchangeDLX },
                    { "x-dead-letter-routing-key", RoutingKey }
                });

            _channel.QueueDeclare(
                queue: QueueRetry,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object>
                {
                    { "x-dead-letter-exchange", ExchangeMain },
                    { "x-dead-letter-routing-key", RoutingKey },
                    { "x-message-ttl", 10000 } // 10 segundos
                });

            _channel.QueueDeclare(
                queue: QueueDLQ,
                durable: true,
                exclusive: false,
                autoDelete: false);


            _channel.QueueBind(QueueMain, ExchangeMain, RoutingKey);
            _channel.QueueBind(QueueRetry, ExchangeDLX, RoutingKey);
        }

        public IModel CreateChannel(string eventQueue)
        {
            ConfigureRabbitMq(eventQueue);
            return _connection.CreateModel();
        }
    }
}
