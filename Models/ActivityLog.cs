namespace AIAssistantSalesOperationsDashboard.Models;

/// <summary>
/// Represents an activity log entry tracking all user actions in the dashboard.
/// Used for audit trails and activity monitoring.
/// </summary>
public class ActivityLog
{
    /// <summary>
    /// Timestamp when the action occurred
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Type of action performed: "Create", "Update", "Delete", "Export", "Filter", etc.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the action and affected data
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Optional identifier of the affected entity (e.g., OrderID)
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Optional reference to related data
    /// </summary>
    public string? RelatedData { get; set; }

    /// <summary>
    /// Creates a new activity log entry with current timestamp
    /// </summary>
    public static ActivityLog Create(string action, string details, string? entityId = null, string? relatedData = null)
    {
        return new ActivityLog
        {
            Timestamp = DateTime.Now,
            Action = action,
            Details = details,
            EntityId = entityId,
            RelatedData = relatedData
        };
    }

    /// <summary>
    /// Formats the log entry for display
    /// </summary>
    public string GetDisplayText()
    {
        var timeAgo = GetTimeAgoText(Timestamp);
        return $"{timeAgo} - {Action}: {Details}";
    }

    /// <summary>
    /// Formats timestamp as "X minutes ago" or "X hours ago"
    /// </summary>
    private static string GetTimeAgoText(DateTime timestamp)
    {
        var timeSpan = DateTime.Now - timestamp;

        if (timeSpan.TotalSeconds < 60)
            return $"{(int)timeSpan.TotalSeconds}s ago";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes}m ago";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours}h ago";

        return timestamp.ToString("MMM dd, HH:mm");
    }
}
