namespace Play_Zone_BackEnd.Models;

public class ServiceRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? SessionId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string RequestType { get; set; } = "CallStaff"; // CallStaff, Order
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending"; // Pending, Accepted, Completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}