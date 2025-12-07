using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Games.Application.producer
{
    public class UserActiveProducer: IUserActiveProducer
    {
        public void PublishUserActiveEvent(string queue, string message)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" }; // ou nome do container no docker-compose
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: "",
                routingKey: queue,
                basicProperties: null,
                body: body);
        }
    }

    public class UserEvent
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public EventUser EventType { get; set; }
    }

    public enum EventUser
    {
        create = 1,
        block = 2,
        active = 3,
        delete = 4
    }
}
