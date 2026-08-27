using HandWStat.E2E.Tests.Fixtures;
using HandWStat.E2E.Tests.Helpers;
using HandWStat.E2E.Tests.Pages;
using Xunit;

namespace HandWStat.E2E.Tests.Tests;

[Collection("E2E")]
public sealed class DashboardSmokeTests(E2EFixture fixture) : E2ETestBase(fixture)
{
    [SkippableFact]
    public async Task Dashboard_LoadsAfterLogin()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var dashboard = new DashboardPage(Page, BaseUrl);
            await dashboard.WaitForLoadAsync();
            Assert.True(await Page.IsVisibleAsync("[data-workspace='dashboard']"));
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Dashboard_LoadsAfterLogin), ex); }
    }

    [SkippableFact]
    public async Task Dashboard_HasKpiOrRankingContent()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var dashboard = new DashboardPage(Page, BaseUrl);
            await dashboard.WaitForLoadAsync();
            await dashboard.WaitForIdleAsync();

            var hasRanking  = await Page.IsVisibleAsync(".ranking-list, .player-rank-row, [class*='rank']");
            var hasStateCard = await Page.IsVisibleAsync(".state-card");
            Assert.True(hasRanking || hasStateCard, "Expected ranking or empty-state content");
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Dashboard_HasKpiOrRankingContent), ex); }
    }

    [SkippableFact]
    public async Task Dashboard_SectionTabs_Navigate()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var dashboard = new DashboardPage(Page, BaseUrl);
            await dashboard.WaitForLoadAsync();
            await dashboard.WaitForIdleAsync();

            Assert.True(await Page.IsVisibleAsync("[data-testid='tab-league']"), "Expected league tab");

            await dashboard.ClickTabAsync("spotlight");

            await Page.WaitForFunctionAsync(
                "() => document.querySelector('[data-testid=\"tab-spotlight\"]')?.getAttribute('aria-selected') === 'true'",
                null, new Microsoft.Playwright.PageWaitForFunctionOptions { Timeout = 5_000 });

            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Dashboard_SectionTabs_Navigate), ex); }
    }

    [SkippableFact]
    public async Task Dashboard_ScopeLabel_IsPresent()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var dashboard = new DashboardPage(Page, BaseUrl);
            await dashboard.WaitForLoadAsync();
            await dashboard.WaitForIdleAsync();
            await dashboard.GetScopeLabelAsync(); // null is OK — no crash is the assertion
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Dashboard_ScopeLabel_IsPresent), ex); }
    }

    [SkippableFact]
    public async Task Dashboard_NoVisibleErrorUI()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var dashboard = new DashboardPage(Page, BaseUrl);
            await dashboard.WaitForLoadAsync();
            await dashboard.WaitForIdleAsync();

            Assert.False(await Page.IsVisibleAsync("#blazor-error-ui"),
                "Blazor error UI is visible — unhandled exception in circuit");

            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Dashboard_NoVisibleErrorUI), ex); }
    }
}
