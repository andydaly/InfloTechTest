using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using UserManagement.Api.Controllers;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Data.Tests;

public class AuthControllerTests
{
    private readonly Mock<IUserService> _users = new();
    private readonly Mock<IUserLogService> _logs = new();

    private static IConfiguration CreateJwtConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "super_secret_test_key_1234567890",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:ExpiresMinutes"] = "5"
            })
            .Build();

    private AuthController CreateController(IConfiguration? cfg = null)
        => new(_users.Object, _logs.Object, cfg ?? CreateJwtConfig());

    [Fact]
    public async Task Login_MissingFields_ReturnsBadRequest()
    {
        // Arrange
        var sut = CreateController();
        var req = new AuthController.LoginRequest("", "");

        // Act
        var result = await sut.Login(req, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _users.VerifyNoOtherCalls();
        _logs.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_UserNotFound_ReturnsUnauthorized()
    {
        // Arrange
        _users.Setup(s => s.GetByEmailAsync("x@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var sut = CreateController();
        var req = new AuthController.LoginRequest("x@example.com", "pwd");

        // Act
        var result = await sut.Login(req, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
        _logs.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_InactiveUser_ReturnsUnauthorized()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Forename = "Ina",
            Surname = "Ctive",
            Email = "ina@example.com",
            Password = "pass",
            IsActive = false
        };

        _users.Setup(s => s.GetByEmailAsync("ina@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);

        var sut = CreateController();
        var req = new AuthController.LoginRequest("ina@example.com", "pass");

        // Act
        var result = await sut.Login(req, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
        _logs.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var user = new User
        {
            Id = 2,
            Forename = "Wron",
            Surname = "Gpass",
            Email = "wrong@example.com",
            Password = "correct",
            IsActive = true
        };

        _users.Setup(s => s.GetByEmailAsync("wrong@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);

        var sut = CreateController();
        var req = new AuthController.LoginRequest("wrong@example.com", "incorrect");

        // Act
        var result = await sut.Login(req, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedResult>();
        _logs.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Login_Success_ReturnsOk_WithToken_AndLogs()
    {
        // Arrange
        var user = new User
        {
            Id = 5,
            Forename = "Peter",
            Surname = "Loew",
            Email = "ploew@example.com",
            Password = "mypassword1",
            IsActive = true
        };

        _users.Setup(s => s.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);

        var sut = CreateController();
        var req = new AuthController.LoginRequest(user.Email, user.Password);

        // Act
        var result = await sut.Login(req, CancellationToken.None);

        // Assert 
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<AuthController.LoginResponse>().Subject;

        payload.Token.Should().NotBeNullOrWhiteSpace();
        payload.UserId.Should().Be(user.Id);
        payload.Forename.Should().Be(user.Forename);
        payload.Email.Should().Be(user.Email);

        _logs.Verify(l => l.LogAsync(
            user.Id,
            UserActionType.LoggedIn,
            It.Is<string>(d => d.Contains("User logged in", StringComparison.OrdinalIgnoreCase)),
            user.Email,
            It.IsAny<CancellationToken>()), Times.Once);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(payload.Token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == "given_name" && c.Value == user.Forename);
        jwt.Claims.Should().Contain(c => c.Type == "family_name" && c.Value == user.Surname);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == $"{user.Forename} {user.Surname}");
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
    }
}
