using System.Net;
using HandWStat.E2E.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HandWStat.E2E.Tests.Fixtures;

/// <summary>
/// Starts the HandWStat.Web.TestHost in-process on a real Kestrel port.
/// The host is started once per test collection and stopped on dispose.
/// </summary>
public sealed class WebHostFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    public string ServerUrl { get; private set; } = string.Empty;

    private IHost? _kestrelHost;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        // Point web root at the output directory's wwwroot — the Content items from HandWStat
        // are copied there via CopyToOutputDirectory="PreserveNewest".
        var webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        if (Directory.Exists(webRoot))
            builder.UseWebRoot(webRoot);

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiSettings:BaseUrl"] = E2EConfig.ApiBaseUrl,
                ["UpdateSettings:Enabled"] = "false",
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Build the in-memory test host required by WebApplicationFactory internals
        var testHost = builder.Build();

        // Build a second host that uses real Kestrel so Playwright can connect
        builder.ConfigureWebHost(b =>
            b.UseKestrel(k => k.Listen(IPAddress.Loopback, 0)));

        _kestrelHost = builder.Build();
        _kestrelHost.Start();

        var server = _kestrelHost.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()!.Addresses;
        ServerUrl = addresses.First().TrimEnd('/');

        return testHost;
    }

    public Task InitializeAsync()
    {
        // Force the factory to start (CreateHost is called lazily on first use)
        var _ = Server;
        return Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        if (_kestrelHost is not null)
        {
            await _kestrelHost.StopAsync(TimeSpan.FromSeconds(5));
            _kestrelHost.Dispose();
        }
        await base.DisposeAsync();
    }
}
