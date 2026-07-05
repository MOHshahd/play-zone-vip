using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Play_Zone_BackEnd.Data;
using Play_Zone_BackEnd.Models;
using Play_Zone_BackEnd.Models.DTOs;

namespace Play_Zone_BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly AppDbContext _db;

    public DevicesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<Device>>> GetAll() =>
        Ok(await _db.Devices.OrderBy(d => d.CreatedAt).ToListAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<Device>> GetById(string id)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();
        return Ok(device);
    }

    [HttpPost]
    public async Task<ActionResult<Device>> Create([FromBody] CreateDeviceRequest request)
    {
        var device = new Device
        {
            Name = request.Name,
            Type = request.Type,
            Games = request.Games
        };
        _db.Devices.Add(device);
        await _db.SaveChangesAsync();
        return StatusCode(201, device);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Device>> Update(string id, [FromBody] UpdateDeviceRequest request)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();

        if (request.Name != null) device.Name = request.Name;
        if (request.Type.HasValue) device.Type = request.Type.Value;
        if (request.Status.HasValue) device.Status = request.Status.Value;
        if (request.Games != null) device.Games = request.Games;

        await _db.SaveChangesAsync();
        return Ok(device);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var device = await _db.Devices.FindAsync(id);
        if (device == null) return NotFound();
        _db.Devices.Remove(device);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
