namespace HandWStat.Models.Analytics;

// ── Metric category ───────────────────────────────────────────────────────────

public enum AnalyticsMetricCategory
{
    Offensive,
    Defensive,
    Goalkeeper,
    Spatial,
    Contextual,
    Composite,
}

// ── Metric grain (observation level) ─────────────────────────────────────────

public enum AnalyticsMetricGrain
{
    Player,  // aggregated per player over a period
    Zone,    // per spatial zone (CAT-23/24/25)
    Match,   // per match event
    Team,    // team-level aggregation
}

// ── Primary source type ───────────────────────────────────────────────────────

public enum MetricPrimarySource
{
    Api,               // raw value from API field
    ComputedFromApi,   // client-side formula over API fields
    LocalCalculation,  // builder-level aggregation (e.g. MatchAnalyticsBuilder)
    Unknown,
}

// ── Metric dictionary entry — export projection ───────────────────────────────

public sealed record MetricDictionaryEntry(
    string Code,
    string Label,
    string TechnicalName,
    string Definition,
    string? Formula,
    AnalyticsMetricUnit Unit,
    AnalyticsMetricGrain Grain,
    AnalyticsMetricCategory Category,
    AnalyticsPositionScope ApplicablePositions,
    int MinimumSample,
    bool HigherIsBetter,
    AnalyticsMetricStatus Status,
    string? SourceField = null,
    string? SourceEndpoint = null,
    string? FallbackPolicy = null);

// ── Metric lineage definition — data traceability ────────────────────────────

public sealed record MetricLineageDefinition(
    string MetricCode,
    MetricPrimarySource PrimarySource,
    string? PrimaryField,
    string? Endpoint,
    string? CalculationSource,
    string? FallbackSource = null,
    string? Notes = null);
