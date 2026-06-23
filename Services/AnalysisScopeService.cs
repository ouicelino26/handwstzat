namespace HandWStat.Services;

public sealed record AnalysisScopeSnapshot(
    int? CompetitionId,
    string? CompetitionName,
    int? TeamId,
    string? TeamName,
    string? Season,
    string? Day)
{
    public static AnalysisScopeSnapshot Empty { get; } = new(null, null, null, null, null, null);

    public bool HasValue =>
        CompetitionId.HasValue
        || TeamId.HasValue
        || !string.IsNullOrWhiteSpace(Season)
        || !string.IsNullOrWhiteSpace(Day);
}

public sealed class AnalysisScopeService
{
    public AnalysisScopeSnapshot Current { get; private set; } = AnalysisScopeSnapshot.Empty;

    public event Action? Changed;

    public void Update(AnalysisScopeSnapshot next)
    {
        if (Current == next)
        {
            return;
        }

        Current = next;
        Changed?.Invoke();
    }

    public void Reset()
    {
        Update(AnalysisScopeSnapshot.Empty);
    }
}
