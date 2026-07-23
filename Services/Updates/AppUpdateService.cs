using System.Net.Http.Json;
using System.Text.Json;
using HandballManagerCore.DTO;
using HandWStat.Configuration;
using HandWStat.Models.Updates;

namespace HandWStat.Services.Updates;

public sealed class AppUpdateService(
    HttpClient httpClient,
    ApiSettings settings,
    IAppVersionProvider versionProvider,
    IDeviceIdentifierProvider deviceIdentifierProvider,
    IExternalLauncher launcher) : IAppUpdateService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public AppUpdateState State { get; private set; } = AppUpdateState.Initial;

    public event Action? StateChanged;

    public async Task CheckAsync(CancellationToken cancellationToken = default)
    {
        SetState(State with { IsChecking = true, ErrorMessage = null });
        var version = versionProvider.Current;
        var request = new ClientUpdateCheckRequestDto
        {
            Application = "HANDWSTAT",
            Platform = version.Platform,
            Architecture = version.Architecture,
            Channel = "STABLE",
            CurrentVersion = version.Version,
            CurrentBuild = version.Build,
            DeviceId = deviceIdentifierProvider.GetAnonymizedId()
        };

        try
        {
            using var response = await httpClient.PostAsJsonAsync(
                BuildUri("client-updates/check"),
                request,
                JsonOptions,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            var update = await response.Content.ReadFromJsonAsync<ClientUpdateCheckResponseDto>(
                JsonOptions,
                cancellationToken);

            SetState(new AppUpdateState(false, update, null, false));
            await TryRecordEventAsync("CHECKED", null, cancellationToken);
            if (update?.UpdateAvailable == true)
            {
                await TryRecordEventAsync("AVAILABLE", null, cancellationToken);
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            var confirmedMandatory = State.Response?.Mandatory == true ? State.Response : null;
            SetState(new AppUpdateState(
                false,
                confirmedMandatory,
                "La verification des mises a jour est momentanement indisponible.",
                false));
        }
    }

    public async Task<bool> OpenDownloadAsync(CancellationToken cancellationToken = default)
    {
        var url = State.Response?.DownloadUrl;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || uri.IsFile)
        {
            SetState(State with { ErrorMessage = "L'adresse de telechargement recue est invalide." });
            return false;
        }

        try
        {
            await TryRecordEventAsync("DOWNLOAD_STARTED", null, cancellationToken);
            return await launcher.OpenAsync(uri);
        }
        catch (Exception exception)
        {
            await TryRecordEventAsync("DOWNLOAD_FAILED", exception.Message, cancellationToken);
            SetState(State with { ErrorMessage = "Impossible d'ouvrir le telechargement." });
            return false;
        }
    }

    public void DismissOptionalUpdate()
    {
        if (State.Response?.Mandatory == true)
        {
            return;
        }

        SetState(State with { OptionalUpdateDismissed = true });
        _ = TryRecordEventAsync("DISMISSED", null, CancellationToken.None);
    }

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
                BuildUri("client-updates/events"),
                request,
                JsonOptions,
                cancellationToken);
        }
        catch
        {
            // Telemetry must never change the update decision or block application startup.
        }
    }

    private Uri BuildUri(string relativePath)
    {
        var baseUrl = string.IsNullOrWhiteSpace(settings.BaseUrl)
            ? ApiSettings.DefaultBaseUrl
            : settings.BaseUrl.Trim();
        return new Uri(new Uri(baseUrl.TrimEnd('/') + "/"), relativePath);
    }

    private void SetState(AppUpdateState state)
    {
        State = state;
        StateChanged?.Invoke();
    }
}
