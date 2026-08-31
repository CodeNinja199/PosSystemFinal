using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using Pos.Application.Interfaces;
using Pos.Application.Messaging;

using RabbitMQ.Client;

namespace Pos.Infrastructure.Messaging;

// How messages leave the POS API:
// 1. OrderService calls PublishAsync only after SaveChangesAsync succeeded, so a message never describes something that was not saved.
// 2. The first call opens one connection and one channel for the life of the app (this class is a singleton) and declares the queue
//    as durable, so the queue itself survives a broker restart; each message is published as persistent JSON for the same reason.
// 3. This is the only try/catch outside the middleware: if RabbitMQ is unreachable, the failure is logged and the order stays saved.
// Follows the RabbitMQ tutorials https://www.rabbitmq.com/tutorials/tutorial-one-dotnet and https://www.rabbitmq.com/tutorials/tutorial-two-dotnet
public class RabbitMqNotificationMessagePublisher : INotificationMessagePublisher, IAsyncDisposable
{
    public const string NotificationsQueueName = "notifications";

    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly ILogger<RabbitMqNotificationMessagePublisher> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqNotificationMessagePublisher(RabbitMqSettings rabbitMqSettings, ILogger<RabbitMqNotificationMessagePublisher> logger)
    {
        _rabbitMqSettings = rabbitMqSettings;
        _logger = logger;
    }

    public async Task PublishAsync(NotificationMessage message)
    {
        try
        {
            IChannel channel = await GetOpenChannelAsync();

            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            string messageJson = JsonSerializer.Serialize(message, jsonOptions);
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);

            BasicProperties properties = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: NotificationsQueueName,
                mandatory: false,
                basicProperties: properties,
                body: messageBytes);

            _logger.LogInformation("Published {MessageType} for user {RecipientUserId}", message.Type, message.RecipientUserId);
        }
        catch (Exception publishException)
        {
            _logger.LogError(publishException, "Could not publish {MessageType} for user {RecipientUserId}", message.Type, message.RecipientUserId);
        }
    }

    private async Task<IChannel> GetOpenChannelAsync()
    {
        if (_channel != null && _channel.IsOpen)
        {
            return _channel;
        }

        ConnectionFactory connectionFactory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.Host
        };
        _connection = await connectionFactory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(queue: NotificationsQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        return _channel;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}