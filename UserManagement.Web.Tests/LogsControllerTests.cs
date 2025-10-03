using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;
using UserManagement.Web.Models.Logs;
using UserManagement.Web.Controllers;

namespace UserManagement.Data.Tests;

public class LogsControllerTests
{
    [Fact]
    public async Task Index_MapsServiceResultsToViewModel()
    {
        // Arrange
        var svc = new Mock<IUserLogService>();
        var logs = new[]
        {
            new UserLog { Id = 1, UserId = 1, Action = UserActionType.Created, OccurredAt = DateTimeOffset.UtcNow },
            new UserLog { Id = 2, UserId = 2, Action = UserActionType.Updated, OccurredAt = DateTimeOffset.UtcNow }
        }.AsEnumerable();

        svc.Setup(s => s.GetAllAsync(1, 25, null, It.IsAny<CancellationToken>())).ReturnsAsync((logs, 2));

        var sut = new LogsController(svc.Object);

        // Act
        var result = await sut.Index(page: 1, pageSize: 25, q: null) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().BeOfType<LogListViewModel>().Which.Items.Select(i => i.Id).Should().BeEquivalentTo(new[] { 1L, 2L });
    }

    [Fact]
    public async Task Details_WhenFound_ReturnsViewWithModel()
    {
        // Arrange
        var svc = new Mock<IUserLogService>();
        var entry = new UserLog { Id = 123, UserId = 9, Action = UserActionType.Viewed };
        svc.Setup(s => s.GetByIdAsync(123, It.IsAny<CancellationToken>())).ReturnsAsync(entry);

        var sut = new LogsController(svc.Object);

        // Act
        var result = await sut.Details(123);

        // Assert
        result.Should().BeOfType<ViewResult>().Which.Model.Should().BeSameAs(entry);
    }

    [Fact]
    public async Task Details_WhenMissing_ReturnsNotFound()
    {
        // Arrange
        var svc = new Mock<IUserLogService>();
        svc.Setup(s => s.GetByIdAsync(404, It.IsAny<CancellationToken>())).ReturnsAsync((UserLog?)null);

        var sut = new LogsController(svc.Object);

        // Act
        var result = await sut.Details(404);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
