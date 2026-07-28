namespace HandWStat.Services.Updates;

public sealed class MauiUpdatePreferenceStore : IUpdatePreferenceStore
{
    private const string LastCheckKey = "updates.last-successful-check-utc";
    private const string IgnoredVersionKey = "updates.ignored-version";
    private const string IgnoredBuildKey = "updates.ignored-build";
    private const string LastStatusKey = "updates.last-status";

    public DateTimeOffset? LastSuccessfulCheckUtc
    {
        get
        {
            var value = Preferences.Default.Get(LastCheckKey, string.Empty);
            return DateTimeOffset.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)
                    ? parsed
                    : null;
        }
        set => SetOrRemove(LastCheckKey, value?.ToUniversalTime().ToString("O"));
    }

    public string? IgnoredVersion
    {
        get => NullIfEmpty(Preferences.Default.Get(IgnoredVersionKey, string.Empty));
        set => SetOrRemove(IgnoredVersionKey, value);
    }

    public int? IgnoredBuild
    {
        get
        {
            var value = Preferences.Default.Get(IgnoredBuildKey, 0);
            return value > 0 ? value : null;
        }
        set
        {
            if (value is > 0) Preferences.Default.Set(IgnoredBuildKey, value.Value);
            else Preferences.Default.Remove(IgnoredBuildKey);
        }
    }

    public string? LastStatus
    {
        get => NullIfEmpty(Preferences.Default.Get(LastStatusKey, string.Empty));
        set => SetOrRemove(LastStatusKey, value);
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static void SetOrRemove(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) Preferences.Default.Remove(key);
        else Preferences.Default.Set(key, value);
    }
}
