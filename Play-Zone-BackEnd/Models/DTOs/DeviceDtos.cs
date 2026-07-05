using System.ComponentModel.DataAnnotations;

namespace Play_Zone_BackEnd.Models.DTOs;

public class CreateDeviceRequest
{
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [Required] public DeviceType Type { get; set; }
    [StringLength(1000)] public string Games { get; set; } = string.Empty;
}

public class UpdateDeviceRequest
{
    [StringLength(200)] public string? Name { get; set; }
    public DeviceType? Type { get; set; }
    public DeviceStatus? Status { get; set; }
    [StringLength(1000)] public string? Games { get; set; }
}
