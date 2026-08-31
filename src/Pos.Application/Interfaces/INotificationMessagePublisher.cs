using Pos.Application.Messaging;

namespace Pos.Application.Interfaces;

// Implemented by RabbitMqNotificationMessagePublisher in Infrastructure. Called by OrderService after an order is saved.
public interface INotificationMessagePublisher
{
    Task PublishAsync(NotificationMessage message);
}