using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.API.Models;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/logs")]
public class LogsController : ControllerBase
{
    private readonly IUserLogService _logs;

    public LogsController(IUserLogService logs)
    {
        _logs = logs;
    }


    [HttpGet]
    public async Task<ActionResult<LogListResponse>> GetAll(int page = 1, int pageSize = 25, string? q = null,
        CancellationToken ct = default)
    {
        var (items, total) = await _logs.GetAllAsync(page, pageSize, q, ct);
        var response = new LogListResponse
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
            Query = q
        };
        return Ok(response);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserLog>> GetById(long id, CancellationToken ct)
    {
        var entry = await _logs.GetByIdAsync(id, ct);
        return entry is null ? NotFound() : Ok(entry);
    }
}


