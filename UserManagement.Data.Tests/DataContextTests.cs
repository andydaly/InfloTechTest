using System;
using System.Linq;
using UserManagement.Models;

namespace UserManagement.Data.Tests;

public class DataContextTests
{
    [Fact]
    public void GetAll_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();

        var entity = new User
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true
        };
        context.Create(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = context.GetAll<User>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result
            .Should().Contain(s => s.Email == entity.Email)
            .Which.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public void GetAll_WhenDeleted_MustNotIncludeDeletedEntity()
    {
        // Arrange: Initializes objects and sets the value of the data that is passed to the method under test.
        var context = CreateContext();
        var entity = context.GetAll<User>().First();
        context.Delete(entity);

        // Act: Invokes the method under test with the arranged parameters.
        var result = context.GetAll<User>();

        // Assert: Verifies that the action of the method under test behaves as expected.
        result.Should().NotContain(s => s.Email == entity.Email);
    }

    [Fact]
    public void Create_WhenUserHasDateOfBirth_MustPersistDateOfBirth()
    {
        // Arrange
        var context = CreateContext();
        var dob = new DateTime(1985, 5, 20);

        var entity = new User
        {
            Forename = "Jane",
            Surname = "Doe",
            Email = "janedoe@example.com",
            DateOfBirth = dob,
            IsActive = true
        };

        // Act
        context.Create(entity);
        var saved = context.GetAll<User>().First(u => u.Email == "janedoe@example.com");

        // Assert
        saved.DateOfBirth.Should().Be(dob);
    }

    [Fact]
    public void Update_WhenUserModified_MustPersistChanges()
    {
        // Arrange
        var context = CreateContext();
        var user = context.GetAll<User>().First();
        var originalSurname = user.Surname;

        // Act
        user.Surname = originalSurname + " Jr.";
        context.Update(user);

        // Assert
        var reloaded = context.GetAll<User>().First(u => u.Id == user.Id);
        reloaded.Surname.Should().Be(user.Surname);
    }

    [Fact]
    public void Create_And_Delete_RoundTrip_Works()
    {
        // Arrange
        var context = CreateContext();
        var email = "temp@example.com";
        var user = new User
        {
            Forename = "Temp",
            Surname = "User",
            Email = email,
            IsActive = false,
            DateOfBirth = new DateTime(1977, 7, 7)
        };

        // Act
        context.Create(user);
        context.GetAll<User>().Should().Contain(u => u.Email == email);

        context.Delete(user);

        // Assert
        context.GetAll<User>().Should().NotContain(u => u.Email == email);
    }


    private DataContext CreateContext() => new();
}
