using HandWStat.Configuration;

namespace HandWStat.Services.Updates;

public sealed class UpdateCheckCoordinator(
    IAppUpdateService updateService,
    IUpdatePreferenceStore preferences,
    UpdateSettings settings,
    TimeProvider timeProvider) : IUpdateCheckCoordinator
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    public Task CheckOnStartupAsync(CancellationToken cancellationToken = default) =>
        settings.Enabled && settings.CheckOnStartup
            ? RunAsync(force: false, cancellationToken)
            : Task.CompletedTask;

    public Task CheckOnResumeAsync(CancellationToken cancellationToken = default) =>
        settings.Enabled && settings.CheckOnResume
            ? RunAsync(force: false, cancellationToken)
            : Task.CompletedTask;

    public Task CheckAfterLoginAsync(CancellationToken cancellationToken = default) =>
        settings.Enabled ? RunAsync(force: false, cancellationToken) : Task.CompletedTask;

    public Task CheckManuallyAsync(CancellationToken cancellationToken = default) =>
        RunAsync(force: true, cancellationToken);

    private async Task RunAsync(bool force, CancellationToken cancellationToken)
    {
        if (!force && !IsDue())
        {
            return;
        }
        if (!await _gate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            if (!force && !IsDue())
            {
                return;
            }

            await updateService.CheckAsync(ignoreDismissedUpdate: force, cancellationToken);
            preferences.LastStatus = updateService.State.Status.ToString();
            if (string.IsNullOrWhiteSpace(updateService.State.ErrorMessage))
            {
                preferences.LastSuccessfulCheckUtc = timeProvider.GetUtcNow();
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private bool IsDue()
    {
        var lastCheck = preferences.LastSuccessfulCheckUtc;
        return !lastCheck.HasValue
            || timeProvider.GetUtcNow() - lastCheck.Value >= TimeSpan.FromHours(settings.CheckIntervalHours);
    }
}
