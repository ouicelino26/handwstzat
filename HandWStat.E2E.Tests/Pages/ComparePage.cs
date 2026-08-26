using Microsoft.Playwright;

namespace HandWStat.E2E.Tests.Pages;

/// <summary>Page object for /compare.</summary>
public sealed class ComparePage(IPage page, string baseUrl)
{
    public async Task NavigateAsync()
    {
        await page.GotoAsync(baseUrl + "/compare");
        // Wait for either the player slots or the access-required card
        await page.WaitForSelectorAsync("[data-testid='cmp-slot-0'], .access-required", new PageWaitForSelectorOptions
        {
            Timeout = 15_000
        });
    }

    /// <summary>Opens the player picker for the given slot (0-based).</summary>
    public async Task OpenSlotPickerAsync(int slot)
    {
        await page.ClickAsync($"[data-testid='cmp-slot-{slot}']");
        // Wait for the picker search input to appear
        await page.WaitForSelectorAsync(".cmp-picker-search", new PageWaitForSelectorOptions
        {
            Timeout = 10_000
        });
    }

    /// <summary>Searches for and selects a player in the currently open picker.</summary>
    public async Task SelectPlayerInPickerAsync(string partialName)
    {
        var search = await page.WaitForSelectorAsync(".cmp-picker-search");
        await search!.FillAsync(partialName);

        // Wait for at least one result
        await page.WaitForSelectorAsync(".cmp-picker-player", new PageWaitForSelectorOptions
        {
            Timeout = 10_000
        });

        // Click the first result
        await page.ClickAsync(".cmp-picker-player");
    }

    /// <summary>Waits for the compare results panel to load (triggered by having ≥2 players selected).</summary>
    public async Task WaitForResultsAsync(int timeoutMs = 20_000)
    {
        await page.WaitForSelectorAsync(".cmp-section, .cmp-results", new PageWaitForSelectorOptions
        {
            Timeout = timeoutMs
        });
    }

    /// <summary>Returns true when the radar chart is visible and rendered (SVG present).</summary>
    public async Task<bool> IsRadarRenderedAsync()
    {
        var radar = await page.QuerySelectorAsync("[data-testid='compare-radar']");
        if (radar is null) return false;
        var svg = await radar.QuerySelectorAsync("svg");
        return svg is not null;
    }

    /// <summary>Gets the name displayed in the slot button for the given slot.</summary>
    public async Task<string?> GetSlotPlayerNameAsync(int slot)
    {
        var el = await page.QuerySelectorAsync($"[data-testid='cmp-slot-{slot}'] .cmp-player__name");
        return el is null ? null : (await el.TextContentAsync())?.Trim();
    }

    public async Task<bool> IsGkVsFieldNoticeVisibleAsync()
    {
        return await page.IsVisibleAsync(".cmp-no-radar-notice");
    }
}
