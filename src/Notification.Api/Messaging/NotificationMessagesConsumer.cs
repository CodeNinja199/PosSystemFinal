using System.Text;
using System.Text.Json;

using Notification.Api.Data;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Notification.Api.Messaging;

// How messages arrive:
// 1. This BackgroundService starts with the app, opens one connection and channel, declares the same durable queue, and subscribes.
// 2. For each message it deserializes the JSON, saves one Notification row in its own scope (the DbContext is scoped), then acknowledges.
//    Acknowledging after the save means a message is removed from the queue only once it is safely in the database.
// 3. If RabbitMQ cannot be reached, the failure is logged and the service stops; Docker's restart policy brings the API back until the broker is up.
// Follows the RabbitMQ tutorials https://www.rabbitmq.com/tutorials/tutorial-one-dotnet and https://www.rabbitmq.com/tutorials/tutorial-two-dotnet
public class NotificationMessagesConsumer : BackgroundService
{
    public const string NotificationsQueueName = "notifications";

    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<NotificationMessagesConsumer> _logger;

    public NotificationMessagesConsumer(RabbitMqSettings rabbitMqSettings, IServiceScopeFactory serviceScopeFactory, ILogger<NotificationMessagesConsumer> logger)
    {
        _rabbitMqSettings = rabbitMqSettings;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            ConnectionFactory connectionFactory = new ConnectionFactory
            {
                HostName = _rabbitMqSettings.Host
            };
            IConnection connection = await connectionFactory.CreateConnectionAsync(stoppingToken);
            IChannel channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(queue: NotificationsQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += HandleReceivedMessageAsync;

            await channel.BasicConsumeAsync(queue: NotificationsQueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
            _logger.LogInformation("Waiting for messages on {QueueName}", NotificationsQueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopped waiting for messages");
        }
        catch (Exception consumerException)
        {
            _logger.LogError(consumerException, "Could not connect to RabbitMQ at {Host}; the consumer is stopping", _rabbitMqSettings.Host);
        }
    }

    // Called by the RabbitMQ client for every message; the channel is the sender.
    private async Task HandleReceivedMessageAsync(object sender, BasicDeliverEventArgs eventArgs)
    {
        byte[] messageBytes = eventArgs.Body.ToArray();
        string messageJson = Encoding.UTF8.GetString(messageBytes);

        JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        NotificationMessage? message = JsonSerializer.Deserialize<NotificationMessage>(messageJson, jsonOptions);
        if (message == null)
        {
            _logger.LogError("A message on the queue could not be read: {MessageJson}", messageJson);
            return;
        }

        using (IServiceScope scope = _serviceScopeFactory.CreateScope())
        {
            NotificationDbContext context = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            Entities.Notification notification = new Entities.Notification
            {
                RecipientUserId = message.RecipientUserId,
                Type = message.Type,
                Message = message.Message,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();
        }

        AsyncEventingBasicConsumer consumer = (AsyncEventingBasicConsumer)sender;
        await consumer.Channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false);
        _logger.LogInformation("Received {MessageType} for user {RecipientUserId}", message.Type, message.RecipientUserId);
    }
}