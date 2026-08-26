using ApexCharts;
using HandWStat;
using HandWStat.Configuration;
using HandWStat.Services;
using HandWStat.Services.Analytics;
using HandWStat.Services.Api;
using HandWStat.Services.Updates;
using HandWStat.Web.TestHost.WebHostServices;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// ── Blazor Web App with Interactive Server ────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ── ApexCharts (web variant) ──────────────────────────────────────────────────
builder.Services.AddApexCharts();

// ── Configuration (no MAUI FileSystem — standard IConfiguration) ─────────────
var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>() ?? new ApiSettings();
var updateSettings = builder.Configuration.GetSection("UpdateSettings").Get<UpdateSettings>() ?? new UpdateSettings();
builder.Services.AddSingleton(apiSettings);
builder.Services.AddSingleton(updateSettings);
builder.Services.AddSingleton(TimeProvider.System);

// ── MAUI service stubs ────────────────────────────────────────────────────────
builder.Services.AddSingleton<IAppVersionProvider, WebAppVersionProvider>();
builder.Services.AddSingleton<IDeviceIdentifierProvider, WebDeviceIdentifierProvider>();
builder.Services.AddSingleton<IExternalLauncher, WebExternalLauncher>();
builder.Services.AddSingleton<IUpdatePreferenceStore, WebUpdatePreferenceStore>();
builder.Services.AddSingleton<IAppAssetReader, WebAppAssetReader>();

// ── HTTP pipeline ─────────────────────────────────────────────────────────────
builder.Services.AddSingleton<HandWStatVersionHandler>();
builder.Services.AddSingleton(sp => new HttpClient(
    sp.GetRequiredService<HandWStatVersionHandler>(),
    disposeHandler: false)
{
    Timeout = TimeSpan.FromSeconds(30)
});

// ── Update infrastructure ─────────────────────────────────────────────────────
builder.Services.AddSingleton<IUpdateArtifactDownloader>(sp =>
    new UpdateArtifactDownloader(
        sp.GetRequiredService<HttpClient>(),
        Path.Combine(Path.GetTempPath(), "HandWStat", "Updates")));
builder.Services.AddSingleton<IAppUpdateService, AppUpdateService>();
builder.Services.AddSingleton<IUpdateCheckCoordinator, UpdateCheckCoordinator>();

// ── API clients ───────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IApiAuthService, ApiAuthService>();
builder.Services.AddSingleton<CompetitionsApiClient>();
builder.Services.AddSingleton<TeamsApiClient>();
builder.Services.AddSingleton<LookupsApiClient>();
builder.Services.AddSingleton<PlayersApiClient>();
builder.Services.AddSingleton<MatchesApiClient>();
builder.Services.AddSingleton<MatchEventsApiClient>();
builder.Services.AddSingleton<StatsApiClient>();
builder.Services.AddSingleton<ExportApiClient>();

// ── Analytics ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IAnalyticsGateway, V1AnalyticsGateway>();
builder.Services.AddSingleton<ILeagueAnalyticsGateway, V2AnalyticsGateway>();
builder.Services.AddSingleton<LeaguePlayerAnalyticsService>();

// ── Business services ─────────────────────────────────────────────────────────
builder.Services.AddSingleton<ReferenceDataService>();
builder.Services.AddSingleton<TeamOfTheDayService>();
builder.Services.AddSingleton<DashboardSnapshotBuilder>();
builder.Services.AddSingleton<StatsDashboardService>();
builder.Services.AddSingleton<AnalysisScopeService>();
builder.Services.AddSingleton<GlobalSearchService>();
builder.Services.AddSingleton<CommandBarService>();

builder.Logging.AddConsole();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

app.Run();

// Exposed so E2E tests can reference Program via WebApplicationFactory<Program>
public partial class Program { }
