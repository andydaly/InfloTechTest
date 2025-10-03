using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;
using UserManagement.Services.Implementations;

namespace UserManagement.Data.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenContextHasEntities_ReturnsThem()
    {
        using var ctx = CreateContext();
        ctx.Users!.AddRange(
            new User { Id = 1, Forename = "A", Surname = "One", Email = "a1@example.com", IsActive = true, Password = "pw1" },
            new User { Id = 2, Forename = "B", Surname = "Two", Email = "b2@example.com", IsActive = false, Password = "pw2" }
        );
        await ctx.SaveChangesAsync();

        var sut = new UserService(ctx);

        var result = await sut.GetAllAsync();

        result.Select(u => u.Email)
              .Should().BeEquivalentTo("a1@example.com", "b2@example.com");
    }

    [Fact]
    public async Task FilterByActiveAsync_WhenTrue_ReturnsOnlyActive()
    {
        using var ctx = CreateContext();
        ctx.Users!.AddRange(
            new User { Id = 1, Forename = "A", Surname = "A", Email = "a@example.com", IsActive = true, Password = "pw1" },
            new User { Id = 2, Forename = "B", Surname = "B", Email = "b@example.com", IsActive = false, Password = "pw2" },
            new User { Id = 3, Forename = "C", Surname = "C", Email = "c@example.com", IsActive = true, Password = "pw3" }
        );
        await ctx.SaveChangesAsync();

        var sut = new UserService(ctx);

        var result = (await sut.FilterByActiveAsync(true)).ToList();

        result.Should().HaveCount(2).And.OnlyContain(u => u.IsActive);
    }

    [Fact]
    public async Task FilterByActiveAsync_WhenFalse_ReturnsOnlyInactive()
    {
        using var ctx = CreateContext();
        ctx.Users!.AddRange(
            new User { Id = 1, Forename = "A", Surname = "A", Email = "a@example.com", IsActive = true, Password = "pw1" },
            new User { Id = 2, Forename = "B", Surname = "B", Email = "b@example.com", IsActive = false, Password = "pw2" },
            new User { Id = 3, Forename = "D", Surname = "D", Email = "d@example.com", IsActive = false, Password = "pw3" }
        );
        await ctx.SaveChangesAsync();

        var sut = new UserService(ctx);

        var result = (await sut.FilterByActiveAsync(false)).ToList();

        result.Should().HaveCount(2).And.OnlyContain(u => !u.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsUser()
    {
        using var ctx = CreateContext();
        ctx.Users!.AddRange(
            new User { Id = 1, Forename = "A", Surname = "A", Email = "a@example.com", IsActive = true, Password = "pw1" },
            new User { Id = 2, Forename = "B", Surname = "B", Email = "b@example.com", IsActive = false, Password = "pw2" }
        );
        await ctx.SaveChangesAsync();

        var sut = new UserService(ctx);

        var user = await sut.GetByIdAsync(2);

        user.Should().NotBeNull();
        user!.Email.Should().Be("b@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNull()
    {
        using var ctx = CreateContext();
        var sut = new UserService(ctx);

        var user = await sut.GetByIdAsync(999);

        user.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_PersistsUser()
    {
        using var ctx = CreateContext();
        var sut = new UserService(ctx);

        var u = new User
        {
            Forename = "N",
            Surname = "N",
            Email = "n@example.com",
            IsActive = true,
            Password = "testpass1"
        };

        await sut.CreateAsync(u);

        (await ctx.Users!.AnyAsync(x => x.Email == "n@example.com")).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_UpdatesAndReturnsTrue()
    {
        using var ctx = CreateContext();
        var existing = new User
        {
            Id = 1,
            Forename = "Old",
            Surname = "Name",
            Email = "old@example.com",
            IsActive = true,
            Password = "oldpass"
        };
        ctx.Users!.Add(existing);
        await ctx.SaveChangesAsync();

        var sut = new UserService(ctx);

        var updated = new User
        {
            Id = 1,
            Forename = "New",
            Surname = "Person",
            Email = "new@example.com",
            IsActive = false,
            Password = "newpass"
        };

        var ok = await sut.UpdateAsync(updated);

        ok.Should().BeTrue();
        var reloaded = await ctx.Users!.FindAsync(1L);
        reloaded!.Forename.Should().Be("New");
        reloaded.Surname.Should().Be("Person");
        reloaded.Email.Should().Be("new@example.com");
        reloaded.IsActive.Should().BeFalse();
        reloaded.Password.Should().Be("newpass");
    }

    [Fact]
    public async Task UpdateAsync_WhenMissing_ReturnsFalse()
    {
        using var ctx = CreateContext();
        var sut = new UserService(ctx);

        var ok = await sut.UpdateAsync(new User { Id = 42, Forename = "X", Surname = "Y", Email = "z@example.com", Password = "pw" });

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_RemovesAndReturnsTrue()
    {
        using var ctx = CreateContext();

        var existing = new User
        {
            Id = 10,
            Forename = "Del",
            Surname = "User",
            Email = "del@example.com",
            Password = "testpass1" 
        };

        ctx.Users!.Add(existing);
        await ctx.SaveChangesAsync();

        var sut = new UserService(ctx);

        var ok = await sut.DeleteAsync(10);

        ok.Should().BeTrue();
        (await ctx.Users!.AnyAsync(u => u.Id == 10)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenMissing_ReturnsFalse()
    {
        using var ctx = CreateContext(); 
        var sut = new UserService(ctx);

        var ok = await sut.DeleteAsync(10);

        ok.Should().BeFalse();
    }

    private static DataContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: $"Tests-{Guid.NewGuid()}")
            .Options;

        var ctx = new DataContext(options);
        ctx.Database.EnsureCreated();

        if (ctx.Users is not null && ctx.Users.Any())
            ctx.Users.RemoveRange(ctx.Users);
        if (ctx.UserLogs is not null && ctx.UserLogs.Any())
            ctx.UserLogs.RemoveRange(ctx.UserLogs);

        ctx.SaveChanges();
        return ctx;
    }
}
