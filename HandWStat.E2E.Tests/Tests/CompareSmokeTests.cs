using HandWStat.E2E.Tests.Fixtures;
using HandWStat.E2E.Tests.Helpers;
using HandWStat.E2E.Tests.Pages;
using Xunit;

namespace HandWStat.E2E.Tests.Tests;

[Collection("E2E")]
public sealed class CompareSmokeTests(E2EFixture fixture) : E2ETestBase(fixture)
{
    [SkippableFact]
    public async Task Compare_LoadsAfterLogin()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var compare = new ComparePage(Page, BaseUrl);
            await compare.NavigateAsync();
            Assert.True(await Page.IsVisibleAsync("[data-testid='cmp-slot-0']"), "Expected compare slot 0");
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Compare_LoadsAfterLogin), ex); }
    }

    [SkippableFact]
    public async Task Compare_TwoPlayers_ShowsResults()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var compare = new ComparePage(Page, BaseUrl);
            await compare.NavigateAsync();

            await compare.OpenSlotPickerAsync(0);
            await compare.SelectPlayerInPickerAsync(string.Empty);

            await compare.OpenSlotPickerAsync(1);
            await compare.SelectPlayerInPickerAsync(string.Empty);

            await compare.WaitForResultsAsync();
            Assert.True(await Page.IsVisibleAsync(".cmp-section"), "Expected compare section content");
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Compare_TwoPlayers_ShowsResults), ex); }
    }

    [SkippableFact]
    public async Task Compare_RadarChart_RendersWithFieldPlayers()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        Skip.If(!E2EConfig.HasFieldPlayer, "HANDWSTAT_E2E_FIELD_PLAYER not configured");
        try
        {
            await LoginAsync();
            var compare = new ComparePage(Page, BaseUrl);
            await compare.NavigateAsync();

            await compare.OpenSlotPickerAsync(0);
            await compare.SelectPlayerInPickerAsync(E2EConfig.FieldPlayerName!);

            await compare.OpenSlotPickerAsync(1);
            await compare.SelectPlayerInPickerAsync(string.Empty);

            await compare.WaitForResultsAsync();

            // E2.13 — wait for SVG render deterministically
            await Page.WaitForFunctionAsync(
                "() => document.querySelector('[data-testid=\"compare-radar\"] svg') !== null",
                null, new Microsoft.Playwright.PageWaitForFunctionOptions { Timeout = 15_000 });

            Assert.True(await compare.IsRadarRenderedAsync(), "Expected radar chart SVG");
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Compare_RadarChart_RendersWithFieldPlayers), ex); }
    }

    [SkippableFact]
    public async Task Compare_SecondPlayerReplaced_ChartRefreshes()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var compare = new ComparePage(Page, BaseUrl);
            await compare.NavigateAsync();

            await compare.OpenSlotPickerAsync(0);
            await compare.SelectPlayerInPickerAsync(string.Empty);
            await compare.OpenSlotPickerAsync(1);
            await compare.SelectPlayerInPickerAsync(string.Empty);
            await compare.WaitForResultsAsync();

            // Replace slot 1
            await compare.OpenSlotPickerAsync(1);
            await Page.WaitForSelectorAsync(".cmp-picker-player");
            var items = await Page.QuerySelectorAllAsync(".cmp-picker-player");
            await (items.Count > 1 ? items[^1] : items[0]).ClickAsync();

            await compare.WaitForResultsAsync(timeoutMs: 15_000);

            // Results panel remains present after swap
            Assert.True(await Page.IsVisibleAsync(".cmp-section"), "Expected compare section after replacement");
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Compare_SecondPlayerReplaced_ChartRefreshes), ex); }
    }

    [SkippableFact]
    public async Task Compare_GkVsField_ShowsIncompatibilityNotice()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        Skip.If(!E2EConfig.HasFieldPlayer || !E2EConfig.HasGoalkeeper,
            "Both HANDWSTAT_E2E_FIELD_PLAYER and HANDWSTAT_E2E_GOALKEEPER required");
        try
        {
            await LoginAsync();
            var compare = new ComparePage(Page, BaseUrl);
            await compare.NavigateAsync();

            await compare.OpenSlotPickerAsync(0);
            await compare.SelectPlayerInPickerAsync(E2EConfig.GoalkeeperName!);
            await compare.OpenSlotPickerAsync(1);
            await compare.SelectPlayerInPickerAsync(E2EConfig.FieldPlayerName!);
            await compare.WaitForResultsAsync();

            Assert.True(await compare.IsGkVsFieldNoticeVisibleAsync(),
                "Expected GK vs field incompatibility notice");
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Compare_GkVsField_ShowsIncompatibilityNotice), ex); }
    }
}
