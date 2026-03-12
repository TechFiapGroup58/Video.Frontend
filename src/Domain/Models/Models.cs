namespace Video.Frontend.Domain.Models;

// ── Auth ──────────────────────────────────────────────────────────────────────

public sealed class UserSession
{
    public string   Token     { get; init; } = string.Empty;
    public string   Email     { get; init; } = string.Empty;
    public string   FullName  { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}

// ── Upload ────────────────────────────────────────────────────────────────────

public enum VideoStatus
{
    Pending    = 0,
    Uploaded   = 1,
    Failed     = 2,
    // Reservados para serviços futuros
    Processing = 10,
    Completed  = 11
}

public sealed class VideoUpload
{
    public Guid        Id               { get; init; }
    public string      OriginalFileName { get; init; } = string.Empty;
    public long        FileSizeBytes    { get; init; }
    public VideoStatus Status           { get; init; }
    public string?     ErrorMessage     { get; init; }
    public DateTime    CreatedAt        { get; init; }

    public string StatusLabel => Status switch
    {
        VideoStatus.Pending    => "Aguardando",
        VideoStatus.Uploaded   => "Enviado",
        VideoStatus.Processing => "Processando",
        VideoStatus.Completed  => "Concluído",
        VideoStatus.Failed     => "Falhou",
        _                      => "Desconhecido"
    };

    public string StatusCss => Status switch
    {
        VideoStatus.Uploaded   => "badge--green",
        VideoStatus.Processing => "badge--blue",
        VideoStatus.Completed  => "badge--teal",
        VideoStatus.Failed     => "badge--red",
        _                      => "badge--gray"
    };

    public string FileSizeFormatted => FileSizeBytes switch
    {
        < 1024                => $"{FileSizeBytes} B",
        < 1024 * 1024         => $"{FileSizeBytes / 1024.0:F1} KB",
        < 1024L * 1024 * 1024 => $"{FileSizeBytes / (1024.0 * 1024):F1} MB",
        _                     => $"{FileSizeBytes / (1024.0 * 1024 * 1024):F2} GB"
    };
}

// ── Processor (futuro ProcessorService) ──────────────────────────────────────

public sealed class ProcessingJob
{
    public Guid      Id         { get; init; }
    public Guid      UploadId   { get; init; }
    public string    Status     { get; init; } = string.Empty;
    public int       Progress   { get; init; }
    public string?   ResultPath { get; init; }
    public DateTime  StartedAt  { get; init; }
    public DateTime? FinishedAt { get; init; }
}

// ── Notification (futuro NotificationService) ─────────────────────────────────

public sealed class Notification
{
    public Guid     Id        { get; init; }
    public string   Message   { get; init; } = string.Empty;
    public string   Type      { get; init; } = "info"; // info | success | warning | error
    public bool     IsRead    { get; init; }
    public DateTime CreatedAt { get; init; }
}
