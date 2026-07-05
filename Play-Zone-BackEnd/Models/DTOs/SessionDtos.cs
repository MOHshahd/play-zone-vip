using System.ComponentModel.DataAnnotations;

namespace Play_Zone_BackEnd.Models.DTOs;

public class StartSessionRequest
{
    [Required] public string DeviceId { get; set; } = string.Empty;
    [Required] public SessionMode Mode { get; set; }
    public bool IsOpenTime { get; set; }
    [Range(1, 720)] public int? DurationMinutes { get; set; }
}

public class SwitchModeRequest
{
    [Required] public SessionMode NewMode { get; set; }
}

public class AddOrderRequest
{
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [Range(0, 99999)] public decimal Price { get; set; }
    [Range(1, 999)] public int Quantity { get; set; } = 1;
}

public class SetPriceRequest
{
    [Required] public DeviceType DeviceType { get; set; }
    [Required] public SessionMode Mode { get; set; }
    [Range(0, 9999)] public decimal PricePerHour { get; set; }
}

public class PauseSessionRequest
{
    public bool IsPaused { get; set; }
    public long PausedTimeMs { get; set; }
}

public class CustomerOrderItem
{
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [Range(0, 99999)] public decimal Price { get; set; }
    [Range(1, 999)] public int Quantity { get; set; } = 1;
}

public class CustomerOrderRequest
{
    [Required] public string DeviceId { get; set; } = string.Empty;
    [Required] public List<CustomerOrderItem> Items { get; set; } = new();
}
