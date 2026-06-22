namespace HandWStat.Components.Shared;

public static class BusyUiHelper
{
    public static readonly TimeSpan MinimumBusyDuration = TimeSpan.FromMilliseconds(300);

    public static async Task EnterAsync(Action setBusy, Func<Task> renderAsync)
    {
        setBusy();
        await renderAsync();
        await Task.Yield();
    }

    public static async Task ExitAsync(
        Action clearBusy,
        Func<Task> renderAsync,
        DateTimeOffset startedAt,
        TimeSpan? minimumDuration = null)
    {
        var duration = minimumDuration ?? MinimumBusyDuration;
        var elapsed = DateTimeOffset.UtcNow - startedAt;

        if (elapsed < duration)
        {
            await Task.Delay(duration - elapsed);
        }

        clearBusy();
        await renderAsync();
    }
}
