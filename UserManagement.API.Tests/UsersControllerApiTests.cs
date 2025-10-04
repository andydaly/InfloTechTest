using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UserManagement.Api.Controllers;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Data.Tests;

public class UsersControllerApiTests
{
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IUserLogService> _logService = new();

    private UsersController CreateController(string performerName = "Peter Loew", bool authenticated = true)
    {
        var sut = new UsersController(_userService.Object, _logService.Object);

        var identity = authenticated
            ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, performerName) }, "TestAuth")
            : new ClaimsIdentity();

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return sut;
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithUsers()
    {
        // Arrange
        var users = new[]
        {
            new User { Id = 1, Forename = "A", Surname = "One", Email = "a@example.com" },
            new User { Id = 2, Forename = "B", Surname = "Two", Email = "b@example.com" }
        }.AsEnumerable();

        _userService.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(users);

        var controller = CreateController();

        // Act
        var result = await controller.GetAll(CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsAssignableFrom<IEnumerable<User>>(ok.Value);

        payload.Select(u => u.Id).Should().BeEquivalentTo(new[] { 1L, 2L });
    }

    [Fact]
    public async Task Get_Found_ReturnsOk_AndLogsWithPerformer()
    {
        var user = new User { Id = 5, Forename = "Det", Surname = "Ails", Email = "det@example.com" };
        _userService.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var controller = CreateController("Peter Loew");

        var result = await controller.Get(5, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<User>(ok.Value);
        payload.Should().BeSameAs(user);

        _logService.Verify(l => l.LogAsync(
            5,
            UserActionType.Viewed,
            It.Is<string>(d => d.Contains("API viewed")),
            It.Is<string?>(p => p == "Peter Loew"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_Missing_ReturnsNotFound()
    {
        _userService.Setup(s => s.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);

        var controller = CreateController();

        var result = await controller.Get(99, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
        _logService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Filter_ReturnsOk_WithFilteredUsers()
    {
        var active = new[]
        {
            new User { Id = 10, Forename = "Act", Surname = "Ive", Email = "a@x.com", IsActive = true }
        }.AsEnumerable();

        _userService.Setup(s => s.FilterByActiveAsync(true, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(active);

        var controller = CreateController();

        var result = await controller.Filter(true, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsAssignableFrom<IEnumerable<User>>(ok.Value);

        payload.Should().ContainSingle(u => u.Id == 10 && u.IsActive);
    }

    [Fact]
    public async Task Create_ReturnsCreated_AndLogsWithPerformer()
    {
        var model = new User { Id = 0, Forename = "New", Surname = "User", Email = "new@example.com" };

        _userService.Setup(s => s.CreateAsync(model, It.IsAny<CancellationToken>()))
                    .Callback<User, CancellationToken>((u, _) => u.Id = 123)
                    .Returns(Task.CompletedTask);

        var controller = CreateController("Peter Loew");

        var result = await controller.Create(model, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(UsersController.Get), created.ActionName);
        Assert.Equal(123L, created.RouteValues!["id"]);
        Assert.Same(model, created.Value);

        _logService.Verify(l => l.LogAsync(
            123,
            UserActionType.Created,
            It.Is<string>(d => d.Contains("API created")),
            It.Is<string?>(p => p == "Peter Loew"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_IdMismatch_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.Update(9, new User { Id = 8 }, CancellationToken.None);

        Assert.IsType<BadRequestResult>(result);
        _userService.Verify(s => s.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _logService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        _userService.Setup(s => s.UpdateAsync(It.Is<User>(u => u.Id == 77), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        var controller = CreateController();

        var result = await controller.Update(77, new User { Id = 77 }, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
        _logService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Update_Ok_ReturnsNoContent_AndLogs()
    {
        var user = new User { Id = 77, Forename = "Up", Surname = "Date" };
        _userService.Setup(s => s.UpdateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var controller = CreateController("Peter Loew");

        var result = await controller.Update(77, user, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);

        _logService.Verify(l => l.LogAsync(
            77,
            UserActionType.Updated,
            It.Is<string>(d => d.Contains("API updated")),
            It.Is<string?>(p => p == "Peter Loew"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Missing_ReturnsNotFound()
    {
        _userService.Setup(s => s.GetByIdAsync(9, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((User?)null);

        var controller = CreateController();

        var result = await controller.Delete(9, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
        _userService.Verify(s => s.DeleteAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        _logService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Delete_Ok_ReturnsNoContent_AndLogs()
    {
        var existing = new User { Id = 9, Forename = "Del", Surname = "User" };
        _userService.Setup(s => s.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _userService.Setup(s => s.DeleteAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var controller = CreateController("Peter Loew");

        var result = await controller.Delete(9, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);

        _logService.Verify(l => l.LogAsync(
            9,
            UserActionType.Deleted,
            It.Is<string>(d => d.Contains("API deleted")),
            It.Is<string?>(p => p == "Peter Loew"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserLogs_ReturnsOk_WithItems()
    {
        var items = new[]
        {
            new UserLog { Id = 1, UserId = 7, Action = UserActionType.Viewed },
            new UserLog { Id = 2, UserId = 7, Action = UserActionType.Updated }
        }.AsEnumerable();

        _logService.Setup(l => l.GetForUserAsync(7, 10, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(items);

        var controller = CreateController();

        var result = await controller.GetUserLogs(7, 10, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsAssignableFrom<IEnumerable<UserLog>>(ok.Value);

        payload.Select(l => l.Id).Should().BeEquivalentTo(new[] { 1L, 2L });
    }
}
