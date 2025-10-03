using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;

namespace UserManagement.Data.Tests;

public class DataContextTests
{
    [Fact]
    public async Task GetAll_WhenNewEntityAdded_MustIncludeNewEntity()
    {
        // Arrange
        var context = CreateContext();

        var entity = new User
        {
            Forename = "Brand New",
            Surname = "User",
            Email = "brandnewuser@example.com",
            DateOfBirth = new DateTime(1990, 1, 1),
            IsActive = true,
            Password = "mypassword1"
        };

        await context.CreateAsync(entity);

        // Act
        var result = context.GetAll<User>().ToList();

        // Assert
        result.Should().Contain(s => s.Email == entity.Email).Which.Should().BeEquivalentTo(entity);
    }

    [Fact]
    public async Task GetAll_WhenDeleted_MustNotIncludeDeletedEntity()
    {
        // Arrange
        var context = CreateContext();
        var entity = context.GetAll<User>().First();

        await context.DeleteAsync(entity);

        // Act
        var result = context.GetAll<User>().ToList();

        // Assert
        result.Should().NotContain(s => s.Email == entity.Email);
    }

    [Fact]
    public async Task Create_WhenUserHasDateOfBirth_MustPersistDateOfBirth()
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
            IsActive = true,
            Password = "mypassword1"
        };

        // Act
        await context.CreateAsync(entity);
        var saved = context.GetAll<User>().First(u => u.Email == "janedoe@example.com");

        // Assert
        saved.DateOfBirth.Should().Be(dob);
    }

    [Fact]
    public async Task Create_WhenUserHasPassword_MustPersistPassword()
    {
        // Arrange
        var context = CreateContext();

        var entity = new User
        {
            Forename = "Paula",
            Surname = "Word",
            Email = "paulaword@example.com",
            IsActive = true,
            DateOfBirth = new DateTime(1992, 2, 2),
            Password = "mypassword1"
        };

        // Act
        await context.CreateAsync(entity);
        var saved = context.GetAll<User>().First(u => u.Email == entity.Email);

        // Assert
        saved.Password.Should().Be("mypassword1");
    }

    [Fact]
    public async Task Update_WhenUserModified_MustPersistChanges()
    {
        // Arrange
        var context = CreateContext();
        var user = context.GetAll<User>().First();
        var originalSurname = user.Surname;

        // Act
        user.Surname = originalSurname + " Jr.";
        await context.UpdateAsync(user);

        // Assert
        var reloaded = context.GetAll<User>().First(u => u.Id == user.Id);
        reloaded.Surname.Should().Be(user.Surname);
    }

    [Fact]
    public async Task Create_And_Delete_RoundTrip_Works()
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
            DateOfBirth = new DateTime(1977, 7, 7),
            Password = "mypassword1"
        };

        // Act
        await context.CreateAsync(user);
        context.GetAll<User>().Should().Contain(u => u.Email == email);

        await context.DeleteAsync(user);

        // Assert
        context.GetAll<User>().Should().NotContain(u => u.Email == email);
    }

    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: $"Tests-{Guid.NewGuid()}")
            .Options;

        var ctx = new DataContext(options);
        ctx.Database.EnsureCreated(); 
        return ctx;
    }
}
