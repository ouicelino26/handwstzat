namespace HandWStat.Services.Updates;

public interface IUpdateCheckCoordinator
{
    Task CheckOnStartupAsync(CancellationToken cancellationToken = default);

    Task CheckOnResumeAsync(CancellationToken cancellationToken = default);

    Task CheckAfterLoginAsync(CancellationToken cancellationToken = default);

    Task CheckManuallyAsync(CancellationToken cancellationToken = default);
}
