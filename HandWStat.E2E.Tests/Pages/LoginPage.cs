using HandWStat.E2E.Tests.Helpers;
using Microsoft.Playwright;

namespace HandWStat.E2E.Tests.Pages;

/// <summary>Encapsulates the login page (/) interactions.</summary>
public sealed class LoginPage(IPage page, string baseUrl)
{
    public async Task NavigateAsync()
    {
        await page.GotoAsync(baseUrl + "/");
        await page.WaitForSelectorAsync("[data-testid='login-submit']");
    }

    public async Task LoginAsync(string username, string password)
    {
        await page.FillAsync("[data-testid='login-username']", username);
        await page.FillAsync("[data-testid='login-password']", password);
        await page.ClickAsync("[data-testid='login-submit']");
    }

    /// <summary>
    /// Full login flow: navigate → fill form → submit → wait for redirect to /dashboard.
    /// Caller must ensure credentials are available before calling.
    /// </summary>
    public async Task LoginAndWaitForDashboardAsync()
    {
        await NavigateAsync();
        await LoginAsync(E2EConfig.Username!, E2EConfig.Password!);

        await page.WaitForURLAsync(url => url.Contains("/dashboard"), new PageWaitForURLOptions
        {
            Timeout = 15_000
        });
    }
}
