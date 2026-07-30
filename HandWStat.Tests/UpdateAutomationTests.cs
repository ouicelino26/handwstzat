using System.Net;
using System.Security.Cryptography;
using HandWStat.Configuration;
using HandWStat.Models.Contracts;
using HandWStat.Models.Updates;
using HandWStat.Services.Updates;

namespace HandWStat.Tests;

public sealed class UpdateAutomationTests
{
    [Fact]
    public async Task StartupCheck_RunsOnceAndPersistsSuccessTime()
    {
        var service = new FakeUpdateService();
        var preferences = new FakePreferences();
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 7, 28, 10, 0, 0, TimeSpan.Zero));
        var coordinator = Coordinator(service, preferences, clock);

        await coordinator.CheckOnStartupAsync();

        Assert.Equal(1, service.CheckCount);
        Assert.Equal(clock.GetUtcNow(), preferences.LastSuccessfulCheckUtc);
    }

    [Fact]
    public async Task ResumeCheck_RespectsConfiguredInterval()
    {
        var service = new FakeUpdateService();
        var preferences = new FakePreferences();
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 7, 28, 10, 0, 0, TimeSpan.Zero));
        var coordinator = Coordinator(service, preferences, clock);

        await coordinator.CheckOnStartupAsync();
        await coordinator.CheckOnResumeAsync();
        Assert.Equal(1, service.CheckCount);

        clock.Advance(TimeSpan.FromHours(12));
        await coordinator.CheckOnResumeAsync();
        Assert.Equal(2, service.CheckCount);
    }

    [Fact]
    public async Task ManualCheck_IgnoresIntervalAndDismissal()
    {
        var service = new FakeUpdateService();
        var preferences = new FakePreferences
        {
            LastSuccessfulCheckUtc = DateTimeOffset.UtcNow
        };
        var coordinator = Coordinator(service, preferences, new FakeTimeProvider(DateTimeOffset.UtcNow));

        await coordinator.CheckManuallyAsync();

        Assert.Equal(1, service.CheckCount);
        Assert.True(service.LastIgnoreDismissedUpdate);
    }

    [Fact]
    public async Task ConcurrentChecks_AreCollapsed()
    {
        var service = new FakeUpdateService(block: true);
        var coordinator = Coordinator(
            service,
            new FakePreferences(),
            new FakeTimeProvider(DateTimeOffset.UtcNow));

        var first = coordinator.CheckManuallyAsync();
        await service.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await coordinator.CheckManuallyAsync();
        service.Complete();
        await first;

        Assert.Equal(1, service.CheckCount);
    }

    [Fact]
    public async Task VerifiedDownload_WritesFinalFileOnlyAfterHashAndSizeMatch()
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes("verified handwstat package");
        var root = TemporaryDirectory();
        try
        {
            var downloader = Downloader(bytes, root);
            var artifact = Artifact(bytes);

            var result = await downloader.DownloadAsync(artifact, null, CancellationToken.None);

            Assert.True(result.Success, result.ErrorMessage);
            Assert.True(File.Exists(result.FilePath));
            Assert.Equal(bytes, await File.ReadAllBytesAsync(result.FilePath!));
            Assert.Empty(Directory.GetFiles(root, "*.tmp"));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task InvalidDownloadedSizeOrHash_IsDeleted(bool wrongSize, bool wrongHash)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes("package bytes");
        var root = TemporaryDirectory();
        try
        {
            var artifact = Artifact(bytes);
            if (wrongSize) artifact.FileSizeBytes++;
            if (wrongHash) artifact.Sha256 = new string('A', 64);

            var result = await Downloader(bytes, root).DownloadAsync(artifact, null, CancellationToken.None);

            Assert.False(result.Success);
            Assert.Empty(Directory.GetFiles(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task CancelledDownload_LeavesNoFile()
    {
        var root = TemporaryDirectory();
        try
        {
            var handler = new AsyncStubHandler((_, token) =>
                Task.FromCanceled<HttpResponseMessage>(token));
            var downloader = new UpdateArtifactDownloader(new HttpClient(handler), root);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            var result = await downloader.DownloadAsync(
                Artifact([1, 2, 3]), null, cancellation.Token);

            Assert.False(result.Success);
            Assert.Empty(Directory.GetFiles(root));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static UpdateCheckCoordinator Coordinator(
        IAppUpdateService service,
        IUpdatePreferenceStore preferences,
        TimeProvider clock) => new(
            service,
            preferences,
            new UpdateSettings
            {
                Enabled = true,
                CheckOnStartup = true,
                CheckOnResume = true,
                CheckIntervalHours = 12
            },
            clock);

    private static UpdateArtifactDownloader Downloader(byte[] bytes, string root)
    {
        return new UpdateArtifactDownloader(
            new HttpClient(new AsyncStubHandler((_, _) =>
            {
                var content = new ByteArrayContent(bytes);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
            })),
            root);
    }

    private static ReleaseArtifactDto Artifact(byte[] bytes) => new()
    {
        Platform = "WINDOWS",
        Architecture = "X64",
        PackageType = "MSIX",
        BuildNumber = 42,
        MinimumSupportedBuild = 1,
        DownloadUrl = "https://downloads.example.test/HandWStat.msix",
        FileName = "HandWStat.msix",
        FileSizeBytes = bytes.Length,
        Sha256 = Convert.ToHexString(SHA256.HashData(bytes))
    };

    private static string TemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"handwstat-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class FakeUpdateService(bool block = false) : IAppUpdateService
    {
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public AppUpdateState State { get; private set; } = new(AppUpdateStatus.UpToDate, null, null, false);
        public int CheckCount { get; private set; }
        public bool LastIgnoreDismissedUpdate { get; private set; }
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public event Action? StateChanged;

        public async Task CheckAsync(bool ignoreDismissedUpdate = false, CancellationToken cancellationToken = default)
        {
            CheckCount++;
            LastIgnoreDismissedUpdate = ignoreDismissedUpdate;
            Started.TrySetResult();
            if (block) await _release.Task.WaitAsync(cancellationToken);
            StateChanged?.Invoke();
        }

        public Task<bool> OpenDownloadAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
        public void DismissOptionalUpdate() { }
        public void Complete() => _release.TrySetResult();
    }

    private sealed class FakePreferences : IUpdatePreferenceStore
    {
        public DateTimeOffset? LastSuccessfulCheckUtc { get; set; }
        public string? IgnoredVersion { get; set; }
        public int? IgnoredBuild { get; set; }
        public string? LastStatus { get; set; }
    }

    private sealed class FakeTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        private DateTimeOffset _utcNow = utcNow;
        public override DateTimeOffset GetUtcNow() => _utcNow;
        public void Advance(TimeSpan duration) => _utcNow += duration;
    }

    private sealed class AsyncStubHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> response) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) => response(request, cancellationToken);
    }
}
