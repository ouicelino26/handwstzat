using HandWStat.E2E.Tests.Helpers;
using HandWStat.E2E.Tests.Pages;
using Microsoft.Playwright;
using Xunit;

namespace HandWStat.E2E.Tests.Fixtures;

/// <summary>
/// Base class for all E2E tests. Provides:
/// - A fresh IPage per test with console/network monitoring
/// - Login helper
/// - Screenshot on failure
/// - Trace management (retain-on-failure)
/// </summary>
public abstract class E2ETestBase : IAsyncLifetime
{
    protected readonly E2EFixture Fixture;
    protected IPage Page { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected PageMonitor Monitor { get; private set; } = null!;

    protected string BaseUrl => Fixture.WebHost.ServerUrl;

    protected E2ETestBase(E2EFixture fixture)
    {
        Fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        Context = await Fixture.Browser.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1440, Height = 900 },
        });

        // Retain trace on failure
        await Context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots   = true,
            Sources     = false,
        });

        Page = await Context.NewPageAsync();
        Monitor = new PageMonitor(Page);
    }

    public async Task DisposeAsync()
    {
        Monitor.Dispose();
        await Context.Tracing.StopAsync(new TracingStopOptions()); // discarded — only keep on failure
        await Context.CloseAsync();
    }

    /// <summary>
    /// Navigates to login page, fills credentials, waits for /dashboard.
    /// Skips the test if credentials are not configured.
    /// </summary>
    protected async Task LoginAsync()
    {
        var login = new LoginPage(Page, BaseUrl);
        await login.LoginAndWaitForDashboardAsync();
    }

    /// <summary>
    /// Saves a screenshot and trace, then re-throws.
    /// Call this in catch blocks that should still fail the test.
    /// </summary>
    protected async Task FailWithArtifactsAsync(string testName, Exception ex)
    {
        await ArtifactHelper.CaptureScreenshotAsync(Page, testName);

        var traceDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "playwright-artifacts");
        Directory.CreateDirectory(traceDir);
        try
        {
            await Context.Tracing.StopAsync(new TracingStopOptions
            {
                Path = Path.Combine(traceDir, $"{testName}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip")
            });
        }
        catch { /* don't mask the original exception */ }

        throw new Exception($"Test '{testName}' failed. Artifacts saved to playwright-artifacts/.", ex);
    }
}
