using HandWStat.Models.Updates;

namespace HandWStat.Services.Updates;

public interface IAppUpdateService
{
    AppUpdateState State { get; }

    event Action? StateChanged;

    Task CheckAsync(CancellationToken cancellationToken = default);

    Task<bool> OpenDownloadAsync(CancellationToken cancellationToken = default);

    void DismissOptionalUpdate();
}

