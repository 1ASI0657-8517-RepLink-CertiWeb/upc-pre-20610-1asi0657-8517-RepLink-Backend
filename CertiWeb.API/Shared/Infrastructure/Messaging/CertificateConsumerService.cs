using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CertiWeb.API.Shared.Infrastructure.Messaging;

public class CertificateConsumerService : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CertificateConsumerService> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public CertificateConsumerService(
        IConfiguration configuration,
        ILogger<CertificateConsumerService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        try
        {
            var host = _configuration["RabbitMQ__Host"]
                    ?? _configuration["RabbitMQ:Host"]
                    ?? "rabbitmq";
            var factory = new ConnectionFactory { HostName = host };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: "inspection.completed",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation(
                    "[RabbitMQ] Mensaje recibido en inspection.completed: {Message}",
                    message
                );
                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(
                queue: "inspection.completed",
                autoAck: false,
                consumer: consumer
            );

            _logger.LogInformation(
                "[RabbitMQ] CertificateConsumerService escuchando en inspection.completed"
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                "[RabbitMQ] Consumer no pudo conectar: {Message}",
                ex.Message
            );
        }

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
