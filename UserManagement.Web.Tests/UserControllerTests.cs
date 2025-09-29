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
    private UsersController CreateController() => new(_userService.Object);
}
