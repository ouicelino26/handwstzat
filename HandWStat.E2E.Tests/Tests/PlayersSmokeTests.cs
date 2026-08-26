using HandWStat.E2E.Tests.Fixtures;
using HandWStat.E2E.Tests.Helpers;
using HandWStat.E2E.Tests.Pages;
using Xunit;

namespace HandWStat.E2E.Tests.Tests;

[Collection("E2E")]
public sealed class PlayersSmokeTests(E2EFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task Players_ListLoads_AfterLogin()
    {
        if (!E2EConfig.HasCredentials) Assert.Skip("HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var players = new PlayersPage(Page, BaseUrl);
            await players.NavigateAsync();
            var names = await players.GetPlayerNamesAsync();
            Assert.True(names.Count > 0, "Expected at least one player row");
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Players_ListLoads_AfterLogin), ex); }
    }

    [Fact]
    public async Task Players_SelectFirstPlayer_ShowsDetailPane()
    {
        if (!E2EConfig.HasCredentials) Assert.Skip("HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var players = new PlayersPage(Page, BaseUrl);
            await players.NavigateAsync();
            await players.SelectFirstPlayerAsync();

            Assert.True(await players.HasTabAsync("overview"), "Expected Brief tab");
            Assert.True(await players.HasTabAsync("analysis"), "Expected Performance tab");
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Players_SelectFirstPlayer_ShowsDetailPane), ex); }
    }

    [Fact]
    public async Task Players_AllTabs_Render()
    {
        if (!E2EConfig.HasCredentials) Assert.Skip("HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var players = new PlayersPage(Page, BaseUrl);
            await players.NavigateAsync();
            await players.SelectFirstPlayerAsync();

            foreach (var tab in new[] { "overview", "analysis", "zones", "analyse" })
            {
                await players.ClickTabAsync(tab);
                await Page.WaitForFunctionAsync(
                    $"() => document.querySelector('[data-testid=\"tab-{tab}\"]')?.getAttribute('aria-selected') === 'true'",
                    null, new Microsoft.Playwright.PageWaitForFunctionOptions { Timeout = 10_000 });
                Assert.False(await Page.IsVisibleAsync("#blazor-error-ui"), $"Error UI on tab '{tab}'");
            }

            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Players_AllTabs_Render), ex); }
    }

    [Fact]
    public async Task Players_FixturePlayer_ProfileLoads()
    {
        if (!E2EConfig.HasCredentials) Assert.Skip("HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        if (!E2EConfig.HasFieldPlayer) Assert.Skip("HANDWSTAT_E2E_FIELD_PLAYER not configured");
        try
        {
            await LoginAsync();
            var players = new PlayersPage(Page, BaseUrl);
            await players.NavigateAsync();
            await players.SelectPlayerByNameAsync(E2EConfig.FieldPlayerName!);
            Assert.True(await players.HasTabAsync("overview"));
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Players_FixturePlayer_ProfileLoads), ex); }
    }

    [Fact]
    public async Task Players_Goalkeeper_ProfileLoads()
    {
        if (!E2EConfig.HasCredentials) Assert.Skip("HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        if (!E2EConfig.HasGoalkeeper) Assert.Skip("HANDWSTAT_E2E_GOALKEEPER not configured");
        try
        {
            await LoginAsync();
            var players = new PlayersPage(Page, BaseUrl);
            await players.NavigateAsync();
            await players.SelectPlayerByNameAsync(E2EConfig.GoalkeeperName!);
            Assert.True(await players.HasTabAsync("overview"));
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Players_Goalkeeper_ProfileLoads), ex); }
    }
}
