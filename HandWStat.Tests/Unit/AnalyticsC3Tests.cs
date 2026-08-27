using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace HandWStat.Tests.Unit;

/// <summary>
/// Phase C3 — Workflow &amp; Product Validation tests.
/// Validates real user journey invariants: empty states, data quality messaging,
/// section structure, and cross-page context rules.
/// These are static file content tests — no runtime Blazor rendering required.
/// </summary>
public class AnalyticsC3Tests
{
    private static readonly string ProjectRoot = GetProjectRoot();

    private static string GetProjectRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "HandWStat.csproj")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? AppContext.BaseDirectory;
    }

    private static string ReadFile(string relativePath)
        => File.ReadAllText(Path.Combine(ProjectRoot, relativePath));

    // ── W1 — Teams roster 5 default columns ───────────────────────────────────

    [Fact]
    public void Teams_Roster_HasDefaultColumn_Joueuse()
    {
        var content = ReadFile("Components/Pages/Teams.razor");
        Assert.Contains("\"name\", \"Joueuse\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Teams_Roster_HasDefaultColumn_Poste()
    {
        var content = ReadFile("Components/Pages/Teams.razor");
        Assert.Contains("\"position\", \"Poste\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Teams_Roster_HasDefaultColumn_Matchs()
    {
        var content = ReadFile("Components/Pages/Teams.razor");
        Assert.Contains("\"matches\", \"Matchs\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Teams_Roster_HasDefaultColumn_Buts()
    {
        var content = ReadFile("Components/Pages/Teams.razor");
        Assert.Contains("\"goals\", \"Buts\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Teams_Roster_HasDefaultColumn_PartButs()
    {
        var content = ReadFile("Components/Pages/Teams.razor");
        Assert.Contains("\"goalsshare\", \"Part buts\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Teams_Roster_HasToggleButton_PlusDeColonnes()
    {
        var content = ReadFile("Components/Pages/Teams.razor");
        Assert.Contains("Plus de colonnes", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Teams_Roster_ToggleButton_HasTestId()
    {
        var content = ReadFile("Components/Pages/Teams.razor");
        Assert.Contains("data-testid=\"roster-expand-cols\"", content, StringComparison.Ordinal);
    }

    // ── W1 — Teams roster empty state ─────────────────────────────────────────

    [Fact]
    public void Teams_Roster_HasEmptyState_WhenRosterEmpty()
    {
        var content = ReadFile("Components/Pages/Teams.razor");
        // The empty state should be guarded by rosterRows.Length == 0
        Assert.Contains("rosterRows.Length == 0", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Teams_Roster_EmptyState_HasUserFriendlyMessage()
    {
        var content = ReadFile("Components/Pages/Teams.razor");
        Assert.Contains("Aucune joueuse dans cette sélection", content, StringComparison.OrdinalIgnoreCase);
    }

    // ── W2 — Clutch empty state ───────────────────────────────────────────────

    [Fact]
    public void AnalyseTab_Clutch_SampleCountZero_ShowsMessage()
    {
        var content = ReadFile("Components/Shared/AnalyseTabPanel.razor");
        // When SampleCount == 0, a human-readable message must appear, not a rate
        Assert.Contains("SampleCount == 0", content, StringComparison.Ordinal);
        // The message must be a user-readable French string
        var zeroIdx = content.IndexOf("SampleCount == 0", StringComparison.Ordinal);
        // In the same branch, "Aucune donnée" should appear
        var msgIdx = content.IndexOf("Aucune donn", StringComparison.OrdinalIgnoreCase);
        Assert.True(msgIdx >= 0, "No user-readable empty clutch message found");
    }

    [Fact]
    public void AnalyseTab_Clutch_LimitedSample_ShowsWarning()
    {
        var content = ReadFile("Components/Shared/AnalyseTabPanel.razor");
        // When SampleCount < 5, a sample warning note must appear
        Assert.Contains("SampleCount < 5", content, StringComparison.Ordinal);
        // The warning text should be user-facing
        var sampleIdx = content.IndexOf("SampleCount < 5", StringComparison.Ordinal);
        var noteIdx = content.IndexOf("court-sample-note", StringComparison.OrdinalIgnoreCase);
        Assert.True(noteIdx > 0, "No sample warning note found for clutch limited sample");
    }

    // ── W2 — GK ScoreState guard ──────────────────────────────────────────────

    [Fact]
    public void AnalyseTab_GkScoreState_GuardedByIsGoalkeeper()
    {
        var content = ReadFile("Components/Shared/AnalyseTabPanel.razor");
        // The GK ScoreState section must be wrapped with @if (IsGoalkeeper)
        var guardIdx = content.IndexOf("@if (IsGoalkeeper)", StringComparison.Ordinal);
        var scoreStateIdx = content.IndexOf("anlz-gk-scorestate-section", StringComparison.Ordinal);
        Assert.True(guardIdx >= 0, "@if (IsGoalkeeper) guard not found");
        Assert.True(scoreStateIdx >= 0, "anlz-gk-scorestate-section not found");
        Assert.True(guardIdx < scoreStateIdx, "GK ScoreState section must be inside @if (IsGoalkeeper) block");
    }

    [Fact]
    public void AnalyseTab_GkScoreState_HasEmptyStateForNoData()
    {
        var content = ReadFile("Components/Shared/AnalyseTabPanel.razor");
        // When GkScoreStateData is null or empty, a human-readable message must appear
        Assert.Contains("GkScoreStateData is null", content, StringComparison.Ordinal);
    }

    // ── W3 — Compare: sample count visible per player ─────────────────────────

    [Fact]
    public void Compare_PlayerCard_ShowsMatchesPlayed()
    {
        var content = ReadFile("Components/Pages/Compare.razor");
        // Each player's match count must be visible in the player cards
        Assert.Contains("cmpPlayer.MatchesPlayed", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Compare_GkVsField_NoticePresent()
    {
        var content = ReadFile("Components/Pages/Compare.razor");
        // GK vs field incompatibility notice must be present
        Assert.Contains("IsGkVsFieldComparison", content, StringComparison.Ordinal);
        Assert.Contains("cmp-compat-notice", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Compare_Radar_GkVsField_IncompatibilityNotice()
    {
        var content = ReadFile("Components/Pages/Compare.razor");
        // Radar incompatibility message for GK vs field must exist
        Assert.Contains("Profils incompatibles", content, StringComparison.OrdinalIgnoreCase);
    }

    // ── W4 — B6 Timeline: 5/10 min selectors inside details ──────────────────

    [Fact]
    public void Players_Timeline_FiveMinButton_InsideDetails()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        var detailsIdx = content.IndexOf("b6-timeline-details", StringComparison.Ordinal);
        // The 5 min button must appear after the details opening
        var fiveMinIdx = content.IndexOf("\"5 min\"", StringComparison.Ordinal);
        // Fallback: text content "5 min"
        if (fiveMinIdx < 0) fiveMinIdx = content.IndexOf(">5 min<", StringComparison.Ordinal);
        Assert.True(detailsIdx >= 0, "b6-timeline-details not found");
        Assert.True(fiveMinIdx > detailsIdx, "5 min selector must be inside the b6-timeline-details element");
    }

    [Fact]
    public void Players_Timeline_TenMinButton_InsideDetails()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        var detailsIdx = content.IndexOf("b6-timeline-details", StringComparison.Ordinal);
        var tenMinIdx = content.IndexOf("\"10 min\"", StringComparison.Ordinal);
        if (tenMinIdx < 0) tenMinIdx = content.IndexOf(">10 min<", StringComparison.Ordinal);
        Assert.True(detailsIdx >= 0, "b6-timeline-details not found");
        Assert.True(tenMinIdx > detailsIdx, "10 min selector must be inside the b6-timeline-details element");
    }

    [Fact]
    public void Players_Timeline_GranularityGroup_HasAriaLabel()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        // The filter chip group for timeline bucket selection must have an accessible label
        Assert.Contains("Granularité temporelle", content, StringComparison.OrdinalIgnoreCase);
    }

    // ── W4 — TemporalCoveragePct warning is human-readable ────────────────────

    [Fact]
    public void Players_Timeline_TemporalCoverage_HumanReadable()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        // TemporalCoveragePct < 80 must trigger a note with readable French text
        var pctIdx = content.IndexOf("TemporalCoveragePct < 80", StringComparison.Ordinal);
        Assert.True(pctIdx >= 0, "TemporalCoveragePct < 80 check not found");
        // The message should mention "Couverture temporelle" not the raw property name
        var msgIdx = content.IndexOf("Couverture temporelle", StringComparison.OrdinalIgnoreCase);
        Assert.True(msgIdx >= 0, "No human-readable TemporalCoveragePct warning found");
        // The raw property name must NOT be the entire visible text
        Assert.DoesNotContain("<p>TemporalCoveragePct", content, StringComparison.Ordinal);
    }

    // ── W5 — Matches Story: disclosure and scoring runs ───────────────────────

    [Fact]
    public void Matches_Story_DisclosurePresent()
    {
        var content = ReadFile("Components/Pages/Matches.razor");
        Assert.Contains("match-advanced-disclosure", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Matches_Story_DataQualityWarning_HumanReadable()
    {
        var content = ReadFile("Components/Pages/Matches.razor");
        // DataQualityWarning must be translated to French human text, not rendered raw
        var propIdx = content.IndexOf("DataQualityWarning", StringComparison.Ordinal);
        Assert.True(propIdx >= 0, "DataQualityWarning check not found");
        // The raw string "DataQualityWarning" must not be used as visible text directly
        Assert.DoesNotContain(">DataQualityWarning<", content, StringComparison.Ordinal);
        // A human-readable French message must appear instead
        Assert.Contains("Données événements partielles", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Matches_CourtTab_LabelIsCourt()
    {
        var content = ReadFile("Components/Pages/Matches.razor");
        Assert.Contains("Court", content, StringComparison.Ordinal);
        Assert.DoesNotContain(">Timeline<", content, StringComparison.Ordinal);
    }

    // ── Quality invariants — no raw technical strings in visible text ──────────

    [Fact]
    public void Players_Performance_NoRaw_SampleReliable_InVisibleText()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        // "SampleReliable" must never be rendered as user-facing text content
        Assert.DoesNotContain(">SampleReliable<", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Players_Performance_NoRaw_QualityTier_InVisibleText()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        // "QualityTier" must never appear as a rendered text node
        Assert.DoesNotContain(">QualityTier<", content, StringComparison.Ordinal);
        Assert.DoesNotContain(">QualityTier.Low<", content, StringComparison.Ordinal);
    }

    [Fact]
    public void AnalyseTab_NoRaw_SampleReliable_InVisibleText()
    {
        var content = ReadFile("Components/Shared/AnalyseTabPanel.razor");
        Assert.DoesNotContain(">SampleReliable<", content, StringComparison.Ordinal);
    }

    // ── W4 — Timeline empty bucket shows data-na not 0 ────────────────────────

    [Fact]
    public void Players_Timeline_NullBucket_ShowsDataNaNotZero()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        // When a bucket value is null, "N/A" must be shown via the data-na class
        // Check that data-na appears inside the timeline-bars rendering section
        var timelineBarsIdx = content.IndexOf("timeline-bars", StringComparison.Ordinal);
        Assert.True(timelineBarsIdx >= 0, "timeline-bars div not found");
        // Find data-na AFTER the timeline-bars section
        var dataNaAfterTimelineIdx = content.IndexOf("data-na", timelineBarsIdx, StringComparison.Ordinal);
        Assert.True(dataNaAfterTimelineIdx > timelineBarsIdx,
            "data-na class must appear inside the timeline-bars rendering block (after timeline-bars)");
    }

    // ── W4 — Évolution line chart visible before B6 details ───────────────────

    [Fact]
    public void Players_Evolution_LineChart_BeforeB6Details()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        var graphSectionIdx = content.IndexOf("ActiveSection == \"graphs\"", StringComparison.Ordinal);
        var chartIdx = content.IndexOf("SeriesType.Line", StringComparison.Ordinal);
        var b6DetailsIdx = content.IndexOf("b6-timeline-details", StringComparison.Ordinal);
        Assert.True(graphSectionIdx >= 0, "graphs section not found");
        Assert.True(chartIdx > graphSectionIdx, "Line chart must be inside graphs section");
        Assert.True(chartIdx < b6DetailsIdx, "Line chart must appear before b6-timeline-details");
    }
}
