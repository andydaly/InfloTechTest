using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Controllers;
using UserManagement.Web.Models.Users;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public async Task List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange
        var controller = CreateController();
        var users = SetupUsers();
        _userService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(users);

        // Act
        var result = await controller.List();

        // Assert
        var expected = ToVm(users);
        result.Model.Should().BeOfType<UserListViewModel>().Which.Items.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task List_WhenFilterIsActive_ModelMustContainOnlyActiveUsers()
    {
        // Arrange
        var controller = CreateController();
        var activeUsers = new[]
        {
        new User { Forename = "Alice", Surname = "A", Email = "a@example.com", IsActive = true },
        new User { Forename = "Cara",  Surname = "C", Email = "c@example.com", IsActive = true }
    };
        _userService.Setup(s => s.FilterByActiveAsync(true, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activeUsers);

        // Act
        var result = await controller.List("active");

        // Assert
        var expected = ToVm(activeUsers);
        result.Model.Should().BeOfType<UserListViewModel>().Which.Items.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task List_WhenFilterIsInactive_ModelMustContainOnlyInactiveUsers()
    {
        // Arrange
        var controller = CreateController();
        var inactiveUsers = new[]
        {
            new User { Forename = "Bob",  Surname = "B", Email = "b@example.com", IsActive = false },
            new User { Forename = "Dane", Surname = "D", Email = "d@example.com", IsActive = false }
        };
        _userService.Setup(s => s.FilterByActiveAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync(inactiveUsers);

        // Act
        var result = await controller.List("inactive");

        // Assert
        var expected = ToVm(inactiveUsers);
        result.Model.Should().BeOfType<UserListViewModel>().Which.Items.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task Details_WhenUserExists_ReturnsViewWithModel()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 5, Forename = "Det", Surname = "Ails", Email = "det@example.com" };

        _userService.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        _userLogService.Setup(s => s.GetForUserAsync(5, 10, It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<UserLog>());

        // Act
        var result = await controller.Details(5) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().BeOfType<UserDetailsViewModel>().Which.User.Should().BeSameAs(user);
    }

    [Fact]
    public async Task Details_WhenUserMissing_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        _userService.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        // Act
        var result = await controller.Details(99);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.Create();

        // Assert
        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Post_InvalidModel_ReturnsSameView()
    {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("Forename", "Required");
        var model = new User();

        // Act
        var result = await controller.Create(model);

        // Assert
        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(model);
        _userService.Verify(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Create_Post_Valid_RedirectsAndCallsService()
    {
        // Arrange
        var controller = CreateController();
        var model = new User { Forename = "New", Surname = "User", Email = "new@example.com" };

        _userService.Setup(s => s.CreateAsync(model, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await controller.Create(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.List));
        _userService.Verify(s => s.CreateAsync(model, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Edit_Get_WhenFound_ReturnsView()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 3, Forename = "Ed", Surname = "It", Email = "edit@example.com" };
        _userService.Setup(s => s.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await controller.Edit(3);

        // Assert
        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(user);
    }

    [Fact]
    public async Task Edit_Post_IdMismatch_ReturnsViewWithModelError()
    {
        // Arrange
        var controller = CreateController();
        var model = new User { Id = 2, Forename = "X" };

        // Act
        var result = await controller.Edit(3, model);

        // Assert
        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(model);
        _userService.Verify(s => s.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Edit_Post_Valid_UpdatesAndRedirects()
    {
        // Arrange
        var controller = CreateController();
        var model = new User { Id = 3, Forename = "Ed", Surname = "It", Email = "edit@example.com" };

        _userService.Setup(s => s.UpdateAsync(model, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await controller.Edit(3, model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.List));
        _userService.Verify(s => s.UpdateAsync(model, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_WhenUpdateReturnsFalse_NotFound()
    {
        // Arrange
        var controller = CreateController();
        var model = new User { Id = 42, Forename = "Missing" };
        _userService.Setup(s => s.UpdateAsync(model, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await controller.Edit(42, model);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_Get_WhenFound_ReturnsView()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 9, Forename = "Del", Surname = "User" };
        _userService.Setup(s => s.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        // Act
        var result = await controller.Delete(9);

        // Assert
        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(user);
    }

    [Fact]
    public async Task Delete_Post_Success_Redirects()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 9, Forename = "Del", Surname = "User", Email = "del@example.com" };

        _userService.Setup(s => s.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userService.Setup(s => s.DeleteAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await controller.DeleteConfirmed(9);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.List));
        _userService.Verify(s => s.DeleteAsync(9, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Post_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();

        _userService.Setup(s => s.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Id = 9, Forename = "Del" });
        _userService.Setup(s => s.DeleteAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await controller.DeleteConfirmed(9);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    private static User[] SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true) =>
    new[]
    {
        new User { Forename = forename, Surname = surname, Email = email, IsActive = isActive }
    };

    private static List<UserListItemViewModel> ToVm(IEnumerable<User> users) =>
    users.Select(u => new UserListItemViewModel
    {
        Id = u.Id,
        Forename = u.Forename,
        Surname = u.Surname,
        Email = u.Email,
        IsActive = u.IsActive,
        DateOfBirth = u.DateOfBirth
    }).ToList();


    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IUserLogService> _userLogService = new();

    private UsersController CreateController() => new(_userService.Object, _userLogService.Object);
}
