using ApexCharts;
using HandWStat.Configuration;
using HandWStat.Services;
using HandWStat.Services.Api;
using HandWStat.Services.Updates;
using Microsoft.Extensions.Logging;

namespace HandWStat;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var apiSettings = AppSettingsLoader.LoadApiSettings();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddApexChartsMaui();
        builder.Services.AddSingleton(apiSettings);
        builder.Services.AddSingleton<IAppVersionProvider, MauiAppVersionProvider>();
        builder.Services.AddSingleton<IDeviceIdentifierProvider, DeviceIdentifierProvider>();
        builder.Services.AddSingleton<IExternalLauncher, MauiExternalLauncher>();
        builder.Services.AddSingleton<HandWStatVersionHandler>();
        builder.Services.AddSingleton(serviceProvider => new HttpClient(
            serviceProvider.GetRequiredService<HandWStatVersionHandler>(),
            disposeHandler: false)
        {
            Timeout = TimeSpan.FromSeconds(30)
        });
        builder.Services.AddSingleton<IAppUpdateService, AppUpdateService>();
        builder.Services.AddSingleton<IApiAuthService, ApiAuthService>();
        builder.Services.AddSingleton<CompetitionsApiClient>();
        builder.Services.AddSingleton<TeamsApiClient>();
        builder.Services.AddSingleton<LookupsApiClient>();
        builder.Services.AddSingleton<PlayersApiClient>();
        builder.Services.AddSingleton<MatchesApiClient>();
        builder.Services.AddSingleton<MatchEventsApiClient>();
        builder.Services.AddSingleton<StatsApiClient>();
        builder.Services.AddSingleton<ReferenceDataService>();
        builder.Services.AddSingleton<TeamOfTheDayService>();
        builder.Services.AddSingleton<StatsDashboardService>();
        builder.Services.AddSingleton<AnalysisScopeService>();
        builder.Services.AddSingleton<GlobalSearchService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
