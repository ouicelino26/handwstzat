namespace HandWStat.Services.Updates;

public sealed class HandWStatVersionHandler : DelegatingHandler
{
    private readonly IAppVersionProvider _versionProvider;

    public HandWStatVersionHandler(IAppVersionProvider versionProvider)
        : this(versionProvider, new HttpClientHandler())
    {
    }

    public HandWStatVersionHandler(IAppVersionProvider versionProvider, HttpMessageHandler innerHandler)
        : base(innerHandler)
    {
        _versionProvider = versionProvider;
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
        return base.SendAsync(request, cancellationToken);
    }

    private static void SetHeader(HttpRequestMessage request, string name, string value)
    {
        request.Headers.Remove(name);
        request.Headers.TryAddWithoutValidation(name, value);
    }
}
