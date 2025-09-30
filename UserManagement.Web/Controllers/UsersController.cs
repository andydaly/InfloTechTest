using System;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;

namespace UserManagement.WebMS.Controllers;

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
    public ViewResult List(string filter = "all")
    {
        var users = filter?.ToLowerInvariant() switch
        {
            "active" => _userService.FilterByActive(true),
            "inactive" => _userService.FilterByActive(false),
            _ => _userService.GetAll()
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
    public IActionResult Create(User model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _userService.Create(model);
        _logService.Log(model.Id, UserActionType.Created, $"Created {model.Forename} {model.Surname}");
        return RedirectToAction(nameof(List));
    }

    [HttpGet("details/{id:long}")]
    public IActionResult Details(long id)
    {
        var user = _userService.GetById(id);
        if (user is null) return NotFound();

        _logService.Log(user.Id, UserActionType.Viewed, $"Viewed {user.Forename} {user.Surname}");

        var vm = new UserDetailsViewModel
        {
            User = user,
            RecentLogs = _logService.GetForUser(user.Id, 10).ToList()
        };
        return View(vm);
    }

    [HttpGet("edit/{id:long}")]
    public IActionResult Edit(long id)
    {
        var user = _userService.GetById(id);
        if (user is null) return NotFound();
        return View(user);
    }

    [HttpPost("edit/{id:long}")]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(long id, User model)
    {
        if (id != model.Id)
        {
            ModelState.AddModelError("", "Mismatched user id.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var ok = _userService.Update(model);
        if (!ok) return NotFound();

        _logService.Log(model.Id, UserActionType.Updated, $"Updated {model.Forename} {model.Surname}");
        return RedirectToAction(nameof(List));
    }

    [HttpGet("delete/{id:long}")]
    public IActionResult Delete(long id)
    {
        var user = _userService.GetById(id);
        if (user is null) return NotFound();
        return View(user);
    }

    [HttpPost("delete/{id:long}")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(long id)
    {
        var user = _userService.GetById(id);
        if (user is null) return NotFound();

        _logService.Log(user.Id, UserActionType.Deleted, $"Deleted {user.Forename} {user.Surname}");
        var ok = _userService.Delete(id);
        if (!ok) return NotFound();

        return RedirectToAction(nameof(List));
    }
}
