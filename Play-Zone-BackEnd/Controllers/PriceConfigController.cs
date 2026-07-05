using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Play_Zone_BackEnd.Data;
using Play_Zone_BackEnd.Models;
using Play_Zone_BackEnd.Models.DTOs;

namespace Play_Zone_BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceConfigController : ControllerBase
{
    private readonly AppDbContext _db;

    public PriceConfigController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<PriceConfig>>> GetAll() =>
        Ok(await _db.PriceConfigs.ToListAsync());

    [HttpPost]
    public async Task<ActionResult<PriceConfig>> SetPrice([FromBody] SetPriceRequest request)
    {
        var existing = await _db.PriceConfigs
            .FirstOrDefaultAsync(p => p.DeviceType == request.DeviceType && p.Mode == request.Mode);

        if (existing != null)
        {
            existing.PricePerHour = request.PricePerHour;
            await _db.SaveChangesAsync();
            return Ok(existing);
        }

        var config = new PriceConfig
        {
            DeviceType = request.DeviceType,
            Mode = request.Mode,
            PricePerHour = request.PricePerHour
        };
        _db.PriceConfigs.Add(config);
        await _db.SaveChangesAsync();
        return StatusCode(201, config);
    }
}
