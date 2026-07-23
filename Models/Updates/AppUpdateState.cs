using HandballManagerCore.DTO;

namespace HandWStat.Models.Updates;

public sealed record AppUpdateState(
    bool IsChecking,
    ClientUpdateCheckResponseDto? Response,
    string? ErrorMessage,
    bool OptionalUpdateDismissed)
{
    public static AppUpdateState Initial { get; } = new(false, null, null, false);

    public bool IsMandatory => Response?.UpdateAvailable == true && Response.Mandatory;

    public bool HasOptionalUpdate =>
        Response?.UpdateAvailable == true && !Response.Mandatory && !OptionalUpdateDismissed;
}

