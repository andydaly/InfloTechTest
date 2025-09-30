using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UsersControllerLoggingTests
{
    [Fact]
    public void Details_LogsViewed()
    {
        // Arrange
        var userSvc = new Mock<IUserService>();
        var logSvc = new Mock<IUserLogService>();
        var user = new User { Id = 5, Forename = "V", Surname = "Iew" };
        userSvc.Setup(s => s.GetById(5)).Returns(user);
        var sut = new UsersController(userSvc.Object, logSvc.Object);

        // Act
        var result = sut.Details(5);

        // Assert
        result.Should().BeOfType<ViewResult>();
        logSvc.Verify(l => l.Log(5, UserActionType.Viewed, It.Is<string>(d => d.Contains("Viewed")), null), Times.Once);
    }

    [Fact]
    public void Create_Post_Valid_LogsCreated()
    {
        // Arrange
        var userSvc = new Mock<IUserService>();
        var logSvc = new Mock<IUserLogService>();
        var model = new User { Id = 0, Forename = "New", Surname = "User", Email = "new@example.com" };

        // Simulate DB setting the new Id inside Create
        userSvc.Setup(s => s.Create(It.IsAny<User>()))
               .Callback<User>(u => u.Id = 123);

        var sut = new UsersController(userSvc.Object, logSvc.Object);

        // Act
        var result = sut.Create(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        logSvc.Verify(l => l.Log(123, UserActionType.Created, It.Is<string>(d => d.Contains("Created")), null), Times.Once);
    }

    [Fact]
    public void Edit_Post_Valid_LogsUpdated()
    {
        // Arrange
        var userSvc = new Mock<IUserService>();
        var logSvc = new Mock<IUserLogService>();
        var model = new User { Id = 7, Forename = "Ed", Surname = "It", Email = "ed@example.com" };

        userSvc.Setup(s => s.Update(model)).Returns(true);
        var sut = new UsersController(userSvc.Object, logSvc.Object);

        // Act
        var result = sut.Edit(7, model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        logSvc.Verify(l => l.Log(7, UserActionType.Updated, It.Is<string>(d => d.Contains("Updated")), null), Times.Once);
    }

    [Fact]
    public void DeleteConfirmed_LogsDeleted()
    {
        // Arrange
        var userSvc = new Mock<IUserService>();
        var logSvc = new Mock<IUserLogService>();
        var user = new User { Id = 9, Forename = "Del", Surname = "User" };

        userSvc.Setup(s => s.GetById(9)).Returns(user);
        userSvc.Setup(s => s.Delete(9)).Returns(true);

        var sut = new UsersController(userSvc.Object, logSvc.Object);

        // Act
        var result = sut.DeleteConfirmed(9);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        logSvc.Verify(l => l.Log(9, UserActionType.Deleted, It.Is<string>(d => d.Contains("Deleted")), null), Times.Once);
    }
}
