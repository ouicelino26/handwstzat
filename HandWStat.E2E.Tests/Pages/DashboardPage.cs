using Microsoft.Playwright;

namespace HandWStat.E2E.Tests.Pages;

/// <summary>Page object for /dashboard.</summary>
public sealed class DashboardPage(IPage page, string baseUrl)
{
    public async Task NavigateAsync()
    {
        await page.GotoAsync(baseUrl + "/dashboard");
        await WaitForLoadAsync();
    }

    /// <summary>Waits until the dashboard workspace is rendered (not a boot splash).</summary>
    public async Task WaitForLoadAsync(int timeoutMs = 20_000)
    {
        await page.WaitForSelectorAsync("[data-workspace='dashboard']", new PageWaitForSelectorOptions
        {
            Timeout = timeoutMs
        });
    }

    /// <summary>Returns true when the busy indicator has cleared.</summary>
    public async Task WaitForIdleAsync(int timeoutMs = 20_000)
    {
        await page.WaitForFunctionAsync(
            "() => document.querySelector('[data-workspace=\"dashboard\"]')?.getAttribute('aria-busy') !== 'true'",
            null, new PageWaitForFunctionOptions { Timeout = timeoutMs });
    }

    public Task ClickTabAsync(string section)
        => page.ClickAsync($"[data-testid='tab-{section}']");

    public Task<bool> IsTabActiveAsync(string section)
        => page.IsVisibleAsync($"[data-testid='tab-{section}'][aria-selected='true']");

    public async Task<string?> GetScopeLabelAsync()
    {
        var el = await page.QuerySelectorAsync(".dashboard-scope-bar__value");
        return el is null ? null : await el.TextContentAsync();
    }
}
