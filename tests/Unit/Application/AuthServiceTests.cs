using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Blazored.LocalStorage;
using Video.Frontend.Application.DTOs;
using Video.Frontend.Application.Services;
using Video.Frontend.Domain.Models;
using Xunit;

namespace Video.Frontend.Tests.Unit.Application;

public sealed class AuthServiceTests
{
    private readonly Mock<ILocalStorageService> _storageMock = new();

    // ── helpers ───────────────────────────────────────────────────────────────

    private static AuthService CreateService(
        HttpClient httpClient,
        ILocalStorageService storage)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("AuthService")).Returns(httpClient);
        return new AuthService(factory.Object, storage);
    }

    private static HttpClient FakeHttp(
        HttpStatusCode statusCode,
        object?        content = null)
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

        return new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
    }

    private static AuthResponse FakeAuthResponse() => new(
        Token:    "fake.jwt.token",
        Email:    "user@test.com",
        FullName: "Test User",
        ExpiresAt: DateTime.UtcNow.AddHours(1)
    );

    // ── LoginAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAuthResponse()
    {
        var auth    = FakeAuthResponse();
        var sut     = CreateService(FakeHttp(HttpStatusCode.OK, auth), _storageMock.Object);
        var request = new LoginRequest { Email = "user@test.com", Password = "password123" };

        var result = await sut.LoginAsync(request);

        result.Token.Should().Be(auth.Token);
        result.Email.Should().Be(auth.Email);
    }

    [Fact]
    public async Task Login_ValidCredentials_SavesSessionToStorage()
    {
        var auth = FakeAuthResponse();
        var sut  = CreateService(FakeHttp(HttpStatusCode.OK, auth), _storageMock.Object);

        await sut.LoginAsync(new LoginRequest { Email = "u@t.com", Password = "pass1234" });

        _storageMock.Verify(
            s => s.SetItemAsync("fiapx_session", It.Is<UserSession>(x => x.Token == auth.Token), default),
            Times.Once);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ThrowsHttpRequestException()
    {
        var sut = CreateService(FakeHttp(HttpStatusCode.Unauthorized), _storageMock.Object);

        var act = () => sut.LoginAsync(new LoginRequest { Email = "x@x.com", Password = "wrong" });

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task Login_ServerError_DoesNotSaveSession()
    {
        var sut = CreateService(FakeHttp(HttpStatusCode.InternalServerError), _storageMock.Object);

        try { await sut.LoginAsync(new LoginRequest { Email = "x@x.com", Password = "pass" }); }
        catch { /* esperado */ }

        _storageMock.Verify(
            s => s.SetItemAsync(It.IsAny<string>(), It.IsAny<UserSession>(), default),
            Times.Never);
    }

    // ── RegisterAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidData_ReturnsAuthResponse()
    {
        var auth = FakeAuthResponse();
        var sut  = CreateService(FakeHttp(HttpStatusCode.Created, auth), _storageMock.Object);
        var req  = new RegisterRequest
        {
            FullName = "Test User", Email = "user@test.com",
            Password = "password123", ConfirmPassword = "password123"
        };

        var result = await sut.RegisterAsync(req);

        result.FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsHttpRequestException()
    {
        var sut = CreateService(FakeHttp(HttpStatusCode.Conflict), _storageMock.Object);
        var req = new RegisterRequest
        {
            FullName = "X", Email = "dup@test.com",
            Password = "pass1234", ConfirmPassword = "pass1234"
        };

        var act = () => sut.RegisterAsync(req);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    // ── LogoutAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_RemovesSessionFromStorage()
    {
        var sut = CreateService(FakeHttp(HttpStatusCode.OK), _storageMock.Object);

        await sut.LogoutAsync();

        _storageMock.Verify(s => s.RemoveItemAsync("fiapx_session", default), Times.Once);
    }

    // ── GetSessionAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetSession_ValidSession_ReturnsSession()
    {
        var session = new UserSession
        {
            Token = "tok", Email = "e@e.com",
            FullName = "U", ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        _storageMock.Setup(s => s.GetItemAsync<UserSession>("fiapx_session", default))
                    .ReturnsAsync(session);

        var sut    = CreateService(FakeHttp(HttpStatusCode.OK), _storageMock.Object);
        var result = await sut.GetSessionAsync();

        result.Should().NotBeNull();
        result!.Token.Should().Be("tok");
    }

    [Fact]
    public async Task GetSession_ExpiredSession_ReturnsNull()
    {
        var expired = new UserSession
        {
            Token = "tok", Email = "e@e.com",
            FullName = "U", ExpiresAt = DateTime.UtcNow.AddHours(-1)
        };
        _storageMock.Setup(s => s.GetItemAsync<UserSession>("fiapx_session", default))
                    .ReturnsAsync(expired);

        var sut    = CreateService(FakeHttp(HttpStatusCode.OK), _storageMock.Object);
        var result = await sut.GetSessionAsync();

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSession_NoSession_ReturnsNull()
    {
        _storageMock.Setup(s => s.GetItemAsync<UserSession>("fiapx_session", default))
                    .ReturnsAsync((UserSession?)null);

        var sut    = CreateService(FakeHttp(HttpStatusCode.OK), _storageMock.Object);
        var result = await sut.GetSessionAsync();

        result.Should().BeNull();
    }

    // ── IsAuthenticatedAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task IsAuthenticated_WithValidSession_ReturnsTrue()
    {
        var session = new UserSession
        {
            Token = "t", Email = "e@e.com",
            FullName = "U", ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
        _storageMock.Setup(s => s.GetItemAsync<UserSession>("fiapx_session", default))
                    .ReturnsAsync(session);

        var sut = CreateService(FakeHttp(HttpStatusCode.OK), _storageMock.Object);
        var result = await sut.IsAuthenticatedAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAuthenticated_WithoutSession_ReturnsFalse()
    {
        _storageMock.Setup(s => s.GetItemAsync<UserSession>("fiapx_session", default))
                    .ReturnsAsync((UserSession?)null);

        var sut    = CreateService(FakeHttp(HttpStatusCode.OK), _storageMock.Object);
        var result = await sut.IsAuthenticatedAsync();

        result.Should().BeFalse();
    }
}
