using System.Net.Http.Json;
using UserManagement.BlazorWeb.Auth;

public class AuthClient
{
    private readonly HttpClient _http;
    private readonly JwtAuthStateProvider _authState;
    private readonly ITokenStore _tokens;

    public AuthClient(HttpClient http, JwtAuthStateProvider authState, ITokenStore tokens)
    {
        _http = http;
        _authState = authState;
        _tokens = tokens;
    }

    public async Task<bool> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var payload = new { email, password };
        var resp = await _http.PostAsJsonAsync("auth/login", payload, ct);
        if (!resp.IsSuccessStatusCode) return false;

        var dto = await resp.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken: ct);
        if (dto is null || string.IsNullOrWhiteSpace(dto.Token)) return false;

        await _authState.NotifyUserAuthenticationAsync(dto.Token);
        return true;
    }

    public Task LogoutAsync() => _authState.NotifyUserLogoutAsync();

    private record TokenResponse(string Token);
}
