namespace Play_Zone_BackEnd.Models.DTOs;

public class ReceiptSummary
{
    public string Id { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string TotalDuration { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateReceiptRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string TotalDuration { get; set; } = string.Empty;
    public List<ModeEntryDto> ModeEntries { get; set; } = new();
    public List<OrderItemDto> OrderItems { get; set; } = new();
    public decimal TimeCharge { get; set; }
    public decimal OrdersTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string PaymentMethod { get; set; } = "cash";
}

public class ModeEntryDto
{
    public string Mode { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public decimal Charge { get; set; }
}

public class OrderItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class TodaySummaryDto
{
    public int TotalSessions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PlayRevenue { get; set; }
    public decimal OrdersRevenue { get; set; }
}
