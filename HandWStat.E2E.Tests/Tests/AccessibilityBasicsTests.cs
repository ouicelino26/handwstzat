using HandWStat.E2E.Tests.Fixtures;
using HandWStat.E2E.Tests.Helpers;
using HandWStat.E2E.Tests.Pages;
using Xunit;

namespace HandWStat.E2E.Tests.Tests;

[Collection("E2E")]
public sealed class AccessibilityBasicsTests(E2EFixture fixture) : E2ETestBase(fixture)
{
    [Fact]
    public async Task Dashboard_NavItems_HaveAccessibleNames()
    {
        if (!E2EConfig.HasCredentials) Assert.Skip("HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var dashboard = new DashboardPage(Page, BaseUrl);
            await dashboard.WaitForLoadAsync();

            var navItems = await Page.QuerySelectorAllAsync("[data-testid^='nav-']");
            Assert.True(navItems.Count > 0, "Expected nav items");

            foreach (var item in navItems)
            {
                var label = await item.GetAttributeAsync("aria-label");
                Assert.False(string.IsNullOrWhiteSpace(label),
                    $"Nav item missing aria-label: {await item.GetAttributeAsync("data-testid")}");
            }

            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Dashboard_NavItems_HaveAccessibleNames), ex); }
    }

    [Fact]
    public async Task Dashboard_TabButtons_HaveRoles()
    {
        if (!E2EConfig.HasCredentials) Assert.Skip("HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var dashboard = new DashboardPage(Page, BaseUrl);
            await dashboard.WaitForLoadAsync();
            await dashboard.WaitForIdleAsync();

            var tabs = await Page.QuerySelectorAllAsync("[data-testid^='tab-'][role='tab']");
            Assert.True(tabs.Count > 0, "Expected tab buttons with role=tab");
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Dashboard_TabButtons_HaveRoles), ex); }
    }

    [Fact]
    public async Task LoginPage_Inputs_HavePlaceholders()
    {
        // No credentials required — tests the public login page
        try
        {
            await Page.GotoAsync(BaseUrl + "/");
            await Page.WaitForSelectorAsync("[data-testid='login-submit']");

            var usernameInput = await Page.WaitForSelectorAsync("[data-testid='login-username']");
            var placeholder = await usernameInput!.GetAttributeAsync("placeholder");
            Assert.False(string.IsNullOrWhiteSpace(placeholder), "Username input missing placeholder");

            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(LoginPage_Inputs_HavePlaceholders), ex); }
    }
}
