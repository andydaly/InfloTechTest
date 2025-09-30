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

    [Fact]
    public void GetById_WhenEntityExists_ReturnsEntity()
    {
        // Arrange
        var service = CreateService();
        var users = new[]
        {
            new User { Id = 1, Forename = "A", Surname = "A", Email = "a@example.com", IsActive = true },
            new User { Id = 2, Forename = "B", Surname = "B", Email = "b@example.com", IsActive = false }
        }.AsQueryable();
        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        var result = service.GetById(2);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("b@example.com");
    }

    [Fact]
    public void GetById_WhenEntityMissing_ReturnsNull()
    {
        // Arrange
        var service = CreateService();
        var users = new[]
        {
            new User { Id = 1, Forename = "A", Surname = "A", Email = "a@example.com", IsActive = true }
        }.AsQueryable();
        _dataContext.Setup(s => s.GetAll<User>()).Returns(users);

        // Act
        var result = service.GetById(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Create_CallsDataContextCreate()
    {
        // Arrange
        var service = CreateService();
        var user = new User { Forename = "N", Surname = "N", Email = "n@example.com", IsActive = true };

        // Act
        service.Create(user);

        // Assert
        _dataContext.Verify(dc => dc.Create(user), Times.Once);
    }

    [Fact]
    public void Update_WhenEntityExists_UpdatesAndReturnsTrue()
    {
        // Arrange
        var service = CreateService();
        var existing = new User { Id = 1, Forename = "A", Surname = "A", Email = "a@example.com", IsActive = true };
        _dataContext.Setup(s => s.GetAll<User>()).Returns(new[] { existing }.AsQueryable());

        var updated = new User { Id = 1, Forename = "Alice", Surname = "Anderson", Email = "alice@example.com", IsActive = false };

        // Act
        var ok = service.Update(updated);

        // Assert
        ok.Should().BeTrue();
        _dataContext.Verify(dc => dc.Update(It.Is<User>(u =>
            u.Id == 1 &&
            u.Forename == "Alice" &&
            u.Surname == "Anderson" &&
            u.Email == "alice@example.com" &&
            u.IsActive == false
        )), Times.Once);
    }

    [Fact]
    public void Update_WhenEntityMissing_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        _dataContext.Setup(s => s.GetAll<User>()).Returns(Enumerable.Empty<User>().AsQueryable());

        var updated = new User { Id = 42, Forename = "X", Surname = "Y", Email = "z@example.com" };

        // Act
        var ok = service.Update(updated);

        // Assert
        ok.Should().BeFalse();
        _dataContext.Verify(dc => dc.Update(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Delete_WhenEntityExists_RemovesAndReturnsTrue()
    {
        // Arrange
        var service = CreateService();
        var existing = new User { Id = 10, Forename = "Del", Surname = "User", Email = "del@example.com" };
        _dataContext.Setup(s => s.GetAll<User>()).Returns(new[] { existing }.AsQueryable());

        // Act
        var ok = service.Delete(10);

        // Assert
        ok.Should().BeTrue();
        _dataContext.Verify(dc => dc.Delete(existing), Times.Once);
    }

    [Fact]
    public void Delete_WhenEntityMissing_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        _dataContext.Setup(s => s.GetAll<User>()).Returns(Enumerable.Empty<User>().AsQueryable());

        // Act
        var ok = service.Delete(10);

        // Assert
        ok.Should().BeFalse();
        _dataContext.Verify(dc => dc.Delete(It.IsAny<User>()), Times.Never);
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
