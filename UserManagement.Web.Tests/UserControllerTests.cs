using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Users;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class UserControllerTests
{
    [Fact]
    public void List_WhenServiceReturnsUsers_ModelMustContainUsers()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var controller = CreateController();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = controller.List();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Model
            .Should().BeOfType<UserListViewModel>()
            .Which.Items.Should().BeEquivalentTo(users);
    }

    [Fact]
    public void List_WhenFilterIsActive_ModelMustContainOnlyActiveUsers()
    {
        // Arrange
        var controller = CreateController();
        var activeUsers = new[]
        {
            new User { Forename = "Alice", Surname = "A", Email = "a@example.com", IsActive = true },
            new User { Forename = "Cara",  Surname = "C", Email = "c@example.com", IsActive = true }
        };

        _userService.Setup(s => s.FilterByActive(true)).Returns(activeUsers);

        // Act
        var result = controller.List("active");

        // Assert
        result.Model.Should().BeOfType<UserListViewModel>().Which.Items.Should().BeEquivalentTo(activeUsers);
    }

    [Fact]
    public void List_WhenFilterIsInactive_ModelMustContainOnlyInactiveUsers()
    {
        // Arrange
        var controller = CreateController();
        var inactiveUsers = new[]
        {
            new User { Forename = "Bob",  Surname = "B", Email = "b@example.com", IsActive = false },
            new User { Forename = "Dane", Surname = "D", Email = "d@example.com", IsActive = false }
        };

        _userService.Setup(s => s.FilterByActive(false)).Returns(inactiveUsers);

        // Act
        var result = controller.List("inactive");

        // Assert
        result.Model.Should().BeOfType<UserListViewModel>().Which.Items.Should().BeEquivalentTo(inactiveUsers);
    }

    [Fact]
    public void Details_WhenUserExists_ReturnsViewWithModel()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 5, Forename = "Det", Surname = "Ails", Email = "det@example.com" };
        _userService.Setup(s => s.GetById(5)).Returns(user);

        // Act
        var result = controller.Details(5) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().BeOfType<UserDetailsViewModel>()
              .Which.User.Should().BeSameAs(user);
    }

    [Fact]
    public void Details_WhenUserMissing_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        _userService.Setup(s => s.GetById(99)).Returns((User?)null);

        // Act
        var result = controller.Details(99);

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
    public void Create_Post_InvalidModel_ReturnsSameView()
    {
        // Arrange
        var controller = CreateController();
        controller.ModelState.AddModelError("Forename", "Required");
        var model = new User();

        // Act
        var result = controller.Create(model);

        // Assert
        result.Should().BeOfType<ViewResult>()
              .Which.Model.Should().Be(model);
        _userService.Verify(s => s.Create(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Create_Post_Valid_RedirectsAndCallsService()
    {
        // Arrange
        var controller = CreateController();
        var model = new User { Forename = "New", Surname = "User", Email = "new@example.com" };

        // Act
        var result = controller.Create(model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.List));
        _userService.Verify(s => s.Create(model), Times.Once);
    }

    [Fact]
    public void Edit_Get_WhenFound_ReturnsView()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 3, Forename = "Ed", Surname = "It", Email = "edit@example.com" };
        _userService.Setup(s => s.GetById(3)).Returns(user);

        // Act
        var result = controller.Edit(3);

        // Assert
        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(user);
    }

    [Fact]
    public void Edit_Post_IdMismatch_ReturnsViewWithModelError()
    {
        // Arrange
        var controller = CreateController();
        var model = new User { Id = 2, Forename = "X" };

        // Act
        var result = controller.Edit(3, model);

        // Assert
        result.Should().BeOfType<ViewResult>().Which.Model.Should().Be(model);
        _userService.Verify(s => s.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Edit_Post_Valid_UpdatesAndRedirects()
    {
        // Arrange
        var controller = CreateController();
        var model = new User { Id = 3, Forename = "Ed", Surname = "It", Email = "edit@example.com" };
        _userService.Setup(s => s.Update(model)).Returns(true);

        // Act
        var result = controller.Edit(3, model);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(UsersController.List));
        _userService.Verify(s => s.Update(model), Times.Once);
    }

    [Fact]
    public void Edit_Post_WhenUpdateReturnsFalse_NotFound()
    {
        // Arrange
        var controller = CreateController();
        var model = new User { Id = 42, Forename = "Missing" };
        _userService.Setup(s => s.Update(model)).Returns(false);

        // Act
        var result = controller.Edit(42, model);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Delete_Get_WhenFound_ReturnsView()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 9, Forename = "Del", Surname = "User" };
        _userService.Setup(s => s.GetById(9)).Returns(user);

        // Act
        var result = controller.Delete(9);

        // Assert
        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(user);
    }

    [Fact]
    public void Delete_Post_Success_Redirects()
    {
        // Arrange
        var controller = CreateController();
        var user = new User { Id = 9, Forename = "Del", Surname = "User", Email = "del@example.com" };
        _userService.Setup(s => s.GetById(9)).Returns(user); 
        _userService.Setup(s => s.Delete(9)).Returns(true);

        // Act
        var result = controller.DeleteConfirmed(9);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>()
              .Which.ActionName.Should().Be(nameof(UsersController.List));
        _userService.Verify(s => s.Delete(9), Times.Once);
    }

    [Fact]
    public void Delete_Post_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        var controller = CreateController();
        _userService.Setup(s => s.Delete(9)).Returns(false);

        // Act
        var result = controller.DeleteConfirmed(9);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    private User[] SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
    {
        var users = new[]
        {
            new User
            {
                Forename = forename,
                Surname = surname,
                Email = email,
                IsActive = isActive
            }
        };

        _userService
            .Setup(s => s.GetAll())
            .Returns(users);

        return users;
    }

    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IUserLogService> _userLogService = new();
    private UsersController CreateController() => new(_userService.Object, _userLogService.Object);
}
