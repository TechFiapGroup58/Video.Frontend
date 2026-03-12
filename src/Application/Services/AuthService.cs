using System.Net.Http.Json;
using Blazored.LocalStorage;
using Video.Frontend.Application.DTOs;
using Video.Frontend.Application.Interfaces;
using Video.Frontend.Domain.Models;

namespace Video.Frontend.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly HttpClient         _http;
    private readonly ILocalStorageService _storage;
    private const string SessionKey = "fiapx_session";

    public AuthService(IHttpClientFactory factory, ILocalStorageService storage)
    {
        _http    = factory.CreateClient("AuthService");
        _storage = storage;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new
        {
            request.Email,
            request.Password
        }, ct);

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct)
            ?? throw new Exception("Resposta inválida do servidor.");

        await SaveSessionAsync(auth);
        return auth;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", new
        {
            request.FullName,
            request.Email,
            request.Password
        }, ct);

        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct)
            ?? throw new Exception("Resposta inválida do servidor.");

        await SaveSessionAsync(auth);
        return auth;
    }

    public async Task LogoutAsync()
        => await _storage.RemoveItemAsync(SessionKey);

    public async Task<UserSession?> GetSessionAsync()
    {
        var session = await _storage.GetItemAsync<UserSession>(SessionKey);
        if (session is null || session.IsExpired)
        {
            await _storage.RemoveItemAsync(SessionKey);
            return null;
        }
        return session;
    }

    public async Task<bool> IsAuthenticatedAsync()
        => await GetSessionAsync() is not null;

    private async Task SaveSessionAsync(AuthResponse auth)
        => await _storage.SetItemAsync(SessionKey, new UserSession
        {
            Token     = auth.Token,
            Email     = auth.Email,
            FullName  = auth.FullName,
            ExpiresAt = auth.ExpiresAt
        });
}
