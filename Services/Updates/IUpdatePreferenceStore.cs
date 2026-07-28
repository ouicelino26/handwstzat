namespace HandWStat.Services.Updates;

public interface IUpdatePreferenceStore
{
    DateTimeOffset? LastSuccessfulCheckUtc { get; set; }

    string? IgnoredVersion { get; set; }

    int? IgnoredBuild { get; set; }

    string? LastStatus { get; set; }
}
