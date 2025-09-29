using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Data.Tests;

public class UserServiceTests
{
    [Fact]
    public void GetAll_WhenContextReturnsEntities_MustReturnSameEntities()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var service = CreateService();
        var users = SetupUsers();

        // Act: Invokes the method under test with the arranged parameters.
        var result = service.GetAll();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().BeSameAs(users);
    }

    [Fact]
    public void FilterByActive_WhenTrue_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var service = CreateService();
        var users = new[]
        {
            new User { Forename = "Alice", Surname = "A", Email = "a@example.com", IsActive = true },
            new User { Forename = "Bob",   Surname = "B", Email = "b@example.com", IsActive = false },
            new User { Forename = "Cara",  Surname = "C", Email = "c@example.com", IsActive = true }
        }.AsQueryable();

        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        var result = service.FilterByActive(true).ToList();

        // Assert
        result.Should().OnlyContain(u => u.IsActive).And.HaveCount(2);
        result.Select(u => u.Email).Should().BeEquivalentTo(new[] { "a@example.com", "c@example.com" });
    }

    [Fact]
    public void FilterByActive_WhenFalse_ReturnsOnlyInactiveUsers()
    {
        // Arrange
        var service = CreateService();
        var users = new[]
        {
            new User { Forename = "Alice", Surname = "A", Email = "a@example.com", IsActive = true },
            new User { Forename = "Bob",   Surname = "B", Email = "b@example.com", IsActive = false },
            new User { Forename = "Dane",  Surname = "D", Email = "d@example.com", IsActive = false }
        }.AsQueryable();

        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        var result = service.FilterByActive(false).ToList();

        // Assert
        result.Should().OnlyContain(u => !u.IsActive).And.HaveCount(2);
        result.Select(u => u.Email).Should().BeEquivalentTo(new[] { "b@example.com", "d@example.com" });
    }

    private IQueryable<User> SetupUsers(string forename = "Johnny", string surname = "User", string email = "juser@example.com", bool isActive = true)
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
        }.AsQueryable();

        _dataContext
            .Setup(s => s.GetAll<User>())
            .Returns(users);

        return users;
    }

    private readonly Mock<IDataContext> _dataContext = new();
    private UserService CreateService() => new(_dataContext.Object);
}
