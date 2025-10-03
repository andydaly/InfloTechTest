using Microsoft.JSInterop;
using UserManagement.BlazorWeb.Auth;

public class BrowserTokenStore : ITokenStore
{
    private const string Key = "um.jwt";
    private readonly IJSRuntime _js;

    public BrowserTokenStore(IJSRuntime js)
    {
        _js = js;
    }

    public Task SetTokenAsync(string? token) => _js.InvokeVoidAsync("localStorage.setItem", Key, token ?? "").AsTask();

    public Task<string?> GetTokenAsync() => _js.InvokeAsync<string?>("localStorage.getItem", Key).AsTask();

    public Task ClearAsync() => _js.InvokeVoidAsync("localStorage.removeItem", Key).AsTask();
}
