namespace HandWStat.Services.Updates;

public interface IExternalLauncher
{
    Task<bool> OpenAsync(Uri uri);
}

