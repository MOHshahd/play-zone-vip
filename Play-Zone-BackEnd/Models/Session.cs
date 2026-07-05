namespace Play_Zone_BackEnd.Models;

public class Session
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool IsOpenTime { get; set; }
    public int? DurationMinutes { get; set; }
    public bool IsPaused { get; set; }
    public long PausedTimeMs { get; set; }
    public DateTime? PauseStartedAt { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public List<SessionModeLog> ModeLogs { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = new();
    public decimal TotalAmount { get; set; }
}
