using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data.Entities;
using UserManagement.Services.Implementations;

namespace UserManagement.Data.Tests;

public class UserLogServiceTests
{
    [Fact]
    public async Task LogAsync_WritesEntry()
    {
        using var ctx = CreateContext();
        var sut = new UserLogService(ctx);

        await sut.LogAsync(42, UserActionType.Created, "Created record", "tester@example.com");

        var log = await ctx.UserLogs!.SingleAsync();
        log.UserId.Should().Be(42);
        log.Action.Should().Be(UserActionType.Created);
        log.Details.Should().Be("Created record");
        log.PerformedBy.Should().Be("tester@example.com");
    }

    [Fact]
    public async Task GetForUserAsync_ReturnsNewestFirst_AndHonorsTake()
    {
        using var ctx = CreateContext();
        var uid = 7L;
        ctx.UserLogs!.AddRange(
            new UserLog { Id = 1, UserId = uid, Action = UserActionType.Viewed, OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-10) },
            new UserLog { Id = 2, UserId = uid, Action = UserActionType.Updated, OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-5) },
            new UserLog { Id = 3, UserId = uid, Action = UserActionType.Created, OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-1) },
            new UserLog { Id = 4, UserId = 999, Action = UserActionType.Created, OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-1) }
        );
        await ctx.SaveChangesAsync();

        var sut = new UserLogService(ctx);

        var items = (await sut.GetForUserAsync(uid, take: 2)).ToList();

        items.Select(i => i.Id).Should().ContainInOrder(3, 2);
    }

    [Fact]
    public async Task GetAllAsync_Paginates_And_Searches()
    {
        using var ctx = CreateContext();
        ctx.UserLogs!.AddRange(
            new UserLog { Id = 1, UserId = 1, Action = UserActionType.Created, Details = "Seed user", PerformedBy = "system", OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-30) },
            new UserLog { Id = 2, UserId = 2, Action = UserActionType.Updated, Details = "Changed email", PerformedBy = "bob", OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-20) },
            new UserLog { Id = 3, UserId = 2, Action = UserActionType.Viewed, Details = "Viewed profile", PerformedBy = "bob", OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-10) },
            new UserLog { Id = 4, UserId = 3, Action = UserActionType.Deleted, Details = "Deleted account", PerformedBy = "alice", OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-5) },
            new UserLog { Id = 5, UserId = 42, Action = UserActionType.Created, Details = "Created 42", PerformedBy = "qa", OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-1) }
        );
        await ctx.SaveChangesAsync();

        var sut = new UserLogService(ctx);

        var (page1, total1) = await sut.GetAllAsync(page: 1, pageSize: 2, query: null);
        page1.Select(i => i.Id).Should().ContainInOrder(5, 4);
        total1.Should().Be(5);

        var (page2, total2) = await sut.GetAllAsync(page: 2, pageSize: 2, query: null);
        page2.Select(i => i.Id).Should().ContainInOrder(3, 2);
        total2.Should().Be(5);

        var (search, totalQ) = await sut.GetAllAsync(page: 1, pageSize: 10, query: "BoB");
        search.Should().OnlyContain(l => (l.PerformedBy ?? "").ToLower().Contains("bob"));
        totalQ.Should().Be(2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMatchOrNull()
    {
        using var ctx = CreateContext();
        ctx.UserLogs!.AddRange(
            new UserLog { Id = 10, UserId = 1, Action = UserActionType.Created },
            new UserLog { Id = 11, UserId = 2, Action = UserActionType.Viewed }
        );
        await ctx.SaveChangesAsync();

        var sut = new UserLogService(ctx);

        (await sut.GetByIdAsync(11))!.Action.Should().Be(UserActionType.Viewed);
        (await sut.GetByIdAsync(99)).Should().BeNull();
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
