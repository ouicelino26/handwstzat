using HandWStat.Configuration;
using HandWStat.Services;

namespace HandWStat.Services.Api;

public sealed class ExportApiClient : ApiClientBase
{
    public ExportApiClient(HttpClient httpClient, ApiSettings settings, IApiAuthService authService)
        : base(httpClient, settings, authService)
    {
    }

    public Task<ExportMetaDto?> GenerateExportMetaAsync(
        AnalyticsExportRequestDto request,
        CancellationToken ct = default)
        => PostAsync<AnalyticsExportRequestDto, ExportMetaDto>(
            "api/v2/exports/analytics", request, ct);

    public async Task<(byte[]? Content, string? FileName, string? ContentType)> DownloadExportAsync(
        AnalyticsExportRequestDto request,
        CancellationToken ct = default)
    {
        return await PostDownloadAsync("api/v2/exports/analytics/download", request, ct);
    }
}

// ── local DTOs (mirror API contracts) ──────────────────────────────────────────

public sealed class AnalyticsExportRequestDto
{
    public string Scope { get; set; } = "SEASON";
    public int? SeasonYear { get; set; }
    public string? SeasonLabel { get; set; }
    public int? CompetitionId { get; set; }
    public int? TeamId { get; set; }
    public List<int>? PlayerIds { get; set; }
    public List<int>? MatchIds { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Day { get; set; }
    public List<string>? Sections { get; set; }
    public bool IncludeRawEvents { get; set; }
    public bool IncludeShotCoordinates { get; set; }
    public bool? IncludeDataQuality { get; set; } = true;
    public string? Format { get; set; } = "XLSX";
    public string? Locale { get; set; } = "fr-FR";
    public string? RequestedBy { get; set; } = "HandWStat";
}

public sealed class ExportMetaDto
{
    public string ExportId { get; set; } = "";
    public string FileName { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public string Sha256 { get; set; } = "";
    public string ContentType { get; set; } = "";
    public string ExportSchemaVersion { get; set; } = "";
    public string GeneratedAtUtc { get; set; } = "";
    public List<string> Warnings { get; set; } = [];
    public string DownloadUrl { get; set; } = "";
}
