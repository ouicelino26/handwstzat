namespace HandWStat.Configuration;

public sealed class ApiSettings
{
    public const string DefaultBaseUrl = "https://handballwstat.ddnsfree.com/api/";

    public string BaseUrl { get; init; } = DefaultBaseUrl;

}
