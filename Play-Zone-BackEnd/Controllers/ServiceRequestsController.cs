using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Play_Zone_BackEnd.Data;
using Play_Zone_BackEnd.Models;

namespace Play_Zone_BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceRequestsController : ControllerBase
{
    private readonly AppDbContext _db;
    public ServiceRequestsController(AppDbContext db) => _db = db;

    [HttpGet("pending")]
    public async Task<ActionResult<List<ServiceRequest>>> GetPending()
    {
        var requests = await _db.ServiceRequests
            .Where(r => r.Status == "Pending")
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return Ok(requests);
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceRequest>>> GetAll()
    {
        var requests = await _db.ServiceRequests
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return Ok(requests);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequest>> Create([FromBody] CreateServiceRequest request)
    {
        var req = new ServiceRequest
        {
            SessionId = request.SessionId,
            DeviceId = request.DeviceId,
            DeviceName = request.DeviceName,
            RequestType = request.RequestType,
            Description = request.Description,
        };
        _db.ServiceRequests.Add(req);
        await _db.SaveChangesAsync();
        return Ok(req);
    }

    [HttpPut("{id}/accept")]
    public async Task<ActionResult> Accept(string id)
    {
        var req = await _db.ServiceRequests.FindAsync(id);
        if (req == null) return NotFound();
        req.Status = "Accepted";
        await _db.SaveChangesAsync();
        return Ok(req);
    }

    [HttpPut("{id}/complete")]
    public async Task<ActionResult> Complete(string id)
    {
        var req = await _db.ServiceRequests.FindAsync(id);
        if (req == null) return NotFound();
        req.Status = "Completed";
        await _db.SaveChangesAsync();
        return Ok(req);
    }
}

public class CreateServiceRequest
{
    public string? SessionId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string RequestType { get; set; } = "CallStaff";
    public string Description { get; set; } = string.Empty;
}