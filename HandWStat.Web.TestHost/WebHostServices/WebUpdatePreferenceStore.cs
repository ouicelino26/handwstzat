using HandWStat.Services.Updates;

namespace HandWStat.Web.TestHost.WebHostServices;

public sealed class WebUpdatePreferenceStore : IUpdatePreferenceStore
{
    public DateTimeOffset? LastSuccessfulCheckUtc { get; set; }
    public string? IgnoredVersion { get; set; }
    public int? IgnoredBuild { get; set; }
    public string? LastStatus { get; set; }
}
