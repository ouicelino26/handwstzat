namespace HandWStat.Services.Updates;

public sealed class HandWStatVersionHandler : DelegatingHandler
{
    private readonly IAppVersionProvider _versionProvider;
    private readonly HandWStat.Configuration.UpdateSettings _settings;

    public HandWStatVersionHandler(
        IAppVersionProvider versionProvider,
        HandWStat.Configuration.UpdateSettings settings)
        : this(versionProvider, settings, new HttpClientHandler())
    {
    }

    public HandWStatVersionHandler(IAppVersionProvider versionProvider, HttpMessageHandler innerHandler)
        : this(versionProvider, new HandWStat.Configuration.UpdateSettings(), innerHandler)
    {
    }

    public HandWStatVersionHandler(
        IAppVersionProvider versionProvider,
        HandWStat.Configuration.UpdateSettings settings,
        HttpMessageHandler innerHandler) : base(innerHandler)
    {
        _versionProvider = versionProvider;
        _settings = settings;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var version = _versionProvider.Current;
        SetHeader(request, "X-HandWStat-Version", version.Version);
        SetHeader(request, "X-HandWStat-Build", version.Build.ToString(System.Globalization.CultureInfo.InvariantCulture));
        SetHeader(request, "X-HandWStat-Platform", version.Platform);
        SetHeader(request, "X-HandWStat-Architecture", version.Architecture);
        SetHeader(request, "X-HandWStat-Channel", _settings.Channel);
        return base.SendAsync(request, cancellationToken);
    }

    private static void SetHeader(HttpRequestMessage request, string name, string value)
    {
        request.Headers.Remove(name);
        request.Headers.TryAddWithoutValidation(name, value);
    }
}
