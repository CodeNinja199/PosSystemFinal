namespace Pos.Infrastructure.Messaging;

// The RabbitMq section of configuration: the host name of the broker (localhost here, rabbitmq inside Docker).
public class RabbitMqSettings
{
    public string Host { get; set; } = string.Empty;
}