using Microsoft.Playwright;

namespace HandWStat.E2E.Tests.Pages;

/// <summary>Page object for /players.</summary>
public sealed class PlayersPage(IPage page, string baseUrl)
{
    public async Task NavigateAsync()
    {
        await page.GotoAsync(baseUrl + "/players");
        await WaitForLoadAsync();
    }

    /// <summary>Waits for the player list to be populated.</summary>
    public async Task WaitForLoadAsync(int timeoutMs = 20_000)
    {
        await page.WaitForSelectorAsync("[data-testid='player-row']", new PageWaitForSelectorOptions
        {
            Timeout = timeoutMs
        });
    }

    /// <summary>Returns the names of all loaded player rows.</summary>
    public async Task<IReadOnlyList<string>> GetPlayerNamesAsync()
    {
        var rows = await page.QuerySelectorAllAsync("[data-testid='player-row']");
        var names = new List<string>(rows.Count);
        foreach (var r in rows)
        {
            var name = await r.GetAttributeAsync("data-player-name");
            if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
        }
        return names;
    }

    /// <summary>Clicks the first player row and waits for the detail pane to load.</summary>
    public async Task SelectFirstPlayerAsync()
    {
        var row = await page.WaitForSelectorAsync("[data-testid='player-row']");
        await row!.ClickAsync();
        await WaitForDetailPaneAsync();
    }

    /// <summary>Selects a player by partial name match.</summary>
    public async Task SelectPlayerByNameAsync(string partialName)
    {
        var rows = await page.QuerySelectorAllAsync("[data-testid='player-row']");
        foreach (var row in rows)
        {
            var name = await row.GetAttributeAsync("data-player-name") ?? string.Empty;
            if (name.Contains(partialName, StringComparison.OrdinalIgnoreCase))
            {
                await row.ClickAsync();
                await WaitForDetailPaneAsync();
                return;
            }
        }
        throw new InvalidOperationException($"No player row found matching '{partialName}'");
    }

    public async Task WaitForDetailPaneAsync(int timeoutMs = 20_000)
    {
        // Detail pane is loaded when a tab bar appears
        await page.WaitForSelectorAsync("[data-testid='tab-overview']", new PageWaitForSelectorOptions
        {
            Timeout = timeoutMs
        });
    }

    public Task ClickTabAsync(string section)
        => page.ClickAsync($"[data-testid='tab-{section}']");

    public Task<bool> HasTabAsync(string section)
        => page.IsVisibleAsync($"[data-testid='tab-{section}']");

    /// <summary>Gets the currently selected player's ID from the active row.</summary>
    public async Task<string?> GetSelectedPlayerIdAsync()
    {
        var active = await page.QuerySelectorAsync("[data-testid='player-row'][aria-pressed='true']");
        return active is null ? null : await active.GetAttributeAsync("data-player-id");
    }
}
