using System.Globalization;
using System.Text;
using HandballManagerCore.DTO;

namespace HandWStat.Services.Api;

public sealed class ApiQueryBuilder
{
    private readonly List<KeyValuePair<string, string>> _pairs = [];

    public ApiQueryBuilder Add(string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            _pairs.Add(new KeyValuePair<string, string>(name, value.Trim()));
        }

        return this;
    }

    public ApiQueryBuilder Add(string name, int? value)
    {
        if (value.HasValue)
        {
            _pairs.Add(new KeyValuePair<string, string>(name, value.Value.ToString(CultureInfo.InvariantCulture)));
        }

        return this;
    }

    public ApiQueryBuilder Add(string name, DateTime? value)
    {
        if (value.HasValue)
        {
            _pairs.Add(new KeyValuePair<string, string>(name, value.Value.ToString("O", CultureInfo.InvariantCulture)));
        }

        return this;
    }

    public ApiQueryBuilder AddCsv(string name, IEnumerable<int>? values)
    {
        if (values is null)
        {
            return this;
        }

        var normalized = values
            .Where(value => value > 0)
            .Distinct()
            .ToArray();

        if (normalized.Length > 0)
        {
            _pairs.Add(new KeyValuePair<string, string>(name, string.Join(",", normalized)));
        }

        return this;
    }

    public string BuildRelativePath(string relativePath)
    {
        if (_pairs.Count == 0)
        {
            return relativePath;
        }

        var builder = new StringBuilder(relativePath);
        builder.Append(relativePath.Contains('?') ? '&' : '?');

        for (var index = 0; index < _pairs.Count; index++)
        {
            var pair = _pairs[index];

            if (index > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(pair.Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(pair.Value));
        }

        return builder.ToString();
    }

    public static ApiQueryBuilder FromStatsOptions(StatsQueryOptionsDto? options)
    {
        var builder = new ApiQueryBuilder();

        if (options is null)
        {
            return builder;
        }

        return builder
            .Add("competitionId", options.CompetitionId)
            .Add("teamId", options.TeamId)
            .Add("playerId", options.PlayerId)
            .AddCsv("playerIds", options.PlayerIds)
            .Add("positionId", options.PositionId)
            .Add("matchId", options.MatchId)
            .Add("from", options.From)
            .Add("to", options.To)
            .Add("year", options.Year)
            .Add("season", options.Season)
            .Add("day", options.Day)
            .Add("attackId", options.AttackId)
            .Add("defenseId", options.DefenseId)
            .Add("trigger", options.Trigger)
            .Add("shootShade", options.ShootShade);
    }
}
