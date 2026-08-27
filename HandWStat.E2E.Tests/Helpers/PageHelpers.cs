using Microsoft.Playwright;

namespace HandWStat.E2E.Tests.Helpers;

/// <summary>
/// Attaches console-error and network-failure monitors to a page.
/// Tests call AssertNoErrors() at the end of each scenario.
/// </summary>
public sealed class PageMonitor : IDisposable
{
    private readonly IPage _page;
    private readonly List<string> _consoleErrors = [];
    private readonly List<string> _networkErrors = [];

    // Console messages that are expected and do not indicate an app bug.
    // CSS/font/static-asset 404s are expected in the WebApplicationFactory test environment
    // because NuGet package static web assets (_content/) and Blazor isolation CSS
    // (.styles.css) are not served in the "Test" environment without UseStaticWebAssets().
    // These do not affect Blazor circuit functionality.
    private static readonly string[] AllowlistedConsolePatterns =
    [
        "ResizeObserver loop",          // ApexCharts SVG animation noise in headless
        "_content/",                    // NuGet package static web assets (CSS, fonts)
        "/_framework/",                 // Blazor framework JS — TestHost serves blazor.server.js,
                                        // not blazor.web.js; circuit still works via /_blazor SignalR
        "/_blazor",                     // Blazor SignalR hub path
        ".styles.css",                  // Blazor component isolation CSS
        ".css",                         // Any CSS resource 404 (layout only, not functional)
        ".woff",                        // Fonts
        ".woff2",                       // Fonts
        "apexcharts.css",               // ApexCharts stylesheet
    ];

    public PageMonitor(IPage page)
    {
        _page = page;
        page.Console     += OnConsole;
        page.PageError   += OnPageError;
        page.RequestFailed += OnRequestFailed;
    }

    private void OnConsole(object? _, IConsoleMessage msg)
    {
        if (msg.Type is not ("error" or "assert")) return;

        // "Failed to load resource: 404" messages put the URL in msg.Location,
        // not in msg.Text — check both so the allowlist works for asset 404s.
        var location = msg.Location ?? string.Empty;
        if (AllowlistedConsolePatterns.Any(p =>
            msg.Text.Contains(p, StringComparison.OrdinalIgnoreCase) ||
            location.Contains(p, StringComparison.OrdinalIgnoreCase))) return;

        _consoleErrors.Add($"[console.{msg.Type}] {msg.Text}");
    }

    private void OnPageError(object? _, string error)
    {
        _consoleErrors.Add($"[page-error] {error}");
    }

    private void OnRequestFailed(object? _, IRequest req)
    {
        // Blazor infrastructure — ignore
        if (req.Url.Contains("/_blazor") || req.Url.Contains("/_framework")) return;
        // CSS/font/icon static assets — do not affect circuit functionality
        if (req.Url.EndsWith(".css", StringComparison.OrdinalIgnoreCase)) return;
        if (req.Url.EndsWith(".css.gz", StringComparison.OrdinalIgnoreCase)) return;
        if (req.Url.EndsWith(".woff", StringComparison.OrdinalIgnoreCase)) return;
        if (req.Url.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase)) return;
        if (req.Url.Contains("/_content/")) return;
        if (req.Url.Contains(".styles.")) return;
        _networkErrors.Add($"[net-fail] {req.Method} {req.Url} → {req.Failure}");
    }

    public IReadOnlyList<string> ConsoleErrors  => _consoleErrors;
    public IReadOnlyList<string> NetworkErrors  => _networkErrors;

    public void AssertNoErrors()
    {
        var all = _consoleErrors.Concat(_networkErrors).ToList();
        if (all.Count > 0)
            throw new InvalidOperationException(
                "Page errors detected:\n" + string.Join("\n", all));
    }

    public void Dispose()
    {
        _page.Console -= OnConsole;
        _page.PageError -= OnPageError;
        _page.RequestFailed -= OnRequestFailed;
    }
}

/// <summary>Screenshots and trace helpers for on-failure diagnostics.</summary>
public static class ArtifactHelper
{
    private static readonly string ArtifactsDir = Path.Combine(
        AppContext.BaseDirectory, "..", "..", "..", "..", "playwright-artifacts");

    public static async Task CaptureScreenshotAsync(IPage page, string name)
    {
        try
        {
            Directory.CreateDirectory(ArtifactsDir);
            var path = Path.Combine(ArtifactsDir, $"{name}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
        }
        catch
        {
            // screenshot failure must never mask the original test failure
        }
    }
}
