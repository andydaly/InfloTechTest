using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UserManagement.BlazorWeb;
using UserManagement.BlazorWeb.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


var baseUrlFromConfig = builder.Configuration["Api:BaseUrl"];
var apiBaseUrl = !string.IsNullOrWhiteSpace(baseUrlFromConfig)
    ? new Uri(baseUrlFromConfig, UriKind.Absolute)
    : new Uri(new Uri(builder.HostEnvironment.BaseAddress), "api/");

builder.Services.AddScoped<ITokenStore, BrowserTokenStore>();
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddHttpClient("api-anon", c => c.BaseAddress = apiBaseUrl);
builder.Services.AddHttpClient("api-auth", c => c.BaseAddress = apiBaseUrl).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("api-auth"));

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddScoped<AuthClient>();

await builder.Build().RunAsync();
