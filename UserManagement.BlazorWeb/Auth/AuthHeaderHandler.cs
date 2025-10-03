﻿using System.Net.Http.Headers;
using UserManagement.BlazorWeb.Auth;

public sealed class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenStore _tokens;
    public AuthHeaderHandler(ITokenStore tokens)
    {
        _tokens = tokens;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokens.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
