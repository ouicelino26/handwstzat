using HandWStat.Models.Analytics;

namespace HandWStat.Tests;

public sealed class RateDisplayModelTests
{
    [Fact]
    public void FromV1_WithVolume_ExposesEvidenceAndReliability()
    {
        var model = RateDisplayModel.FromV1(
            "SHOT_RATE",
            "Taux de tir",
            50,
            "%",
            "Buts sur tentatives.",
            numerator: 5,
            denominator: 10,
            minimumSample: 4,
            tone: "good");

        Assert.True(model.HasVolume);
        Assert.True(model.SampleReliable);
        Assert.Equal("5 / 10", model.VolumeLabel);
        Assert.Equal("50", model.ValueLabel);
    }

    [Fact]
    public void FromV1_WithoutVolume_DoesNotInventEvidence()
    {
        var model = RateDisplayModel.FromV1(
            "LEGACY_RATE",
            "Taux v1",
            42,
            "%",
            "Valeur historique.");

        Assert.False(model.HasVolume);
        Assert.False(model.SampleReliable);
        Assert.Equal("Volume non fourni par l'API", model.VolumeLabel);
        Assert.Equal("Qualite non renseignee", model.QualityLabel);
    }

    [Fact]
    public void FromV1_WithZeroDenominator_FormatsNA()
    {
        var model = RateDisplayModel.FromV1(
            "EMPTY_RATE",
            "Taux vide",
            0,
            "%",
            "Aucun volume.",
            numerator: 0,
            denominator: 0,
            minimumSample: 1);

        Assert.Null(model.Value);
        Assert.Equal("N/A", model.ValueLabel);
        Assert.Equal("Indicateur non calculable", model.ReliabilityLabel);
    }

    [Fact]
    public void FromV1_WithNonFiniteValue_FormatsNA()
    {
        var model = RateDisplayModel.FromV1(
            "INVALID_RATE",
            "Taux invalide",
            double.PositiveInfinity,
            "%",
            "Valeur invalide.",
            numerator: 1,
            denominator: 2,
            minimumSample: 1,
            tone: "good");

        Assert.Null(model.Value);
        Assert.Equal("N/A", model.ValueLabel);
        Assert.Equal("Non calculable", model.QualityLabel);
        Assert.Equal("neutral", model.Tone);
    }

    [Fact]
    public void FromV1_WithoutMinimumSample_DoesNotClaimLimitedQuality()
    {
        var model = RateDisplayModel.FromV1(
            "RATE_WITHOUT_THRESHOLD",
            "Taux sans seuil",
            50,
            "%",
            "Seuil absent.",
            numerator: 5,
            denominator: 10);

        Assert.False(model.SampleReliable);
        Assert.Equal("Qualite non renseignee", model.QualityLabel);
        Assert.Equal("Fiabilite non renseignee", model.ReliabilityLabel);
    }
}
