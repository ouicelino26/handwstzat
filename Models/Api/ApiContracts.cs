namespace HandWStat.Models.Api;

public sealed record LoginRequest(string Username, string Password);

public sealed class LoginResponse
{
    public string? accesstoken { get; set; }

    public string? username { get; set; }

    public string? role { get; set; }
}
