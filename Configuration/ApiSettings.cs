namespace HandWStat.Configuration;

public sealed class ApiSettings
{
    public const string DefaultBaseUrl = "https://handballwstat.ddnsfree.com/api/";

    public string BaseUrl { get; init; } = DefaultBaseUrl;

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;
}
