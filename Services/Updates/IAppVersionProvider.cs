using HandWStat.Models.Updates;

namespace HandWStat.Services.Updates;

public interface IAppVersionProvider
{
    AppVersionInfo Current { get; }
}

