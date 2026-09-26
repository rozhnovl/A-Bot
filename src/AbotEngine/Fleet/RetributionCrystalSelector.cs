namespace AbotEngine.Fleet;

/// <summary>The range calculation behind one beam-crystal choice.</summary>
public readonly record struct RetributionCrystalDecision(
    LaserCrystal Crystal,
    int EngagementRangeMeters,
    int CurrentDistanceMeters,
    int? ExpectedRangeMeters,
    string Reason,
    double CurrentRangeHitChance = 1,
    bool CurrentRangeUseful = true);

/// <summary>
/// One carried charge in Gil/Fen's grouped Small Focused Beam Laser II fit. FalloffEndMeters is the
/// second distance shown by the in-space module tooltip (optimal + the 3 km accuracy-falloff width).
/// </summary>
public sealed record RetributionCrystalBallistics(
    LaserCrystal Crystal,
    int TypeId,
    string MenuLabel,
    int OptimalMeters,
    int FalloffEndMeters,
    double EmDamage,
    double ThermalDamage)
{
    public int FalloffMeters => Math.Max(1, FalloffEndMeters - OptimalMeters);
    public double RawDamage => EmDamage + ThermalDamage;

    /// <summary>Range-only hit chance; tracking/signature are independent additional penalties.</summary>
    public double RangeHitChance(int distanceMeters)
    {
        if (distanceMeters <= OptimalMeters) return 1;
        var falloffFractions = (distanceMeters - OptimalMeters) / (double)FalloffMeters;
        return Math.Pow(0.5, falloffFractions * falloffFractions);
    }
}

public static class RetributionCrystalCatalog
{
    // Ranges measured by the operator on this exact build. Faction-crystal damage is the SDE base
    // damage including the faction modifier; Aurora/Gleam are their T2 values.
    public static readonly IReadOnlyList<RetributionCrystalBallistics> Carried = new RetributionCrystalBallistics[]
    {
        new(LaserCrystal.Aurora, 12559, "Aurora S", 45_000, 48_000, 5, 3),
        new(LaserCrystal.Xray, 23075, "Imperial Navy Xray S", 19_000, 22_000, 6.9, 4.6),
        new(LaserCrystal.Standard, 23079, "Imperial Navy Standard S", 25_000, 28_000, 5.75, 3.45),
        new(LaserCrystal.Gamma, 23073, "Imperial Navy Gamma S", 15_000, 18_000, 8.05, 4.6),
        new(LaserCrystal.Gleam, 12557, "Gleam S", 6_000, 9_000, 7, 7),
        new(LaserCrystal.Multifrequency, 23071, "Imperial Navy Multifrequency S", 12_000, 15_000, 8.05, 5.75),
    };

    public static RetributionCrystalBallistics? For(LaserCrystal crystal) =>
        Carried.FirstOrDefault(c => c.Crystal == crystal);

    public static RetributionCrystalBallistics? FromTypeId(int? typeId) =>
        typeId is int id ? Carried.FirstOrDefault(c => c.TypeId == id) : null;
}

/// <summary>
/// Selects a crystal that can cover both the range now and the range at which the current
/// manoeuvre/enemy archetype is expected to settle. The maximum is intentional: loading a short
/// crystal early while burning toward a tight orbit would stop damage during the approach, while
/// a target expected to pull range should cause a pre-emptive long-range load.
/// </summary>
public static class RetributionCrystalSelector
{
    // A current charge within 15% of the best range-only score is retained to avoid menu churn.
    // Gleam is exempt: when the tight-orbit doctrine requests it and it actually wins, load it.
    private const double KeepCurrentScoreFraction = 0.85;

    public static RetributionCrystalDecision Decide(
        int currentDistanceMeters,
        int? expectedRangeMeters,
        LaserCrystal currentCrystal,
        bool preferGleam)
    {
        var current = Math.Max(0, currentDistanceMeters);
        var expected = expectedRangeMeters is int value ? Math.Max(0, value) : (int?)null;
        var engagement = Math.Max(current, expected ?? 0);

        var candidates = RetributionCrystalCatalog.Carried
            .Where(c => preferGleam || c.Crystal != LaserCrystal.Gleam)
            .Select(c => new
            {
                Profile = c,
                Score = c.RawDamage * c.RangeHitChance(engagement),
            })
            .OrderByDescending(c => c.Score)
            .ThenByDescending(c => c.Profile.OptimalMeters)
            .ToArray();
        var best = candidates[0];
        var selected = best.Profile;
        var loaded = candidates.FirstOrDefault(c => c.Profile.Crystal == currentCrystal);
        if (best.Profile.Crystal != LaserCrystal.Gleam && loaded is not null &&
            loaded.Score >= best.Score * KeepCurrentScoreFraction)
            selected = loaded.Profile;

        var expectedText = expected is int expectedMeters
            ? $"current={current}m, expected={expectedMeters}m"
            : $"current={current}m, expected=unknown";
        return new RetributionCrystalDecision(
            selected.Crystal,
            engagement,
            current,
            expected,
            $"coverage=max(current, expected)={engagement}m ({expectedText}); " +
            $"optimal={selected.OptimalMeters}m, falloff-end={selected.FalloffEndMeters}m, " +
            $"range-hit={selected.RangeHitChance(engagement):P0}",
            selected.RangeHitChance(current),
            current <= selected.FalloffEndMeters);
    }
}
