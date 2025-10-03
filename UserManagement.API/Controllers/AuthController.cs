using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserManagement.Data.Entities;
using UserManagement.Services.Interfaces;

namespace UserManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _users;
    private readonly IUserLogService _logs;
    private readonly IConfiguration _config;

    public AuthController(IUserService users, IUserLogService logs, IConfiguration config)
    {
        _users = users;
        _logs = logs;
        _config = config;
    }

    public record LoginRequest(string Email, string Password);
    public class LoginResponse
    {
        public string Token { get; set; } = "";
        public long UserId { get; set; }
        public string Forename { get; set; } = "";
        public string Email { get; set; } = "";
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and Password are required.");

        var user = await _users.GetByEmailAsync(request.Email, ct);
        if (user is null || !user.IsActive)
            return Unauthorized();

        if (!string.Equals(user.Password, request.Password))
            return Unauthorized();

        var token = GenerateJwt(user);

        await _logs.LogAsync(
            user.Id,
            UserActionType.LoggedIn,   
            "User logged in (API)",
            performedBy: user.Email,
            ct: ct);

        return Ok(new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            Forename = user.Forename,
            Email = user.Email
        });
    }

    private string GenerateJwt(User user)
    {
        var jwt = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("given_name", user.Forename),
            new Claim("family_name", user.Surname),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{user.Forename} {user.Surname}")
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"] ?? "60")),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
