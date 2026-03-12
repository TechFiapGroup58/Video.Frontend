using System.ComponentModel.DataAnnotations;

namespace Video.Frontend.Application.DTOs;

// ── Auth ──────────────────────────────────────────────────────────────────────

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Nome obrigatório")]
    [MinLength(3, ErrorMessage = "Mínimo 3 caracteres")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha obrigatória")]
    [MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Compare(nameof(Password), ErrorMessage = "Senhas não conferem")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required(ErrorMessage = "E-mail obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha obrigatória")]
    public string Password { get; set; } = string.Empty;
}

public sealed record AuthResponse(
    string   Token,
    string   Email,
    string   FullName,
    DateTime ExpiresAt
);

// ── Upload ────────────────────────────────────────────────────────────────────

public sealed record UploadVideoResponse(
    Guid     Id,
    string   OriginalFileName,
    string   StoredFileName,
    long     FileSizeBytes,
    int      Status,
    string   StoragePath,
    DateTime CreatedAt
);

public sealed record VideoUploadSummary(
    Guid     Id,
    string   OriginalFileName,
    long     FileSizeBytes,
    int      Status,
    string?  ErrorMessage,
    DateTime CreatedAt
);

// ── ProcessorService (futuro) ─────────────────────────────────────────────────

public sealed record ProcessingJobResponse(
    Guid      Id,
    Guid      UploadId,
    string    Status,
    int       Progress,
    string?   ResultPath,
    DateTime  StartedAt,
    DateTime? FinishedAt
);

// ── NotificationService (futuro) ──────────────────────────────────────────────

public sealed record NotificationResponse(
    Guid     Id,
    string   Message,
    string   Type,
    bool     IsRead,
    DateTime CreatedAt
);
