namespace HandWStat.Models.Updates;

public sealed record AppVersionInfo(
    string Version,
    int Build,
    string Platform,
    string Architecture);

