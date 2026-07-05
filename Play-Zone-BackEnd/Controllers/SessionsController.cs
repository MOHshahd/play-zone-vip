using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Play_Zone_BackEnd.Data;
using Play_Zone_BackEnd.Models;
using Play_Zone_BackEnd.Models.DTOs;

namespace Play_Zone_BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SessionsController(AppDbContext db) => _db = db;

    [HttpGet("active")]
    public async Task<ActionResult<List<Session>>> GetActive()
    {
        var active = await _db.Sessions
            .Include(s => s.ModeLogs)
            .Include(s => s.OrderItems)
            .Where(s => s.Status == SessionStatus.Active).ToListAsync();
        return Ok(active);
    }

    [HttpGet]
    public async Task<ActionResult<List<Session>>> GetAll()
    {
        var sessions = await _db.Sessions
            .Include(s => s.ModeLogs)
            .Include(s => s.OrderItems)
            .OrderByDescending(s => s.StartTime).ToListAsync();
        return Ok(sessions);
    }

    [HttpPost("start")]
    public async Task<ActionResult<Session>> StartSession([FromBody] StartSessionRequest request)
    {
        var device = await _db.Devices.FindAsync(request.DeviceId);
        if (device == null) return NotFound("Device not found");

        if (device.Status == DeviceStatus.Maintenance)
            return BadRequest("Device is under maintenance");

        var activeSession = await _db.Sessions
            .FirstOrDefaultAsync(s => s.DeviceId == request.DeviceId && s.Status == SessionStatus.Active);
        if (activeSession != null)
            return BadRequest("Device already has an active session");

        device.Status = DeviceStatus.InUse;

        var session = new Session
        {
            DeviceId = device.Id,
            DeviceName = device.Name,
            DeviceType = device.Type,
            StartTime = DateTime.UtcNow,
            IsOpenTime = request.IsOpenTime,
            DurationMinutes = request.DurationMinutes,
        };
        session.ModeLogs.Add(new SessionModeLog
        {
            SessionId = session.Id,
            Mode = request.Mode,
            StartTime = DateTime.UtcNow
        });

        _db.Sessions.Add(session);
        await _db.SaveChangesAsync();
        return Ok(session);
    }

    [HttpPost("{id}/switch-mode")]
    public async Task<ActionResult<Session>> SwitchMode(string id, [FromBody] SwitchModeRequest request)
    {
        var session = await _db.Sessions
            .Include(s => s.ModeLogs)
            .FirstOrDefaultAsync(s => s.Id == id && s.Status == SessionStatus.Active);
        if (session == null) return NotFound("Active session not found");

        var currentLog = session.ModeLogs.Last();
        if (currentLog.Mode == request.NewMode)
            return BadRequest("Already in this mode");

        currentLog.EndTime = DateTime.UtcNow;
        session.ModeLogs.Add(new SessionModeLog
        {
            SessionId = session.Id,
            Mode = request.NewMode,
            StartTime = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Ok(session);
    }

    [HttpPost("{id}/add-order")]
    public async Task<ActionResult<Session>> AddOrder(string id, [FromBody] AddOrderRequest request)
    {
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.Id == id && s.Status == SessionStatus.Active);
        if (session == null) return NotFound("Active session not found");

        var order = new OrderItem
        {
            SessionId = session.Id,
            Name = request.Name,
            Price = request.Price,
            Quantity = request.Quantity
        };
        _db.OrderItems.Add(order);
        await _db.SaveChangesAsync();
        return Ok(session);
    }

    [HttpPost("{id}/pause")]
    public async Task<ActionResult> PauseSession(string id, [FromBody] PauseSessionRequest request)
    {
        var session = await _db.Sessions
            .FirstOrDefaultAsync(s => s.Id == id && s.Status == SessionStatus.Active);
        if (session == null) return NotFound("Active session not found");

        if (request.IsPaused && !session.IsPaused)
        {
            session.IsPaused = true;
            session.PauseStartedAt = DateTime.UtcNow;
        }
        else if (!request.IsPaused && session.IsPaused && session.PauseStartedAt.HasValue)
        {
            var pauseDuration = (long)(DateTime.UtcNow - session.PauseStartedAt.Value).TotalMilliseconds;
            session.PausedTimeMs += Math.Max(0, pauseDuration);
            session.IsPaused = false;
            session.PauseStartedAt = null;
        }

        await _db.SaveChangesAsync();

        return Ok(new
        {
            session.Id,
            session.IsPaused,
            session.PausedTimeMs,
            session.PauseStartedAt
        });
    }

    [HttpPost("{id}/end")]
    public async Task<ActionResult> EndSession(string id)
    {
        var session = await _db.Sessions
            .Include(s => s.ModeLogs)
            .Include(s => s.OrderItems)
            .FirstOrDefaultAsync(s => s.Id == id && s.Status == SessionStatus.Active);
        if (session == null) return NotFound("Active session not found");

        session.EndTime = DateTime.UtcNow;
        session.Status = SessionStatus.Completed;

        var lastLog = session.ModeLogs.Last();
        lastLog.EndTime = session.EndTime;

        var device = await _db.Devices.FindAsync(session.DeviceId);
        if (device != null) device.Status = DeviceStatus.Available;

        await _db.SaveChangesAsync();

        return Ok(new { session.Id, session.EndTime });
    }

    private async Task<Receipt> GenerateReceipt(Session session)
    {
        var receipt = new Receipt
        {
            SessionId = session.Id,
            DeviceName = session.DeviceName,
            DeviceType = session.DeviceType,
            StartTime = session.StartTime,
            EndTime = session.EndTime!.Value,
            TotalDuration = session.EndTime.Value - session.StartTime,
            IsOpenTime = session.IsOpenTime
        };

        decimal timeCharge = 0;

        var priceConfigs = await _db.PriceConfigs
            .Where(p => p.DeviceType == session.DeviceType)
            .ToListAsync();
        var priceByMode = priceConfigs.ToDictionary(p => p.Mode, p => p.PricePerHour);

        foreach (var log in session.ModeLogs)
        {
            if (log.EndTime == null) continue;

            var duration = log.EndTime.Value - log.StartTime;
            var pricePerHour = priceByMode.GetValueOrDefault(log.Mode, 30);
            var charge = (decimal)duration.TotalHours * pricePerHour;

            receipt.ModeEntries.Add(new ReceiptModeEntry
            {
                ReceiptId = receipt.Id,
                Mode = log.Mode == SessionMode.Single ? "Single" : "Multi",
                Duration = FormatDuration(duration),
                Charge = Math.Round(charge, 2)
            });

            timeCharge += charge;
        }

        receipt.TimeCharge = Math.Round(timeCharge, 2);
        foreach (var o in session.OrderItems)
        {
            receipt.OrderItems.Add(new ReceiptOrderItem
            {
                ReceiptId = receipt.Id,
                Name = o.Name,
                Price = o.Price,
                Quantity = o.Quantity
            });
        }
        receipt.OrdersTotal = Math.Round(session.OrderItems.Sum(o => o.Price * o.Quantity), 2);
        receipt.GrandTotal = Math.Round(receipt.TimeCharge + receipt.OrdersTotal, 2);

        return receipt;
    }

    private static string FormatDuration(TimeSpan ts) =>
        $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
}
