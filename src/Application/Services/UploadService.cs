using System.Net.Http.Headers;
using System.Net.Http.Json;
using Video.Frontend.Application.DTOs;
using Video.Frontend.Application.Interfaces;

namespace Video.Frontend.Application.Services;

public sealed class UploadService : IUploadService
{
    private readonly HttpClient _http;

    public UploadService(IHttpClientFactory factory)
        => _http = factory.CreateClient("UploadService");

    public async Task<UploadVideoResponse> UploadAsync(
        Stream  file,
        string  fileName,
        string  contentType,
        long    size,
        CancellationToken ct = default)
    {
        using var content    = new MultipartFormDataContent();
        using var fileContent = new StreamContent(file);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var response = await _http.PostAsync("api/uploads", content, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UploadVideoResponse>(cancellationToken: ct)
            ?? throw new Exception("Resposta inválida do servidor.");
    }

    public async Task<IEnumerable<VideoUploadSummary>> ListAsync(CancellationToken ct = default)
    {
        var result = await _http.GetFromJsonAsync<IEnumerable<VideoUploadSummary>>("api/uploads", ct);
        return result ?? Enumerable.Empty<VideoUploadSummary>();
    }

    public async Task<VideoUploadSummary> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<VideoUploadSummary>($"api/uploads/{id}", ct)
            ?? throw new Exception("Upload não encontrado.");
    }
}
