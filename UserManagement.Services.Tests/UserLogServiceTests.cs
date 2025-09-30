using System;
using System.Linq;
using UserManagement.Models;
using UserManagement.Services.Domain.Implementations;

namespace UserManagement.Data.Tests;

public class UserLogServiceTests
{
    [Fact]
    public void Log_WritesEntryViaDataContext()
    {
        // Arrange
        var data = new Mock<IDataContext>();
        var sut = new UserLogService(data.Object);

        // Act
        sut.Log(42, UserActionType.Created, "Created record", "tester@example.com");

        // Assert
        data.Verify(d => d.Create(It.Is<UserLog>(l =>
            l.UserId == 42 &&
            l.Action == UserActionType.Created &&
            l.Details == "Created record" &&
            l.PerformedBy == "tester@example.com"
        )), Times.Once);
    }

    [Fact]
    public void GetForUser_ReturnsMostRecentFirst_AndHonorsTake()
    {
        // Arrange
        var userId = 7L;
        var logs = new[]
        {
            new UserLog { Id = 1, UserId = userId, Action = UserActionType.Viewed,  OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-10) },
            new UserLog { Id = 2, UserId = userId, Action = UserActionType.Updated, OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-5)  },
            new UserLog { Id = 3, UserId = userId, Action = UserActionType.Created, OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-1)  },
            new UserLog { Id = 4, UserId = 999,    Action = UserActionType.Created, OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-1)  }, // other user
        }.AsQueryable();

        var data = new Mock<IDataContext>();
        data.Setup(d => d.GetAll<UserLog>()).Returns(logs);
        var sut = new UserLogService(data.Object);

        // Act
        var result = sut.GetForUser(userId, take: 2).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(l => l.Id).Should().ContainInOrder(3, 2); // newest first
    }

    [Fact]
    public void GetAll_Paginates_And_Searches()
    {
        // Arrange
        var logs = new[]
        {
            new UserLog { Id = 1,  UserId = 1, Action = UserActionType.Created, Details = "Seed user",        PerformedBy = "system", OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-30) },
            new UserLog { Id = 2,  UserId = 2, Action = UserActionType.Updated, Details = "Changed email",    PerformedBy = "bob",    OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-20) },
            new UserLog { Id = 3,  UserId = 2, Action = UserActionType.Viewed,  Details = "Viewed profile",   PerformedBy = "bob",    OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-10) },
            new UserLog { Id = 4,  UserId = 3, Action = UserActionType.Deleted, Details = "Deleted account",  PerformedBy = "alice",  OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-5)  },
            new UserLog { Id = 5,  UserId = 42,Action = UserActionType.Created, Details = "Created 42",       PerformedBy = "qa",     OccurredAt = DateTimeOffset.UtcNow.AddMinutes(-1)  },
        }.AsQueryable();

        var data = new Mock<IDataContext>();
        data.Setup(d => d.GetAll<UserLog>()).Returns(logs);
        var sut = new UserLogService(data.Object);

        // page 1, size 2 -> newest first: Id 5, then 4
        var (items1, total1) = sut.GetAll(page: 1, pageSize: 2, query: null);
        items1.Select(i => i.Id).Should().ContainInOrder(5, 4);
        total1.Should().Be(5);

        // page 2, size 2 -> next two: Id 3, then 2
        var (items2, total2) = sut.GetAll(page: 2, pageSize: 2, query: null);
        items2.Select(i => i.Id).Should().ContainInOrder(3, 2);
        total2.Should().Be(5);

        // search by performedBy (case-insensitive)
        var (itemsQ, totalQ) = sut.GetAll(page: 1, pageSize: 10, query: "BoB");
        itemsQ.Should().OnlyContain(l => (l.PerformedBy ?? "").ToLower().Contains("bob"));
        totalQ.Should().Be(2);
    }

    [Fact]
    public void GetById_ReturnsExactMatchOrNull()
    {
        // Arrange
        var logs = new[]
        {
            new UserLog { Id = 10, UserId = 1, Action = UserActionType.Created },
            new UserLog { Id = 11, UserId = 2, Action = UserActionType.Viewed  },
        }.AsQueryable();

        var data = new Mock<IDataContext>();
        data.Setup(d => d.GetAll<UserLog>()).Returns(logs);
        var sut = new UserLogService(data.Object);

        // Act / Assert
        sut.GetById(11)?.Action.Should().Be(UserActionType.Viewed);
        sut.GetById(99).Should().BeNull();
    }
}
