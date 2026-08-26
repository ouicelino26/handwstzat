using Xunit;

namespace HandWStat.E2E.Tests.Fixtures;

/// <summary>
/// Top-level collection fixture: owns WebHost + Browser lifecycle.
/// All E2E test classes share this fixture via [Collection("E2E")].
/// </summary>
public sealed class E2EFixture : IAsyncLifetime
{
    public WebHostFixture  WebHost { get; } = new();
    public BrowserFixture  Browser { get; } = new();

    public async Task InitializeAsync()
    {
        await WebHost.InitializeAsync();
        await Browser.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        await WebHost.DisposeAsync();
    }
}

[CollectionDefinition("E2E")]
public class E2ECollection : ICollectionFixture<E2EFixture> { }
