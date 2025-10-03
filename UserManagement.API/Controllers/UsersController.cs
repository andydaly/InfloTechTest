using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _svc;
    private readonly IUserLogService _logs;
    public UsersController(IUserService svc, IUserLogService logs)
    {
        _svc = svc;
        _logs = logs;
    }

    private string? Performer => User?.Identity?.IsAuthenticated == true ? User.Identity!.Name : null;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll(CancellationToken ct)
        => Ok(await _svc.GetAllAsync(ct));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<User>> Get(long id, CancellationToken ct)
    {
        var user = await _svc.GetByIdAsync(id, ct);
        if (user is null) return NotFound();

        await _logs.LogAsync(user.Id, UserActionType.Viewed, $"API viewed {user.Forename} {user.Surname}", Performer, ct);
        return Ok(user);
    }

    [HttpGet("active/{isActive:bool}")]
    public async Task<ActionResult<IEnumerable<User>>> Filter(bool isActive, CancellationToken ct)
        => Ok(await _svc.FilterByActiveAsync(isActive, ct));

    [HttpPost]
    public async Task<ActionResult<User>> Create(User user, CancellationToken ct)
    {
        await _svc.CreateAsync(user, ct); 
        await _logs.LogAsync(user.Id, UserActionType.Created, $"API created {user.Forename} {user.Surname}", Performer, ct);
        return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, User user, CancellationToken ct)
    {
        if (id != user.Id) return BadRequest();

        var ok = await _svc.UpdateAsync(user, ct);
        if (!ok) return NotFound();

        await _logs.LogAsync(user.Id, UserActionType.Updated, $"API updated {user.Forename} {user.Surname}", Performer, ct);
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        var existing = await _svc.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        await _logs.LogAsync(existing.Id, UserActionType.Deleted, $"API deleted {existing.Forename} {existing.Surname}", Performer, ct);

        var ok = await _svc.DeleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("{id:long}/logs")]
    public async Task<ActionResult<IEnumerable<UserLog>>> GetUserLogs(long id, int take = 10, CancellationToken ct = default)
        => Ok(await _logs.GetForUserAsync(id, take, ct));
}
