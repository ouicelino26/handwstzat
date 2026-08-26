using System.Runtime.InteropServices;
using HandWStat.Models.Updates;
using HandWStat.Services.Updates;

namespace HandWStat.Web.TestHost.WebHostServices;

public sealed class WebAppVersionProvider : IAppVersionProvider
{
    public AppVersionInfo Current { get; } = new AppVersionInfo(
        Version: "1.0.0",
        Build: 0,
        Platform: "Web",
        Architecture: RuntimeInformation.ProcessArchitecture.ToString());
}
