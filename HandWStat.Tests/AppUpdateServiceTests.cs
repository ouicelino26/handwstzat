using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using HandballManagerCore.DTO;
using HandWStat.Configuration;
using HandWStat.Models.Updates;
using HandWStat.Services.Updates;
using Microsoft.Extensions.Logging.Abstractions;

namespace HandWStat.Tests;

public sealed class AppUpdateServiceTests
{
    [Fact]
    public void VersionProvider_ReadsAndMapsInstalledVersion()
    {
        var provider = new AppVersionProvider(
            () => "1.4.2",
            () => "42",
            () => "WinUI",
            () => Architecture.X64);

        var version = provider.Current;

        Assert.Equal(new AppVersionInfo("1.4.2", 42, "WINDOWS", "X64"), version);
    }

    [Theory]
    [InlineData(Architecture.X64, "X64")]
    [InlineData(Architecture.X86, "X86")]
    [InlineData(Architecture.Arm64, "ARM64")]
    [InlineData(Architecture.Arm, "ARM")]
    public void ArchitectureMapping_IsStable(Architecture architecture, string expected)
    {
        Assert.Equal(expected, AppVersionProvider.MapArchitecture(architecture));
    }

    [Fact]
    public async Task NoUpdate_LeavesApplicationUnblocked()
    {
        var service = CreateService(Response(updateAvailable: false));

        await service.CheckAsync();

        Assert.False(service.State.IsMandatory);
        Assert.False(service.State.HasOptionalUpdate);
    }

    [Fact]
    public async Task OptionalUpdate_CanBeDismissed()
    {
        var service = CreateService(Response(updateAvailable: true, mandatory: false));
        await service.CheckAsync();

        Assert.True(service.State.HasOptionalUpdate);
        service.DismissOptionalUpdate();

        Assert.False(service.State.HasOptionalUpdate);
    }

    [Fact]
    public async Task DismissedOptionalUpdate_IsSuppressedUntilManualCheck()
    {
        var preferences = new FakePreferences();
        var first = CreateService(Response(updateAvailable: true), preferences);
        await first.CheckAsync();
        first.DismissOptionalUpdate();

        var automatic = CreateService(Response(updateAvailable: true), preferences);
        await automatic.CheckAsync();
        Assert.False(automatic.State.HasOptionalUpdate);

        await automatic.CheckAsync(ignoreDismissedUpdate: true);
        Assert.True(automatic.State.HasOptionalUpdate);
    }

    [Fact]
    public async Task MandatoryUpdate_BlocksApplication()
    {
        var service = CreateService(Response(updateAvailable: true, mandatory: true));

        await service.CheckAsync();

        Assert.True(service.State.IsMandatory);
    }

    [Fact]
    public async Task UnavailableApi_DoesNotBlockWithoutPriorMandatoryDecision()
    {
        var handler = new StubHandler(_ => throw new HttpRequestException("offline"));
        var service = CreateService(handler, new FakeLauncher());

        await service.CheckAsync();

        Assert.False(service.State.IsMandatory);
        Assert.NotNull(service.State.ErrorMessage);
    }

    [Fact]
    public async Task InvalidDownloadUrl_IsRejected()
    {
        var update = Response(updateAvailable: true, mandatory: false);
        update.DownloadUrl = "file:///tmp/application.msix";
        var service = CreateService(update);
        await service.CheckAsync();

        var opened = await service.OpenDownloadAsync();

        Assert.False(opened);
    }

    [Fact]
    public async Task DownloadAction_UsesExternalLauncher()
    {
        var launcher = new FakeLauncher();
        var service = CreateService(new StubHandler(request =>
            request.RequestUri!.AbsolutePath.EndsWith("/check", StringComparison.Ordinal)
                ? JsonResponse(Response(updateAvailable: true, mandatory: false))
                : new HttpResponseMessage(HttpStatusCode.Accepted)), launcher);
        await service.CheckAsync();

        var opened = await service.OpenDownloadAsync();

        Assert.True(opened);
        Assert.True(launcher.LastUri?.IsFile);
    }

