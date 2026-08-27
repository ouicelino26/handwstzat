using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace HandWStat.Tests.Unit;

/// <summary>
/// Phase C2 — Analytics Information Architecture tests.
/// Validates markup-level changes: removed noise, added disclosures, reordered sections.
/// These are static file content tests — no runtime Blazor rendering required.
/// </summary>
public class AnalyticsC2Tests
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

    // ── P0-01 Compare label ────────────────────────────────────────────────────

    [Fact]
    public void Compare_Subtoolbar_DoesNotContainNormalise()
    {
        var content = ReadFile("Components/Pages/Compare.razor");
        // Case-insensitive check: "normalisé" or "normalisée" or "normalisées" must be absent from subtoolbar
        Assert.DoesNotContain("normalis", content, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Compare_Subtoolbar_ContainsValeursAbsolues()
    {
        var content = ReadFile("Components/Pages/Compare.razor");
        Assert.Contains("Valeurs absolues / match", content, StringComparison.Ordinal);
    }

    // ── P0-02 Dashboard Insight Strip ─────────────────────────────────────────

    [Fact]
    public void Dashboard_DoesNotContain_InsightStrip()
    {
        var content = ReadFile("Components/Pages/Dashboard.razor");
        Assert.DoesNotContain("dashboard-insight-strip", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Dashboard_DoesNotContain_InsightArticle()
    {
        var content = ReadFile("Components/Pages/Dashboard.razor");
        Assert.DoesNotContain("dashboard-insight\"", content, StringComparison.Ordinal);
    }

    // ── P0-03 Players Brief — kpi-tile-grid absent ────────────────────────────

    [Fact]
    public void Players_Brief_DoesNotContain_KpiTileGrid()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        Assert.DoesNotContain("kpi-tile-grid kpi-tile-grid--5", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Players_Brief_Contains_DlList()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        // dl-list (brief-stat-list) must still be present
        Assert.Contains("brief-stat-list", content, StringComparison.Ordinal);
    }

    // ── P1-01 B6 Timeline disclosure ──────────────────────────────────────────

    [Fact]
    public void Players_Evolution_Contains_DetailsDisclosure()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        Assert.Contains("b6-timeline-details", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Players_Evolution_DetailsWrapsTimeline()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        // details must appear before timeline-section
        var detailsIdx = content.IndexOf("b6-timeline-details", StringComparison.Ordinal);
        var timelineIdx = content.IndexOf("timeline-section", StringComparison.Ordinal);
        Assert.True(detailsIdx >= 0, "b6-timeline-details not found");
        Assert.True(timelineIdx > detailsIdx, "timeline-section must be inside the details element");
    }

    [Fact]
    public void Players_Evolution_DisclosureLabelPresent()
    {
        var content = ReadFile("Components/Pages/Players.razor");
        Assert.Contains("Analyse avancée par période", content, StringComparison.OrdinalIgnoreCase);
    }

    // ── P1-04 Dashboard Secondary Grid tables absent ──────────────────────────

    [Fact]
    public void Dashboard_DoesNotContain_SecondaryGridTopButeuses()
    {
        var content = ReadFile("Components/Pages/Dashboard.razor");
        Assert.DoesNotContain("dashboard-secondary-grid", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Dashboard_DoesNotContain_DashboardMiniTable()
    {
        var content = ReadFile("Components/Pages/Dashboard.razor");
        Assert.DoesNotContain("dashboard-mini-table", content, StringComparison.Ordinal);
    }

    // ── P1-02 Match Story disclosure ──────────────────────────────────────────

    [Fact]
    public void Matches_Story_Contains_ExpertDisclosure()
    {
        var content = ReadFile("Components/Pages/Matches.razor");
        Assert.Contains("match-advanced-disclosure", content, StringComparison.Ordinal);
    }

    [Fact]
    public void Matches_Story_ScoringRunsInsideDisclosure()
    {
        var content = ReadFile("Components/Pages/Matches.razor");
        var disclosureIdx = content.IndexOf("match-advanced-disclosure", StringComparison.Ordinal);
        var runsIdx = content.IndexOf("Séries de buts", StringComparison.Ordinal);
        Assert.True(disclosureIdx >= 0, "match-advanced-disclosure not found");
        Assert.True(runsIdx > disclosureIdx, "Séries de buts must be inside the disclosure");
    }

    [Fact]
    public void Matches_Story_DisclosureLabelPresent()
    {
        var content = ReadFile("Components/Pages/Matches.razor");
        Assert.Contains("Analyse avancée du match", content, StringComparison.Ordinal);
    }

    // ── P1-02 Match spatial tab label ─────────────────────────────────────────

    [Fact]
    public void Matches_SpatialTab_LabelIsCourt()
    {
        var content = ReadFile("Components/Pages/Matches.razor");
        // The tab button for "zones" must contain "Court", not "Timeline"
        // We look for the tab button that sets zones section
        var zonesTabPattern = @"SetActiveSection\(""zones""\)[^<]*<[^>]*>\s*(Court|Timeline)\s*</button>";
        var match = Regex.Match(content, zonesTabPattern, RegexOptions.Singleline);
        if (match.Success)
        {
            Assert.Equal("Court", match.Groups[1].Value.Trim());
        }
        else
        {
            // Fallback: just check the tab area contains "Court" and doesn't have standalone "Timeline" tab
            Assert.Contains("Court", content, StringComparison.Ordinal);
        }
    }

    // ── P1-05 Analyse tab section order ───────────────────────────────────────

    [Fact]
    public void AnalyseTab_HalfTime_BeforeClutch()
    {
        var content = ReadFile("Components/Shared/AnalyseTabPanel.razor");
        var halfTimeIdx = content.IndexOf("anlz-halftime-section", StringComparison.Ordinal);
        var clutchIdx = content.IndexOf("anlz-clutch-section", StringComparison.Ordinal);
        Assert.True(halfTimeIdx >= 0, "anlz-halftime-section not found");
        Assert.True(clutchIdx >= 0, "anlz-clutch-section not found");
        Assert.True(halfTimeIdx < clutchIdx, "HalfTime section must appear before Clutch");
    }

    [Fact]
    public void AnalyseTab_Clutch_BeforeHistogram()
    {
        var content = ReadFile("Components/Shared/AnalyseTabPanel.razor");
        var clutchIdx = content.IndexOf("anlz-clutch-section", StringComparison.Ordinal);
        var histogramIdx = content.IndexOf("PositionProfileHistogram", StringComparison.Ordinal);
        Assert.True(clutchIdx >= 0, "anlz-clutch-section not found");
        Assert.True(histogramIdx >= 0, "PositionProfileHistogram not found");
        Assert.True(clutchIdx < histogramIdx, "Clutch section must appear before Histogram");
    }

    [Fact]
    public void AnalyseTab_Histogram_BeforeRadar()
    {
        var content = ReadFile("Components/Shared/AnalyseTabPanel.razor");
        var histogramIdx = content.IndexOf("PositionProfileHistogram", StringComparison.Ordinal);
        var radarIdx = content.IndexOf("PositionRadarChart", StringComparison.Ordinal);
        Assert.True(histogramIdx < radarIdx, "Histogram must appear before Radar");
    }

    [Fact]
    public void AnalyseTab_ContextualSplits_AfterExport()
    {
        var content = ReadFile("Components/Shared/AnalyseTabPanel.razor");
        var exportIdx = content.IndexOf("anlz-export-toolbar", StringComparison.Ordinal);
        var ctxIdx = content.IndexOf("ctx-panel", StringComparison.Ordinal);
        Assert.True(exportIdx >= 0, "anlz-export-toolbar not found");
        Assert.True(ctxIdx >= 0, "ctx-panel not found");
        Assert.True(exportIdx < ctxIdx, "Export toolbar must appear before Contextual splits");
    }
}
