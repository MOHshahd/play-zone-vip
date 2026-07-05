namespace Play_Zone_BackEnd.Models;

public class Receipt
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? SessionId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public bool IsOpenTime { get; set; }
    public List<ReceiptModeEntry> ModeEntries { get; set; } = new();
    public List<ReceiptOrderItem> OrderItems { get; set; } = new();
    public decimal TimeCharge { get; set; }
    public decimal OrdersTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string PaymentMethod { get; set; } = "cash";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ReceiptModeEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ReceiptId { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Duration { get; set; } = string.Empty;
    public decimal Charge { get; set; }
}

public class ReceiptOrderItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ReceiptId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Total => Price * Quantity;
}
