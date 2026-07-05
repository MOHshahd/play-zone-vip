using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Play_Zone_BackEnd.Data;
using Play_Zone_BackEnd.Models;
using Play_Zone_BackEnd.Models.DTOs;

namespace Play_Zone_BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceiptsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReceiptsController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<ActionResult<Receipt>> Create([FromBody] CreateReceiptRequest request)
    {
        // Only link to session if the session actually exists in DB to avoid FK violation
        string? validSessionId = null;
        if (!string.IsNullOrEmpty(request.SessionId))
        {
            var sessionExists = await _db.Sessions.AnyAsync(s => s.Id == request.SessionId);
            if (sessionExists) validSessionId = request.SessionId;
        }

        var receipt = new Receipt
        {
            SessionId = validSessionId,
            DeviceName = request.DeviceName,
            StartTime = DateTime.TryParse(request.StartTime, out var st) ? st : DateTime.UtcNow,
            EndTime = DateTime.TryParse(request.EndTime, out var et) ? et : DateTime.UtcNow,
            TotalDuration = TimeSpan.TryParse(request.TotalDuration, out var td) ? td : TimeSpan.Zero,
            TimeCharge = request.TimeCharge,
            OrdersTotal = request.OrdersTotal,
            GrandTotal = request.GrandTotal,
            PaymentMethod = request.PaymentMethod ?? "cash",
        };

        if (Enum.TryParse<DeviceType>(request.DeviceType, true, out var dt))
            receipt.DeviceType = dt;

        foreach (var m in request.ModeEntries)
        {
            receipt.ModeEntries.Add(new ReceiptModeEntry
            {
                ReceiptId = receipt.Id,
                Mode = m.Mode,
                Duration = m.Duration,
                Charge = m.Charge
            });
        }

        foreach (var o in request.OrderItems)
        {
            receipt.OrderItems.Add(new ReceiptOrderItem
            {
                ReceiptId = receipt.Id,
                Name = o.Name,
                Price = o.Price,
                Quantity = o.Quantity
            });
        }

        _db.Receipts.Add(receipt);
        await _db.SaveChangesAsync();
        return StatusCode(201, receipt);
    }

    [HttpGet("today")]
    public async Task<ActionResult<TodaySummaryDto>> GetTodaySummary()
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);

        var receipts = await _db.Receipts
            .Where(r => r.CreatedAt >= todayStart && r.CreatedAt < todayEnd)
            .ToListAsync();

        return Ok(new TodaySummaryDto
        {
            TotalSessions = receipts.Count,
            TotalRevenue = receipts.Sum(r => r.GrandTotal),
            PlayRevenue = receipts.Sum(r => r.TimeCharge),
            OrdersRevenue = receipts.Sum(r => r.OrdersTotal)
        });
    }

    [HttpGet("by-date")]
    public async Task<ActionResult<List<Receipt>>> GetByDate([FromQuery] int year, [FromQuery] int month, [FromQuery] int? day = null)
    {
        var query = _db.Receipts
            .Include(r => r.ModeEntries)
            .Include(r => r.OrderItems)
            .AsQueryable();

        if (day.HasValue)
        {
            var date = new DateTime(year, month, day.Value);
            query = query.Where(r => r.CreatedAt.Year == year && r.CreatedAt.Month == month && r.CreatedAt.Day == day.Value);
        }
        else
        {
            query = query.Where(r => r.CreatedAt.Year == year && r.CreatedAt.Month == month);
        }

        var receipts = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Ok(receipts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Receipt>> GetById(string id)
    {
        var receipt = await _db.Receipts
            .Include(r => r.ModeEntries)
            .Include(r => r.OrderItems)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (receipt == null) return NotFound();
        return Ok(receipt);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var receipt = await _db.Receipts.FindAsync(id);
        if (receipt == null) return NotFound();
        _db.Receipts.Remove(receipt);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAll([FromQuery] int? year, [FromQuery] int? month)
    {
        var query = _db.Receipts.AsQueryable();
        if (year.HasValue)
            query = query.Where(r => r.CreatedAt.Year == year.Value);
        if (month.HasValue)
            query = query.Where(r => r.CreatedAt.Month == month.Value);
        var count = await query.CountAsync();
        _db.Receipts.RemoveRange(query);
        await _db.SaveChangesAsync();
        return Ok(new { deleted = count });
    }
}
