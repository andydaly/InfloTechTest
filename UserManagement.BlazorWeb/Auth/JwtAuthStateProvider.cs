using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using UserManagement.BlazorWeb.Auth;

public sealed class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly ITokenStore _tokens;

    public JwtAuthStateProvider(ITokenStore tokens)
    {
        _tokens = tokens;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokens.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = ParseClaims(token);
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "jwt"));
        return new AuthenticationState(user);
    }

    public async Task NotifyUserAuthenticationAsync(string token)
    {
        await _tokens.SetTokenAsync(token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task NotifyUserLogoutAsync()
    {
        await _tokens.ClearAsync();
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim> ParseClaims(string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
}
