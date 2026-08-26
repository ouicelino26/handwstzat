using HandWStat.E2E.Tests.Helpers;
using Microsoft.Playwright;
using Xunit;

namespace HandWStat.E2E.Tests.Fixtures;

/// <summary>
/// Manages Playwright browser lifecycle: one browser instance shared per collection.
/// </summary>
public sealed class BrowserFixture : IAsyncLifetime
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public IBrowser Browser => _browser ?? throw new InvalidOperationException("Browser not initialised");

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        IBrowserType browserType = E2EConfig.BrowserType switch
        {
            "firefox" => _playwright.Firefox,
            "webkit"  => _playwright.Webkit,
            _         => _playwright.Chromium,
        };

        _browser = await browserType.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = E2EConfig.Headless,
            SlowMo   = E2EConfig.SlowMo,
        });
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }
}
