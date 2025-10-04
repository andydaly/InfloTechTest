using Microsoft.AspNetCore.Mvc;
using Moq;
using UserManagement.Api.Controllers;
using UserManagement.API.Models;           
using UserManagement.Data.Entities;       
using UserManagement.Services.Interfaces;  

namespace UserManagement.Data.Tests
{
    public class LogsControllerApiTests
    {
        private readonly Mock<IUserLogService> _logs = new();

        private LogsController CreateController() => new(_logs.Object);

        [Fact]
        public async Task GetAll_ReturnsOk_WithPagedResponse()
        {
            // Arrange
            var items = new[]
            {
                new UserLog { Id = 1, UserId = 10, Action = UserActionType.Viewed },
                new UserLog { Id = 2, UserId = 10, Action = UserActionType.Updated }
            }.AsEnumerable();

            _logs.Setup(s => s.GetAllAsync(2, 5, "bob", It.IsAny<CancellationToken>()))
                 .ReturnsAsync((items, 42));

            var sut = CreateController();

            // Act
            var result = await sut.GetAll(page: 2, pageSize: 5, q: "bob", ct: CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<LogListResponse>(ok.Value);

            payload.Total.Should().Be(42);
            payload.Page.Should().Be(2);
            payload.PageSize.Should().Be(5);
            payload.Query.Should().Be("bob");
            payload.Items.Select(i => i.Id).Should().BeEquivalentTo(new[] { 1L, 2L });

            _logs.Verify(s => s.GetAllAsync(2, 5, "bob", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_Found_ReturnsOk_WithEntry()
        {
            // Arrange
            var entry = new UserLog { Id = 7, UserId = 1, Action = UserActionType.Created };
            _logs.Setup(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entry);

            var sut = CreateController();

            // Act
            var result = await sut.GetById(7, CancellationToken.None);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<UserLog>(ok.Value);
            payload.Should().BeSameAs(entry);

            _logs.Verify(s => s.GetByIdAsync(7, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_Missing_ReturnsNotFound()
        {
            // Arrange
            _logs.Setup(s => s.GetByIdAsync(404, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((UserLog?)null);

            var sut = CreateController();

            // Act
            var result = await sut.GetById(404, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
            _logs.Verify(s => s.GetByIdAsync(404, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
