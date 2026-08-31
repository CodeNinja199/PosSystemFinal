namespace Pos.Application.Messaging;

// What the POS API puts on the queue: the type, who it is for, and the text. The Notification API stores it as it is.
public class NotificationMessage
{
    public string Type { get; set; } = string.Empty;

    public int RecipientUserId { get; set; }

    public string Message { get; set; } = string.Empty;
}