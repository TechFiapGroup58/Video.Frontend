using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Video.Frontend.Application.DTOs;
using Video.Frontend.Application.Services;
using Xunit;

namespace Video.Frontend.Tests.Unit.Application;

public sealed class UploadServiceTests
{
    private static UploadService CreateService(HttpClient httpClient)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("UploadService")).Returns(httpClient);
        return new UploadService(factory.Object);
    }

    private static HttpClient FakeHttp(HttpStatusCode statusCode, object? content = null)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content    = content is null
                    ? new StringContent("")
                    : JsonContent.Create(content)
            });

        return new HttpClient(handler.Object) { BaseAddress = new Uri("http://localhost/") };
    }

    private static UploadVideoResponse FakeUploadResponse() => new(
        Id: Guid.NewGuid(),
        OriginalFileName: "video.mp4",
        StoredFileName:   "stored-video.mp4",
        FileSizeBytes:    1024 * 1024,
        Status:           1,
        StoragePath:      "fiapx-videos/stored-video.mp4",
        CreatedAt:        DateTime.UtcNow
    );

    // ── UploadAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Upload_Success_ReturnsResponse()
    {
        var expected = FakeUploadResponse();
        var sut      = CreateService(FakeHttp(HttpStatusCode.Created, expected));

        var result = await sut.UploadAsync(
            new MemoryStream(new byte[] { 1, 2, 3 }),
            "video.mp4", "video/mp4", 3);

        result.Should().NotBeNull();
        result.OriginalFileName.Should().Be("video.mp4");
        result.Status.Should().Be(1);
    }

    [Fact]
    public async Task Upload_ServerError_ThrowsHttpRequestException()
    {
        var sut = CreateService(FakeHttp(HttpStatusCode.BadRequest));

        var act = () => sut.UploadAsync(
            new MemoryStream(new byte[] { 1 }),
            "video.mp4", "video/mp4", 1);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task Upload_Unauthorized_ThrowsHttpRequestException()
    {
        var sut = CreateService(FakeHttp(HttpStatusCode.Unauthorized));

        var act = () => sut.UploadAsync(
            new MemoryStream(new byte[] { 1 }),
            "video.mp4", "video/mp4", 1);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // ── ListAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task List_Success_ReturnsItems()
    {
        var items = new[]
        {
            new VideoUploadSummary(Guid.NewGuid(), "a.mp4", 100, 1, null, DateTime.UtcNow),
            new VideoUploadSummary(Guid.NewGuid(), "b.mp4", 200, 0, null, DateTime.UtcNow)
        };
        var sut = CreateService(FakeHttp(HttpStatusCode.OK, items));

        var result = await sut.ListAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task List_Unauthorized_ThrowsHttpRequestException()
    {
        var sut = CreateService(FakeHttp(HttpStatusCode.Unauthorized));

        var act = () => sut.ListAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_Found_ReturnsSummary()
    {
        var id      = Guid.NewGuid();
        var summary = new VideoUploadSummary(id, "video.mp4", 512, 1, null, DateTime.UtcNow);
        var sut     = CreateService(FakeHttp(HttpStatusCode.OK, summary));

        var result = await sut.GetByIdAsync(id);

        result.Id.Should().Be(id);
        result.OriginalFileName.Should().Be("video.mp4");
    }

    [Fact]
    public async Task GetById_NotFound_ThrowsHttpRequestException()
    {
        var sut = CreateService(FakeHttp(HttpStatusCode.NotFound));

        var act = () => sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetById_Forbidden_ThrowsHttpRequestException()
    {
        var sut = CreateService(FakeHttp(HttpStatusCode.Forbidden));

        var act = () => sut.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
