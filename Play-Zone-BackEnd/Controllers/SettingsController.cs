using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Play_Zone_BackEnd.Data;
using Play_Zone_BackEnd.Models;

namespace Play_Zone_BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SettingsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<Dictionary<string, string>>> GetAll()
    {
        var settings = await _db.SiteSettings.ToListAsync();
        return Ok(settings.ToDictionary(s => s.Key, s => s.Value));
    }

    [HttpPost]
    public async Task<ActionResult> Set([FromBody] Dictionary<string, string> body)
    {
        foreach (var kv in body)
        {
            var existing = await _db.SiteSettings.FirstOrDefaultAsync(s => s.Key == kv.Key);
            if (existing != null)
            {
                existing.Value = kv.Value;
            }
            else
            {
                _db.SiteSettings.Add(new SiteSetting { Key = kv.Key, Value = kv.Value });
            }
        }
        await _db.SaveChangesAsync();
        return Ok();
    }
}
