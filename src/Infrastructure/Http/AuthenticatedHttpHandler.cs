using Blazored.LocalStorage;
using Video.Frontend.Domain.Models;

namespace Video.Frontend.Infrastructure.Http;

/// <summary>
/// DelegatingHandler que injeta o Bearer token em todas as requisições
/// para serviços que exigem autenticação (UploadService, ProcessorService, etc.)
/// </summary>
public sealed class AuthenticatedHttpHandler : DelegatingHandler
{
    private readonly ILocalStorageService _storage;
    private const string SessionKey = "fiapx_session";

    public AuthenticatedHttpHandler(ILocalStorageService storage)
        => _storage = storage;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken  cancellationToken)
    {
        var session = await _storage.GetItemAsync<UserSession>(SessionKey);

        if (session is not null && !session.IsExpired)
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", session.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}
