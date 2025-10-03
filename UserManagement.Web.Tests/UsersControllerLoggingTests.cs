using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Controllers;

namespace UserManagement.Data.Tests;

public class UsersControllerLoggingTests
{
    [Fact]
    public async Task Details_LogsViewed()
    {
        // Arrange
        var userSvc = new Mock<IUserService>();
        var logSvc = new Mock<IUserLogService>();
        var user = new User { Id = 5, Forename = "V", Surname = "Iew" };

        userSvc.Setup(s => s.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        logSvc.Setup(s => s.GetForUserAsync(5, 10, It.IsAny<CancellationToken>())).ReturnsAsync(Enumerable.Empty<UserLog>());

        var sut = new UsersController(userSvc.Object, logSvc.Object);

        // Act
        var result = await sut.Details(5);

        // Assert
        result.Should().BeOfType<ViewResult>();
        logSvc.Verify(l => l.LogAsync(5, UserActionType.Viewed, It.Is<string>(d => d.Contains("Viewed")),  null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_Post_Valid_LogsCreated()
    {
        // Arrange
        var userSvc = new Mock<IUserService>();
        var logSvc = new Mock<IUserLogService>();
        var model = new User { Id = 0, Forename = "New", Surname = "User", Email = "new@example.com" };

        userSvc.Setup(s => s.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>())).Callback<User, CancellationToken>((u, _) => u.Id = 123).Returns(Task.CompletedTask);

        var sut = new UsersController(userSvc.Object, logSvc.Object);

        // Act
        var result = await sut.Create(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        logSvc.Verify(l => l.LogAsync(123, UserActionType.Created, It.Is<string>(d => d.Contains("Created")), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_Valid_LogsUpdated()
    {
        // Arrange
        var userSvc = new Mock<IUserService>();
        var logSvc = new Mock<IUserLogService>();
        var model = new User { Id = 7, Forename = "Ed", Surname = "It", Email = "ed@example.com" };

        userSvc.Setup(s => s.UpdateAsync(model, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = new UsersController(userSvc.Object, logSvc.Object);

        // Act
        var result = await sut.Edit(7, model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        logSvc.Verify(l => l.LogAsync(7, UserActionType.Updated, It.Is<string>(d => d.Contains("Updated")), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteConfirmed_LogsDeleted()
    {
        // Arrange
        var userSvc = new Mock<IUserService>();
        var logSvc = new Mock<IUserLogService>();
        var user = new User { Id = 9, Forename = "Del", Surname = "User" };

        userSvc.Setup(s => s.GetByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        userSvc.Setup(s => s.DeleteAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var sut = new UsersController(userSvc.Object, logSvc.Object);

        // Act
        var result = await sut.DeleteConfirmed(9);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        logSvc.Verify(l => l.LogAsync(9, UserActionType.Deleted, It.Is<string>(d => d.Contains("Deleted")), null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
