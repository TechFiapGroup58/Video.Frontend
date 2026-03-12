using Video.Frontend.Application.DTOs;
using Video.Frontend.Domain.Models;

namespace Video.Frontend.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse>  LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse>  RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task                LogoutAsync();
    Task<UserSession?>  GetSessionAsync();
    Task<bool>          IsAuthenticatedAsync();
}

public interface IUploadService
{
    Task<UploadVideoResponse>             UploadAsync(Stream file, string fileName, string contentType, long size, CancellationToken ct = default);
    Task<IEnumerable<VideoUploadSummary>> ListAsync(CancellationToken ct = default);
    Task<VideoUploadSummary>              GetByIdAsync(Guid id, CancellationToken ct = default);
}

// ── Prontos para implementar quando os serviços estiverem disponíveis ─────────

public interface IProcessorService
{
    Task<ProcessingJobResponse>             GetJobAsync(Guid uploadId, CancellationToken ct = default);
    Task<IEnumerable<ProcessingJobResponse>> ListJobsAsync(CancellationToken ct = default);
}

public interface INotificationService
{
    Task<IEnumerable<NotificationResponse>> ListAsync(CancellationToken ct = default);
    Task                                    MarkAsReadAsync(Guid id, CancellationToken ct = default);
    Task                                    MarkAllAsReadAsync(CancellationToken ct = default);
}
