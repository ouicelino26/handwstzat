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
                BaseUrl = ReadString(apiSection, nameof(ApiSettings.BaseUrl), ApiSettings.DefaultBaseUrl)
            };
        }
        catch
        {
            return new ApiSettings();
        }
    }

    public static UpdateSettings LoadUpdateSettings()
    {
        try
        {
            using var stream = FileSystem.OpenAppPackageFileAsync(FileName).GetAwaiter().GetResult();
            using var document = JsonDocument.Parse(stream);

            if (!document.RootElement.TryGetProperty("UpdateSettings", out var section))
            {
                return new UpdateSettings();
            }

            return new UpdateSettings
            {
                Enabled = ReadBoolean(section, nameof(UpdateSettings.Enabled), true),
                CheckOnStartup = ReadBoolean(section, nameof(UpdateSettings.CheckOnStartup), true),
                CheckOnResume = ReadBoolean(section, nameof(UpdateSettings.CheckOnResume), true),
                CheckIntervalHours = Math.Clamp(
                    ReadInteger(section, nameof(UpdateSettings.CheckIntervalHours), 12), 1, 168),
                Channel = ReadString(section, nameof(UpdateSettings.Channel), "STABLE").Trim().ToUpperInvariant(),
                AllowAutomaticDownload = ReadBoolean(
                    section, nameof(UpdateSettings.AllowAutomaticDownload), false)
            };
        }
        catch
        {
            return new UpdateSettings();
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

    private static bool ReadBoolean(JsonElement section, string propertyName, bool fallback) =>
        section.TryGetProperty(propertyName, out var value)
        && value.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? value.GetBoolean()
            : fallback;

    private static int ReadInteger(JsonElement section, string propertyName, int fallback) =>
        section.TryGetProperty(propertyName, out var value)
        && value.TryGetInt32(out var result)
            ? result
            : fallback;
}
