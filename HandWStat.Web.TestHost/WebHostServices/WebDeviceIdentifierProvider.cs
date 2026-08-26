using HandWStat.Services.Updates;

namespace HandWStat.Web.TestHost.WebHostServices;

public sealed class WebDeviceIdentifierProvider : IDeviceIdentifierProvider
{
    private static readonly string _id = Guid.NewGuid().ToString("N")[..12];

    public string GetAnonymizedId() => _id;
}
