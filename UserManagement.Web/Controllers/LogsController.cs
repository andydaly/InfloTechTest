using System.Linq;
using System.Threading.Tasks;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models.Logs;

namespace UserManagement.Web.Controllers;

[Route("logs")]
public class LogsController : Controller
{
    private readonly IUserLogService _logs;

    public LogsController(IUserLogService logs)
    {
        _logs = logs;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 25, string? q = null)
    {
        var (items, total) = await _logs.GetAllAsync(page, pageSize, q);

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
    public async Task<IActionResult> Details(long id)
    {
        var log = await _logs.GetByIdAsync(id);
        if (log is null) return NotFound();
        return View(log);
    }
}
