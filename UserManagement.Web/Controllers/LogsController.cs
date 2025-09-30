using System.Linq;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Logs;

namespace UserManagement.WebMS.Controllers;

[Route("logs")]
public class LogsController : Controller
{
    private readonly IUserLogService _logs;

    public LogsController(IUserLogService logs) => _logs = logs;

    [HttpGet("")]
    public IActionResult Index(int page = 1, int pageSize = 25, string? q = null)
    {
        var (items, total) = _logs.GetAll(page, pageSize, q);

        var vm = new LogListViewModel
        {
            Items = items.Select(l => new LogListItemViewModel
            {
                Id = l.Id,
                UserId = l.UserId,
                Action = l.Action,
                OccurredAt = l.OccurredAt,
                PerformedBy = l.PerformedBy,
                Details = l.Details
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total,
            Query = q
        };

        return View(vm);
    }

    [HttpGet("{id:long}")]
    public IActionResult Details(long id)
    {
        var log = _logs.GetById(id);
        if (log is null) return NotFound();
        return View(log);
    }
}
