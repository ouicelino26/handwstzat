using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;

namespace HandWStat.Services.Updates;

public sealed class DeviceIdentifierProvider : IDeviceIdentifierProvider
{
    private const string PreferenceKey = "handwstat.update-device-id";

    public string GetAnonymizedId()
    {
        var identifier = Preferences.Default.Get(PreferenceKey, string.Empty);
        if (string.IsNullOrWhiteSpace(identifier))
        {
            identifier = Guid.NewGuid().ToString("N");
            Preferences.Default.Set(PreferenceKey, identifier);
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identifier)));
    }
}
