namespace Cdm.Web.Models;

/// <summary>Mirror of the SignalR NotificationData sent by NotificationHub.</summary>
public class NotificationData
{
    public int? Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
    public int? SenderId { get; set; }
    public string? SenderName { get; set; }
}
