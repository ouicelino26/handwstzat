using HandballManagerCore.DTO;

namespace HandWStat.Models.Updates;

public enum AppUpdateStatus
{
    NotChecked,
    Checking,
    UpToDate,
    OptionalUpdateAvailable,
    MandatoryUpdateAvailable,
    Downloading,
    DownloadVerified,
    Installing,
    Error
}

public sealed record AppUpdateState(
    AppUpdateStatus Status,
    ClientUpdateCheckResponseDto? Response,
    string? ErrorMessage,
    bool OptionalUpdateDismissed,
    double DownloadProgress = 0,
    string? DownloadedFilePath = null)
{
    public static AppUpdateState Initial { get; } = new(AppUpdateStatus.NotChecked, null, null, false);

    public bool IsChecking => Status == AppUpdateStatus.Checking;

    public bool IsMandatory => Status == AppUpdateStatus.MandatoryUpdateAvailable
        || Response?.UpdateAvailable == true && Response.Mandatory;

    public bool HasOptionalUpdate => Status == AppUpdateStatus.OptionalUpdateAvailable
        && !OptionalUpdateDismissed;
}
