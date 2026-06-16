using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace CertiWeb.API.Shared.Infrastructure.Messaging;

public class RabbitMQProducer
{
    private readonly string _host;

    public RabbitMQProducer(IConfiguration configuration)
    {
        _host = configuration["RabbitMQ__Host"] ?? "localhost";
    }

    public void Publish(string queueName, object message)
    {
        try
        {
            var factory = new ConnectionFactory { HostName = _host };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RabbitMQ] Error publishing to {queueName}: {ex.Message}");
        }
    }
}
