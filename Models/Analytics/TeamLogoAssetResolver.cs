using System.Globalization;
using System.Text;
using HandballManagerCore.DTO;

namespace HandWStat.Models.Analytics;

public static class TeamLogoAssetResolver
{
    public static string? ResolveByTeam(TeamDto? team, string? season)
    {
        return ResolveLogoUrl(team?.TeamLogo, season);
    }

    public static string? ResolveByName(IReadOnlyList<TeamDto> teams, string? teamName, string? season)
    {
        if (string.IsNullOrWhiteSpace(teamName) || teams.Count == 0)
        {
            return null;
        }

        var team = teams.FirstOrDefault(item => MatchesTeamLabel(item, teamName));
        return ResolveByTeam(team, season);
    }

    public static string? ResolveLogoUrl(string? logo, string? season)
    {
        if (string.IsNullOrWhiteSpace(logo))
        {
            return null;
        }

        var cleanValue = logo.Trim().Replace('\\', '/');
        if (cleanValue.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || cleanValue.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || cleanValue.StartsWith("/", StringComparison.Ordinal))
        {
            return cleanValue;
        }

        if (cleanValue.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
        {
            return "/" + cleanValue;
        }

        var fileName = cleanValue.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? cleanValue;
        var seasonFolder = string.Equals(season, "2024-2025", StringComparison.OrdinalIgnoreCase)
            ? "2024-2025"
            : "2025-2026";

        return $"/images/team-logos/lbe/{seasonFolder}/{Uri.EscapeDataString(fileName)}";
    }

    private static bool MatchesTeamLabel(TeamDto team, string label)
    {
        var normalizedLabel = Normalize(label);
        if (string.IsNullOrWhiteSpace(normalizedLabel))
        {
            return false;
        }

        return MatchesNormalized(team.TeamName, normalizedLabel)
            || MatchesNormalized(team.TeamCode, normalizedLabel);
    }

    private static bool MatchesNormalized(string? value, string normalizedLabel)
    {
        var normalizedValue = Normalize(value);
        if (string.IsNullOrWhiteSpace(normalizedValue))
        {
            return false;
        }

        if (string.Equals(normalizedValue, normalizedLabel, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return normalizedLabel.Length > 3
            && (normalizedValue.Contains(normalizedLabel, StringComparison.OrdinalIgnoreCase)
                || normalizedLabel.Contains(normalizedValue, StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        var previousWasSpace = false;

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
                previousWasSpace = false;
                continue;
            }

            if (!previousWasSpace)
            {
                builder.Append(' ');
                previousWasSpace = true;
            }
        }

        return builder.ToString().Trim();
    }
}
