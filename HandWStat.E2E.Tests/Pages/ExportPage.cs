using Microsoft.Playwright;

namespace HandWStat.E2E.Tests.Pages;

/// <summary>Page object for /export.</summary>
public sealed class ExportPage(IPage page, string baseUrl)
{
    public async Task NavigateAsync()
    {
        await page.GotoAsync(baseUrl + "/export");
        // Wait for step 1 (wizard) or access-required card
        await page.WaitForSelectorAsync(".wizard__steps, .access-required", new PageWaitForSelectorOptions
        {
            Timeout = 15_000
        });
    }

    /// <summary>Advances from step 1 to step 2.</summary>
    public Task ClickNextStep1Async()
        => page.ClickAsync("[data-testid='export-next-1']");

    /// <summary>Advances from step 2 to step 3.</summary>
    public Task ClickNextStep2Async()
        => page.ClickAsync("[data-testid='export-next-2']");

    /// <summary>Clicks the generate button on step 3.</summary>
    public Task ClickGenerateAsync()
        => page.ClickAsync("[data-testid='export-generate']");

    /// <summary>Returns the active step number (1, 2, or 3) based on the wizard CSS.</summary>
    public async Task<int> GetActiveStepAsync()
    {
        for (int i = 3; i >= 1; i--)
        {
            var steps = await page.QuerySelectorAllAsync(".wizard__step");
            foreach (var step in steps)
            {
                var cls = await step.GetAttributeAsync("class") ?? string.Empty;
                var num = await step.QuerySelectorAsync(".wizard__step-num");
                if (num is not null && cls.Contains("wizard__step--active"))
                {
                    var text = await num.TextContentAsync();
                    if (int.TryParse(text?.Trim(), out var n)) return n;
                }
            }
        }
        return 1;
    }

    /// <summary>Waits until the current step number matches expected.</summary>
    public async Task WaitForStepAsync(int step, int timeoutMs = 10_000)
    {
        var end = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < end)
        {
            if (await GetActiveStepAsync() == step) return;
            await Task.Delay(200);
        }
        throw new TimeoutException($"Export wizard did not advance to step {step}");
    }

    public async Task WaitForGenerateButtonAsync(int timeoutMs = 10_000)
    {
        await page.WaitForSelectorAsync("[data-testid='export-generate']", new PageWaitForSelectorOptions
        {
            Timeout = timeoutMs
        });
    }
}
