using System.Net.Http.Json;
using System.Text.Json;
using HandballManagerCore.DTO;
using HandballManagerCore.Releases;
using HandWStat.Configuration;
using HandWStat.Models.Updates;
using Microsoft.Extensions.Logging;

namespace HandWStat.Services.Updates;

public sealed class AppUpdateService(
    HttpClient httpClient,
    ApiSettings apiSettings,
    UpdateSettings updateSettings,
    IAppVersionProvider versionProvider,
    IDeviceIdentifierProvider deviceIdentifierProvider,
    IExternalLauncher launcher,
    IUpdateArtifactDownloader downloader,
    IUpdatePreferenceStore preferences,
    ILogger<AppUpdateService> logger) : IAppUpdateService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SemaphoreSlim _checkGate = new(1, 1);

    public AppUpdateState State { get; private set; } = AppUpdateState.Initial;

    public event Action? StateChanged;

    public async Task CheckAsync(
        bool ignoreDismissedUpdate = false,
        CancellationToken cancellationToken = default)
    {
        if (!await _checkGate.WaitAsync(0, cancellationToken))
        {
            return;
        }

        try
        {
            SetState(State with
            {
                Status = AppUpdateStatus.Checking,
                ErrorMessage = null,
                DownloadProgress = 0
            });
            await TryRecordEventAsync("CHECK_STARTED", null, cancellationToken);

            var version = versionProvider.Current;
            var request = new ClientUpdateCheckRequestDto
            {
                Application = "HANDWSTAT",
                Platform = version.Platform,
                Architecture = version.Architecture,
                Channel = updateSettings.Channel,
                CurrentVersion = version.Version,
                CurrentBuild = version.Build,
                DeviceId = deviceIdentifierProvider.GetAnonymizedId()
            };

            using var response = await httpClient.PostAsJsonAsync(
                BuildUri("client-updates/check"), request, JsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            var update = await response.Content.ReadFromJsonAsync<ClientUpdateCheckResponseDto>(
                JsonOptions, cancellationToken);
            if (update is null)
            {
                throw new JsonException("Empty update response.");
            }

            if (!update.UpdateAvailable)
            {
                SetState(new AppUpdateState(AppUpdateStatus.UpToDate, update, null, false));
                await TryRecordEventAsync("CHECKED", null, cancellationToken);
                await TryRecordEventAsync("UP_TO_DATE", null, cancellationToken);
                return;
            }

            if (!TryBuildValidatedArtifact(update, version, out _))
            {
                SetState(new AppUpdateState(
                    AppUpdateStatus.Error,
                    null,
                    "La mise a jour retournee par l'API est invalide.",
                    false));
                return;
            }

            var dismissed = !update.Mandatory
                && !ignoreDismissedUpdate
                && string.Equals(preferences.IgnoredVersion, update.LatestVersion, StringComparison.OrdinalIgnoreCase)
                && preferences.IgnoredBuild == update.LatestBuild;
            var status = update.Mandatory
                ? AppUpdateStatus.MandatoryUpdateAvailable
                : AppUpdateStatus.OptionalUpdateAvailable;
            SetState(new AppUpdateState(status, update, null, dismissed));

            await TryRecordEventAsync("CHECKED", null, cancellationToken);
            await TryRecordEventAsync(update.Mandatory ? "MANDATORY" : "AVAILABLE", null, cancellationToken);
        }
        catch (Exception exception) when (
            exception is HttpRequestException
            or TaskCanceledException
            or JsonException
            or NotSupportedException)
        {
            var confirmedMandatory = State.Response?.Mandatory == true ? State.Response : null;
            SetState(new AppUpdateState(
                confirmedMandatory is null
                    ? AppUpdateStatus.Error
                    : AppUpdateStatus.MandatoryUpdateAvailable,
                confirmedMandatory,
                "La verification des mises a jour est momentanement indisponible.",
                false));
        }
        finally
        {
            _checkGate.Release();
        }
    }

    public async Task<bool> OpenDownloadAsync(CancellationToken cancellationToken = default)
    {
        var response = State.Response;
        var version = versionProvider.Current;
        if (response is null || !TryBuildValidatedArtifact(response, version, out var artifact))
        {
            SetState(State with { ErrorMessage = "L'artefact de mise a jour est invalide." });
            return false;
        }

        if (IsStoreLink(artifact.PackageType))
        {
            return await OpenExternalAsync(new Uri(artifact.DownloadUrl), cancellationToken);
        }

        var progressEvents = new HashSet<int>();
        var progress = new Progress<double>(value =>
        {
            var normalized = Math.Clamp(value, 0, 100);
            SetState(State with
            {
                Status = AppUpdateStatus.Downloading,
                DownloadProgress = normalized,
                ErrorMessage = null
            });
            var milestone = normalized >= 100 ? 100 : (int)(normalized / 25) * 25;
            if (progressEvents.Add(milestone))
            {
                _ = TryRecordEventAsync("DOWNLOAD_PROGRESS", null, CancellationToken.None);
            }
        });

        SetState(State with { Status = AppUpdateStatus.Downloading, DownloadProgress = 0, ErrorMessage = null });
        await TryRecordEventAsync("DOWNLOAD_STARTED", null, cancellationToken);
        var result = await downloader.DownloadAsync(artifact, progress, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.FilePath))
        {
            await TryRecordEventAsync("DOWNLOAD_FAILED", result.ErrorMessage, cancellationToken);
            SetState(State with { Status = AppUpdateStatus.Error, ErrorMessage = result.ErrorMessage });
            return false;
        }

        await TryRecordEventAsync("DOWNLOAD_COMPLETED", null, cancellationToken);
        await TryRecordEventAsync("DOWNLOAD_VERIFIED", null, cancellationToken);
        SetState(State with
        {
            Status = AppUpdateStatus.DownloadVerified,
            DownloadProgress = 100,
            DownloadedFilePath = result.FilePath,
            ErrorMessage = null
        });

        await TryRecordEventAsync("INSTALL_STARTED", null, cancellationToken);
        SetState(State with { Status = AppUpdateStatus.Installing });
        try
        {
            var opened = await launcher.OpenAsync(new Uri(result.FilePath));
            await TryRecordEventAsync(opened ? "INSTALL_HANDOFF" : "INSTALL_FAILED", null, cancellationToken);
            if (!opened)
            {
                SetState(State with { Status = AppUpdateStatus.Error, ErrorMessage = "Impossible d'ouvrir le package verifie." });
            }
            return opened;
        }
        catch
        {
            await TryRecordEventAsync("INSTALL_FAILED", null, cancellationToken);
            SetState(State with { Status = AppUpdateStatus.Error, ErrorMessage = "Impossible d'ouvrir le package verifie." });
            return false;
        }
    }

    public void DismissOptionalUpdate()
    {
        if (State.Response?.Mandatory == true)
        {
            return;
        }

        preferences.IgnoredVersion = State.Response?.LatestVersion;
        preferences.IgnoredBuild = State.Response?.LatestBuild;
        SetState(State with { OptionalUpdateDismissed = true });
        _ = TryRecordEventAsync("DISMISSED", null, CancellationToken.None);
    }

    private async Task<bool> OpenExternalAsync(Uri uri, CancellationToken cancellationToken)
    {
        try
        {
            await TryRecordEventAsync("DOWNLOAD_STARTED", null, cancellationToken);
            var opened = await launcher.OpenAsync(uri);
            await TryRecordEventAsync(opened ? "INSTALL_HANDOFF" : "INSTALL_FAILED", null, cancellationToken);
            return opened;
        }
        catch
        {
            await TryRecordEventAsync("DOWNLOAD_FAILED", null, cancellationToken);
            SetState(State with { Status = AppUpdateStatus.Error, ErrorMessage = "Impossible d'ouvrir le telechargement." });
            return false;
        }
    }

    private static bool TryBuildValidatedArtifact(
        ClientUpdateCheckResponseDto response,
        AppVersionInfo current,
        out ReleaseArtifactDto artifact)
    {
        artifact = new ReleaseArtifactDto
        {
            Platform = response.Platform ?? string.Empty,
            Architecture = response.Architecture ?? string.Empty,
            PackageType = response.PackageType ?? string.Empty,
            BuildNumber = response.LatestBuild ?? 0,
            MinimumSupportedBuild = response.MinimumSupportedBuild ?? 1,
            Mandatory = response.Mandatory,
            DownloadUrl = response.DownloadUrl ?? string.Empty,
            FileName = response.FileName ?? string.Empty,
            FileSizeBytes = response.FileSizeBytes ?? 0,
            Sha256 = response.Sha256 ?? string.Empty,
            MinimumOsVersion = response.MinimumOsVersion,
            SignatureThumbprint = response.SignatureThumbprint,
            Active = true
        };

        return string.Equals(artifact.Platform, current.Platform, StringComparison.OrdinalIgnoreCase)
            && (string.Equals(artifact.Architecture, current.Architecture, StringComparison.OrdinalIgnoreCase)
                || string.Equals(artifact.Architecture, "ANY", StringComparison.OrdinalIgnoreCase))
            && ReleaseArtifactValidator.TryValidate(
                new ReleaseArtifactValidationInput(
                    response.LatestVersion,
                    artifact.BuildNumber,
                    artifact.Platform,
                    artifact.Architecture,
                    artifact.PackageType,
                    artifact.DownloadUrl,
                    artifact.FileName,
                    artifact.FileSizeBytes,
                    artifact.Sha256),
                out _);
    }

    private static bool IsStoreLink(string packageType) =>
        packageType.EndsWith("STORE_URL", StringComparison.OrdinalIgnoreCase)
        || string.Equals(packageType, "APPSTORE", StringComparison.OrdinalIgnoreCase)
        || string.Equals(packageType, "TESTFLIGHT", StringComparison.OrdinalIgnoreCase);

    private async Task TryRecordEventAsync(
        string eventType,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            var version = versionProvider.Current;
            var request = new UpdateEventRequestDto
            {
                ReleaseId = State.Response?.ReleaseId,
                Application = "HANDWSTAT",
                Platform = version.Platform,
                DeviceId = deviceIdentifierProvider.GetAnonymizedId(),
                CurrentBuild = version.Build,
                TargetBuild = State.Response?.LatestBuild,
                EventType = eventType,
                ErrorMessage = errorMessage
            };

            using var response = await httpClient.PostAsJsonAsync(
                BuildUri("client-updates/events"), request, JsonOptions, cancellationToken);
        }
        catch
        {
            logger.LogDebug("Update telemetry event {EventType} could not be sent.", eventType);
        }
    }

    private Uri BuildUri(string relativePath)
    {
        var baseUrl = string.IsNullOrWhiteSpace(apiSettings.BaseUrl)
            ? ApiSettings.DefaultBaseUrl
            : apiSettings.BaseUrl.Trim();
        return new Uri(new Uri(baseUrl.TrimEnd('/') + "/"), relativePath);
    }

    private void SetState(AppUpdateState state)
    {
        State = state;
        StateChanged?.Invoke();
    }
}
