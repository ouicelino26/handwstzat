using System.Text.RegularExpressions;
using HandballManagerCore.DTO;

namespace HandWStat.Models.Analytics;

public static partial class MatchFilterCatalog
{
    public static IReadOnlyList<string> GetSeasons(IEnumerable<MatchListItemDto> matches)
    {
        return matches
            .Select(match => Clean(match.Season))
            .Where(season => !string.IsNullOrWhiteSpace(season))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(season => season, StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToList();
    }

    public static IReadOnlyList<string> GetDays(IEnumerable<MatchListItemDto> matches, string? season = null)
    {
        var query = matches;

        if (!string.IsNullOrWhiteSpace(season))
        {
            query = query.Where(match => string.Equals(Clean(match.Season), Clean(season), StringComparison.OrdinalIgnoreCase));
        }

        return query
            .Select(match => Clean(match.Day))
            .Where(day => !string.IsNullOrWhiteSpace(day))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(day => GetDayOrder(day))
            .ThenBy(day => day, StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToList();
    }

    public static IReadOnlyList<MatchListItemDto> ApplySeasonAndDay(
        IEnumerable<MatchListItemDto> matches,
        string? season,
        string? day)
    {
        var query = matches;

        if (!string.IsNullOrWhiteSpace(season))
        {
            query = query.Where(match => string.Equals(Clean(match.Season), Clean(season), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(day))
        {
            query = query.Where(match => string.Equals(Clean(match.Day), Clean(day), StringComparison.OrdinalIgnoreCase));
        }

        return query.ToList();
    }

    public static string FormatSeasonDay(MatchListItemDto? match)
    {
        if (match is null)
        {
            return "Non renseigne";
        }

        var season = Clean(match.Season);
        var day = Clean(match.Day);

        if (string.IsNullOrWhiteSpace(season) && string.IsNullOrWhiteSpace(day))
        {
            return "Non renseigne";
        }

        if (string.IsNullOrWhiteSpace(season))
        {
            return day ?? "Non renseigne";
        }

        if (string.IsNullOrWhiteSpace(day))
        {
            return season;
        }

        return $"{season} - {day}";
    }

    public static string? NormalizeSelection(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? Clean(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int GetDayOrder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return int.MaxValue;
        }

        var digits = DayNumberRegex().Match(value);
        if (digits.Success && int.TryParse(digits.Value, out var number))
        {
            return number;
        }

        return int.MaxValue - 1;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex DayNumberRegex();
}
