namespace HandWStat.Services.Analytics;

// ──────────────────────────────────────────────────────────────────────────────
// B5.19 — Centralised backend quality tier mapping
// Maps backend string tier tokens to the frontend QualityTier enum.
// All callers must use this helper — no inline switch statements in Razor.
// ──────────────────────────────────────────────────────────────────────────────

public static class BackendQualityHelper
{
    /// <summary>
    /// Maps a backend tier string token ("High" | "Medium" | "Low" | null)
    /// to the frontend <see cref="QualityTier"/> enum.
    /// Unknown or null values map to NotApplicable.
    /// </summary>
    public static QualityTier MapBackendQualityTier(string? backendTier) => backendTier switch
    {
        "High"   => QualityTier.High,
        "Medium" => QualityTier.Medium,
        "Low"    => QualityTier.Low,
        _        => QualityTier.NotApplicable
    };

    /// <summary>
    /// Derives a QualityTier from a SampleReliable flag and a SampleCount.
    /// Count == 0 → NotApplicable.
    /// SampleReliable == true → High.
    /// SampleReliable == false → Low.
    /// </summary>
    public static QualityTier FromSampleReliable(bool sampleReliable, int sampleCount)
    {
        if (sampleCount == 0) return QualityTier.NotApplicable;
        return sampleReliable ? QualityTier.High : QualityTier.Low;
    }
}
