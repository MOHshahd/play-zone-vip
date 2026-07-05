namespace Play_Zone_BackEnd.Models;

public class PriceConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DeviceType DeviceType { get; set; }
    public SessionMode Mode { get; set; }
    public decimal PricePerHour { get; set; }
}
