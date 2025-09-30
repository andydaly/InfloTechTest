using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using UserManagement.Services.Domain.Interfaces;
using UserManagement.Web.Models.Logs;
using UserManagement.WebMS.Controllers;

namespace UserManagement.Data.Tests;

public class LogsControllerTests
{
    [Fact]
    public void Index_MapsServiceResultsToViewModel()
    {
        // Arrange
        var svc = new Mock<IUserLogService>();
        var logs = new[]
        {
            new UserLog { Id = 1, UserId = 1, Action = UserActionType.Created, OccurredAt = DateTimeOffset.UtcNow },
            new UserLog { Id = 2, UserId = 2, Action = UserActionType.Updated, OccurredAt = DateTimeOffset.UtcNow }
        };
        svc.Setup(s => s.GetAll(1, 25, null))
           .Returns((logs, 2));

        var sut = new LogsController(svc.Object);

        // Act
        var result = sut.Index(page: 1, pageSize: 25, q: null) as ViewResult;

        // Assert
        result.Should().NotBeNull();
        result!.Model.Should().BeOfType<LogListViewModel>()
            .Which.Items.Select(i => i.Id).Should().BeEquivalentTo(new[] { 1L, 2L });
    }

    [Fact]
    public void Details_WhenFound_ReturnsViewWithModel()
    {
        // Arrange
        var svc = new Mock<IUserLogService>();
        var entry = new UserLog { Id = 123, UserId = 9, Action = UserActionType.Viewed };
        svc.Setup(s => s.GetById(123)).Returns(entry);
        var sut = new LogsController(svc.Object);

        // Act
        var result = sut.Details(123);

        // Assert
        result.Should().BeOfType<ViewResult>()
              .Which.Model.Should().BeSameAs(entry);
    }

    [Fact]
    public void Details_WhenMissing_ReturnsNotFound()
    {
        // Arrange
        var svc = new Mock<IUserLogService>();
        svc.Setup(s => s.GetById(404)).Returns((UserLog?)null);
        var sut = new LogsController(svc.Object);

        // Act
        var result = sut.Details(404);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