    [Fact]
    public async Task VersionHeaders_AreAddedToEveryApiRequest()
    {
        HttpRequestMessage? captured = null;
        var inner = new StubHandler(request =>
        {
            captured = request;
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        var client = new HttpClient(new HandWStatVersionHandler(new FakeVersionProvider(), inner));

        await client.GetAsync("https://api.example.test/api/system/version");

        Assert.Equal("1.4.2", captured!.Headers.GetValues("X-HandWStat-Version").Single());
        Assert.Equal("42", captured.Headers.GetValues("X-HandWStat-Build").Single());
        Assert.Equal("WINDOWS", captured.Headers.GetValues("X-HandWStat-Platform").Single());
        Assert.Equal("X64", captured.Headers.GetValues("X-HandWStat-Architecture").Single());
        Assert.Equal("STABLE", captured.Headers.GetValues("X-HandWStat-Channel").Single());
    }

    [Theory]
    [InlineData("http://downloads.example.test/app.msix", "WINDOWS", "X64", "MSIX", 1024, true)]
    [InlineData("/app.msix", "WINDOWS", "X64", "MSIX", 1024, true)]
    [InlineData("file:///tmp/app.msix", "WINDOWS", "X64", "MSIX", 1024, true)]
    [InlineData("https://localhost/app.msix", "WINDOWS", "X64", "MSIX", 1024, true)]
    [InlineData("https://downloads.example.test/app.msix", "ANDROID", "X64", "MSIX", 1024, true)]
    [InlineData("https://downloads.example.test/app.aab", "WINDOWS", "X64", "AAB", 1024, true)]
    [InlineData("https://downloads.example.test/app.msix", "WINDOWS", "X64", "MSIX", 0, true)]
    [InlineData("https://downloads.example.test/app.msix", "WINDOWS", "X64", "MSIX", 1024, false)]
    public async Task InvalidArtifact_IsRejected(
        string url,
        string platform,
        string architecture,
        string packageType,
        long size,
        bool validSha)
    {
        var response = Response(updateAvailable: true);
        response.DownloadUrl = url;
        response.Platform = platform;
        response.Architecture = architecture;
        response.PackageType = packageType;
        response.FileName = packageType == "AAB" ? "app.aab" : "app.msix";
        response.FileSizeBytes = size;
        response.Sha256 = validSha ? new string('A', 64) : "invalid";

        var service = CreateService(response);
        await service.CheckAsync();

        Assert.Equal(AppUpdateStatus.Error, service.State.Status);
        Assert.False(await service.OpenDownloadAsync());
    }

    [Fact]
    public async Task InvalidJson_IsHandled()
    {
        var handler = new StubHandler(request => request.RequestUri!.AbsolutePath.EndsWith("/check")
            ? new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("{invalid", System.Text.Encoding.UTF8, "application/json") }
            : new HttpResponseMessage(HttpStatusCode.Accepted));
        var service = CreateService(handler, new FakeLauncher());

        await service.CheckAsync();

        Assert.Equal(AppUpdateStatus.Error, service.State.Status);
    }

    [Fact]
    public async Task TelemetryFailure_DoesNotBlockUpdateDecision()
    {
        var handler = new StubHandler(request => request.RequestUri!.AbsolutePath.EndsWith("/check")
            ? JsonResponse(Response(updateAvailable: true))
            : throw new HttpRequestException("telemetry offline"));
        var service = CreateService(handler, new FakeLauncher());

        await service.CheckAsync();

        Assert.True(service.State.HasOptionalUpdate);
    }

    private static AppUpdateService CreateService(
        ClientUpdateCheckResponseDto response,
        FakePreferences? preferences = null)
    {
        return CreateService(new StubHandler(request =>
            request.RequestUri!.AbsolutePath.EndsWith("/check", StringComparison.Ordinal)
                ? JsonResponse(response)
                : new HttpResponseMessage(HttpStatusCode.Accepted)), new FakeLauncher(), preferences);
    }

    private static AppUpdateService CreateService(
        HttpMessageHandler handler,
        IExternalLauncher launcher,
        FakePreferences? preferences = null)
    {
        return new AppUpdateService(
            new HttpClient(handler),
            new ApiSettings { BaseUrl = "https://api.example.test/api/" },
            new UpdateSettings(),
            new FakeVersionProvider(),
            new FakeDeviceIdentifierProvider(),
            launcher,
            new FakeDownloader(),
            preferences ?? new FakePreferences(),
            NullLogger<AppUpdateService>.Instance);
    }

    private static ClientUpdateCheckResponseDto Response(bool updateAvailable, bool mandatory = false) => new()
    {
        ReleaseId = 7,
        UpdateAvailable = updateAvailable,
        Mandatory = mandatory,
        CurrentBuildBlocked = mandatory,
        LatestVersion = "1.4.2",
        LatestBuild = 42,
        MinimumSupportedBuild = 30,
        DownloadUrl = "https://handballwstat.ddnsfree.com/releases/HandWStat.msix",
        FileName = "HandWStat.msix",
        Platform = "WINDOWS",
        Architecture = "X64",
        PackageType = "MSIX",
        FileSizeBytes = 1024,
        Sha256 = new string('A', 64),
        ApiVersion = "1.0.0",
        DatabaseVersion = "1.0.0"
    };

    private static HttpResponseMessage JsonResponse<T>(T value) => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(value)
    };

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(response(request));
        }
    }

    private sealed class FakeVersionProvider : IAppVersionProvider
    {
        public AppVersionInfo Current { get; } = new("1.4.2", 42, "WINDOWS", "X64");
    }

    private sealed class FakeDeviceIdentifierProvider : IDeviceIdentifierProvider
    {
        public string GetAnonymizedId() => new string('D', 64);
    }

    private sealed class FakeLauncher : IExternalLauncher
    {
        public Uri? LastUri { get; private set; }

        public Task<bool> OpenAsync(Uri uri)
        {
            LastUri = uri;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeDownloader : IUpdateArtifactDownloader
    {
        public Task<DownloadedArtifactResult> DownloadAsync(
            ReleaseArtifactDto artifact,
            IProgress<double>? progress,
            CancellationToken cancellationToken)
        {
            progress?.Report(100);
            return Task.FromResult(DownloadedArtifactResult.Verified(
                Path.GetFullPath($"verified-{artifact.FileName}")));
        }
    }

    private sealed class FakePreferences : IUpdatePreferenceStore
    {
        public DateTimeOffset? LastSuccessfulCheckUtc { get; set; }
        public string? IgnoredVersion { get; set; }
        public int? IgnoredBuild { get; set; }
        public string? LastStatus { get; set; }
    }
}
