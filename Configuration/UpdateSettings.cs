namespace HandWStat.Configuration;

public sealed class UpdateSettings
{
    public bool Enabled { get; init; } = true;

    public bool CheckOnStartup { get; init; } = true;

    public bool CheckOnResume { get; init; } = true;

    public int CheckIntervalHours { get; init; } = 12;

    public string Channel { get; init; } = "STABLE";

    public bool AllowAutomaticDownload { get; init; }
}
