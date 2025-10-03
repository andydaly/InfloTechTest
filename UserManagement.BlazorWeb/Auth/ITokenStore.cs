namespace UserManagement.BlazorWeb.Auth;

public interface ITokenStore
{
    Task SetTokenAsync(string? token);
    Task<string?> GetTokenAsync();
    Task ClearAsync();
}
