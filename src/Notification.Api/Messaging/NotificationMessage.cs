namespace Notification.Api.Messaging;

// A copy of the POS API's message shape; the Notification API does not care which service sent it.
public class NotificationMessage
{
    public string Type { get; set; } = string.Empty;

    public int RecipientUserId { get; set; }

    public string Message { get; set; } = string.Empty;
}