namespace HandWStat.E2E.Tests.Helpers;

/// <summary>
/// Runtime configuration for E2E tests — sourced from environment variables.
/// No credentials are hardcoded here. See README.md for setup instructions.
/// </summary>
public static class E2EConfig
{
    /// <summary>Backend API base URL (no trailing slash). Injected into the TestHost at startup.</summary>
    public static string ApiBaseUrl =>
        Environment.GetEnvironmentVariable("HANDWSTAT_E2E_BASE_URL")
        ?? "https://api.dev.handwstat.fr";

    /// <summary>Username to log in with during smoke tests.</summary>
    public static string? Username =>
        Environment.GetEnvironmentVariable("HANDWSTAT_E2E_USERNAME");

    /// <summary>Password for the smoke-test account.</summary>
    public static string? Password =>
        Environment.GetEnvironmentVariable("HANDWSTAT_E2E_PASSWORD");

    /// <summary>
    /// Optional: name of a field player to use in Players/Compare tests.
    /// If absent, the first player found in the directory is used.
    /// </summary>
    public static string? FieldPlayerName =>
        Environment.GetEnvironmentVariable("HANDWSTAT_E2E_FIELD_PLAYER");

    /// <summary>
    /// Optional: name of a goalkeeper to use in GK-specific tests.
    /// If absent, GK-specific tests are skipped.
    /// </summary>
    public static string? GoalkeeperName =>
        Environment.GetEnvironmentVariable("HANDWSTAT_E2E_GOALKEEPER");

    /// <summary>Returns true when credentials are configured.</summary>
    public static bool HasCredentials =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);

    /// <summary>Returns true when a field player fixture is configured.</summary>
    public static bool HasFieldPlayer => !string.IsNullOrWhiteSpace(FieldPlayerName);

    /// <summary>Returns true when a goalkeeper fixture is configured.</summary>
    public static bool HasGoalkeeper => !string.IsNullOrWhiteSpace(GoalkeeperName);

    /// <summary>Playwright browser to use (chromium/firefox/webkit). Default: chromium.</summary>
    public static string BrowserType =>
        Environment.GetEnvironmentVariable("HANDWSTAT_E2E_BROWSER") ?? "chromium";

    /// <summary>Run in headless mode. Default: true. Set to "false" to watch tests.</summary>
    public static bool Headless =>
        (Environment.GetEnvironmentVariable("HANDWSTAT_E2E_HEADLESS") ?? "true")
            .Equals("true", StringComparison.OrdinalIgnoreCase);

    /// <summary>Slow-motion delay in milliseconds for debugging. Default: 0.</summary>
    public static float SlowMo =>
        float.TryParse(Environment.GetEnvironmentVariable("HANDWSTAT_E2E_SLOW_MO"), out var v) ? v : 0f;
}
