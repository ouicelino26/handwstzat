using HandWStat.E2E.Tests.Fixtures;
using HandWStat.E2E.Tests.Helpers;
using HandWStat.E2E.Tests.Pages;
using Xunit;

namespace HandWStat.E2E.Tests.Tests;

[Collection("E2E")]
public sealed class PlayerSwitchTests(E2EFixture fixture) : E2ETestBase(fixture)
{
    [SkippableFact]
    public async Task Players_RapidSwitch_FinalProfileIsCorrect()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var players = new PlayersPage(Page, BaseUrl);
            await players.NavigateAsync();

            var allNames = await players.GetPlayerNamesAsync();
            if (allNames.Count < 3) return; // Not enough players; test not applicable

            var rows = await Page.QuerySelectorAllAsync("[data-testid='player-row']");

            // A → B (rapid, no wait)
            await rows[0].ClickAsync();
            await rows[1].ClickAsync();
            await players.WaitForDetailPaneAsync();

            var selectedId = await players.GetSelectedPlayerIdAsync();
            var expectedId = await rows[1].GetAttributeAsync("data-player-id");
            Assert.Equal(expectedId, selectedId);

            // A → B → C
            await rows[0].ClickAsync();
            await rows[1].ClickAsync();
            await rows[2].ClickAsync();
            await players.WaitForDetailPaneAsync();

            selectedId = await players.GetSelectedPlayerIdAsync();
            expectedId = await rows[2].GetAttributeAsync("data-player-id");
            Assert.Equal(expectedId, selectedId);

            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Players_RapidSwitch_FinalProfileIsCorrect), ex); }
    }
}
