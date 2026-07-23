using System.Net;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using HandballManagerCore.DTO;
using HandWStat.Configuration;
using HandWStat.Models.Updates;
using HandWStat.Services.Updates;

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
        Assert.Equal("https://handballwstat.ddnsfree.com/releases/HandWStat.msix", launcher.LastUri?.ToString());
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
    }

    private static AppUpdateService CreateService(ClientUpdateCheckResponseDto response)
    {
        return CreateService(new StubHandler(request =>
            request.RequestUri!.AbsolutePath.EndsWith("/check", StringComparison.Ordinal)
                ? JsonResponse(response)
                : new HttpResponseMessage(HttpStatusCode.Accepted)), new FakeLauncher());
    }

    private static AppUpdateService CreateService(HttpMessageHandler handler, IExternalLauncher launcher)
    {
        return new AppUpdateService(
            new HttpClient(handler),
            new ApiSettings { BaseUrl = "https://api.example.test/api/" },
            new FakeVersionProvider(),
            new FakeDeviceIdentifierProvider(),
            launcher);
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
}
