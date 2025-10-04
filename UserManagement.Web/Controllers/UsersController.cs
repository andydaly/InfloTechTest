using System;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models.Users;

namespace UserManagement.Web.Controllers;

[Route("users")]
public class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IUserLogService _logService;

    public UsersController(IUserService userService, IUserLogService logService)
    {
        _userService = userService;
        _logService = logService;
    }

    [HttpGet]
    public async Task<ViewResult> List(string filter = "all")
    {
        var users = filter?.ToLowerInvariant() switch
        {
            "active" => await _userService.FilterByActiveAsync(true),
            "inactive" => await _userService.FilterByActiveAsync(false),
            _ => await _userService.GetAllAsync()
        };

        var items = users.Select(p => new UserListItemViewModel
        {
            Id = p.Id,
            Forename = p.Forename,
            Surname = p.Surname,
            Email = p.Email,
            IsActive = p.IsActive,
            DateOfBirth = p.DateOfBirth
        });

        var model = new UserListViewModel
        {
            Items = items.ToList(),
            CurrentFilter = filter ?? "all"
        };

        return View(model);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        var model = new User
        {
            IsActive = true,
            DateOfBirth = new DateTime(1990, 1, 1)
        };
        return View(model);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _userService.CreateAsync(model);
        await _logService.LogAsync(model.Id, UserActionType.Created, $"Created {model.Forename} {model.Surname}");
        return RedirectToAction(nameof(List));
    }

    [HttpGet("details/{id:long}")]
    public async Task<IActionResult> Details(long id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null) return NotFound();

        await _logService.LogAsync(user.Id, UserActionType.Viewed, $"Viewed {user.Forename} {user.Surname}");

        var vm = new UserDetailsViewModel
        {
            User = user,
            RecentLogs = (await _logService.GetForUserAsync(user.Id, 10)).ToList()
        };
        return View(vm);
    }

    [HttpGet("edit/{id:long}")]
    public async Task<IActionResult> Edit(long id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null) return NotFound();
        return View(user);
    }

    [HttpPost("edit/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, User model)
    {
        if (id != model.Id)
        {
            ModelState.AddModelError("", "Mismatched user id.");
        }

        ModelState.Remove(nameof(UserManagement.Data.Entities.User.Password));

        if (!ModelState.IsValid)
            return View(model);

        var ok = await _userService.UpdateAsync(model);
        if (!ok) return NotFound();

        await _logService.LogAsync(model.Id, UserActionType.Updated, $"Updated {model.Forename} {model.Surname}");
        return RedirectToAction(nameof(List));
    }

    [HttpGet("delete/{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null) return NotFound();
        return View(user);
    }

    [HttpPost("delete/{id:long}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user is null) return NotFound();

        await _logService.LogAsync(user.Id, UserActionType.Deleted, $"Deleted {user.Forename} {user.Surname}");
        var ok = await _userService.DeleteAsync(id);
        if (!ok) return NotFound();

        return RedirectToAction(nameof(List));
    }
}
