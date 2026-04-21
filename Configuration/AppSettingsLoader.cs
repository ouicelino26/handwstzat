using System.Text.Json;

namespace HandWStat.Configuration;

public static class AppSettingsLoader
{
    private const string FileName = "appsettings.json";

    public static ApiSettings LoadApiSettings()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync(FileName).GetAwaiter().GetResult();
            using var document = JsonDocument.Parse(stream);

            if (!document.RootElement.TryGetProperty("ApiSettings", out var apiSection))
            {
                return new ApiSettings();
            }

            return new ApiSettings
            {
                BaseUrl = ReadString(apiSection, nameof(ApiSettings.BaseUrl), ApiSettings.DefaultBaseUrl),
                ClientId = ReadString(apiSection, nameof(ApiSettings.ClientId), string.Empty),
                ClientSecret = ReadString(apiSection, nameof(ApiSettings.ClientSecret), string.Empty)
            };
        }
        catch
        {
            return new ApiSettings();
        }
    }

    private static string ReadString(JsonElement section, string propertyName, string fallback)
    {
        return section.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(value.GetString())
                ? value.GetString()!
                : fallback;
    }
}
