using HandWStat.E2E.Tests.Fixtures;
using HandWStat.E2E.Tests.Helpers;
using HandWStat.E2E.Tests.Pages;
using Microsoft.Playwright;
using Xunit;

namespace HandWStat.E2E.Tests.Tests;

[Collection("E2E")]
public sealed class ExportSmokeTests(E2EFixture fixture) : E2ETestBase(fixture)
{
    [SkippableFact]
    public async Task Export_WizardLoads_AfterLogin()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var export = new ExportPage(Page, BaseUrl);
            await export.NavigateAsync();
            Assert.True(await Page.IsVisibleAsync(".wizard__steps"), "Expected export wizard");
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Export_WizardLoads_AfterLogin), ex); }
    }

    [SkippableFact]
    public async Task Export_WizardAdvancesThreeSteps()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var export = new ExportPage(Page, BaseUrl);
            await export.NavigateAsync();

            Assert.True(await Page.IsVisibleAsync("[data-testid='export-next-1']"), "Expected next on step 1");

            await export.ClickNextStep1Async();
            await Page.WaitForSelectorAsync("[data-testid='export-next-2']", new PageWaitForSelectorOptions
            {
                Timeout = 10_000
            });

            await export.ClickNextStep2Async();
            await export.WaitForGenerateButtonAsync();

            Assert.True(await Page.IsVisibleAsync("[data-testid='export-generate']"), "Expected generate button");
            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Export_WizardAdvancesThreeSteps), ex); }
    }

    [SkippableFact]
    public async Task Export_Download_ProducesFile()
    {
        Skip.If(!E2EConfig.HasCredentials, "HANDWSTAT_E2E_USERNAME / _PASSWORD not configured");
        try
        {
            await LoginAsync();
            var export = new ExportPage(Page, BaseUrl);
            await export.NavigateAsync();

            await export.ClickNextStep1Async();
            if (await Page.IsVisibleAsync("[data-testid='export-next-2']"))
                await export.ClickNextStep2Async();

            await export.WaitForGenerateButtonAsync(timeoutMs: 15_000);

            var download = await Page.RunAndWaitForDownloadAsync(async () =>
            {
                await export.ClickGenerateAsync();
            }, new PageRunAndWaitForDownloadOptions
            {
                Timeout = 120_000
            });

            var path = await download.PathAsync();
            Assert.False(string.IsNullOrWhiteSpace(path), "Expected a download path");

            var info = new FileInfo(path!);
            Assert.True(info.Exists, "Downloaded file does not exist");
            Assert.True(info.Length > 0, "Downloaded file is empty");

            var suggested = download.SuggestedFilename;
            Assert.True(
                suggested.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                suggested.EndsWith(".xls", StringComparison.OrdinalIgnoreCase),
                $"Expected .xlsx, got: {suggested}");

            Monitor.AssertNoErrors();
        }
        catch (Exception ex) { await FailWithArtifactsAsync(nameof(Export_Download_ProducesFile), ex); }
    }
}
