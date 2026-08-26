using HandWStat.E2E.Tests.Fixtures;
using HandWStat.E2E.Tests.Helpers;
using HandWStat.E2E.Tests.Pages;
using Xunit;

namespace HandWStat.E2E.Tests.Tests;

[Collection("E2E")]
public sealed class ResponsiveSmokeTests(E2EFixture fixture) : E2ETestBase(fixture)
{
    private static readonly (int W, int H, string Label)[] Viewports =
    [
        (1440, 900, "desktop"),
        (430,  900, "mobile"),
    ];

    [Fact]
    public async Task Dashboard_RendersOnAllViewports()
    {
        if (!E2EConfig.HasCredentials) Assert.Skip("HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            foreach (var (w, h, label) in Viewports)
            {
                await Page.SetViewportSizeAsync(w, h);

                await LoginAsync();

                var dashboard = new DashboardPage(Page, BaseUrl);
                await dashboard.WaitForLoadAsync();

                Assert.True(await Page.IsVisibleAsync("[data-workspace='dashboard']"),
                    $"Dashboard not visible at {label} ({w}x{h})");
                Assert.True(await Page.IsVisibleAsync(".studio-domain-nav, .studio-nav-item"),
                    $"Navigation not present at {label} ({w}x{h})");
                Assert.False(await Page.IsVisibleAsync("#blazor-error-ui"),
                    $"Blazor error UI at {label} ({w}x{h})");

                // Return to login for next iteration
                await Page.GotoAsync(BaseUrl + "/");
                await Page.WaitForSelectorAsync("[data-testid='login-submit']");
            }

            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Dashboard_RendersOnAllViewports), ex); }
    }

    [Fact]
    public async Task Players_RendersOnAllViewports()
    {
        if (!E2EConfig.HasCredentials) Assert.Skip("HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            foreach (var (w, h, label) in Viewports)
            {
                await Page.SetViewportSizeAsync(w, h);

                await LoginAsync();

                var players = new PlayersPage(Page, BaseUrl);
                await players.NavigateAsync();

                Assert.True(await Page.IsVisibleAsync("[data-testid='player-row']"),
                    $"No player rows at {label} ({w}x{h})");
                Assert.False(await Page.IsVisibleAsync("#blazor-error-ui"),
                    $"Blazor error UI at {label} ({w}x{h})");

                await Page.GotoAsync(BaseUrl + "/");
                await Page.WaitForSelectorAsync("[data-testid='login-submit']");
            }

            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Players_RendersOnAllViewports), ex); }
    }
}
