namespace Notification.Api.Entities;

public class Notification
{
    public int Id { get; set; }

    public int RecipientUserId { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }
}