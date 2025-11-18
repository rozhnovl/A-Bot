using Sanderling.ABot.Bot;
using Sanderling.ABot.Bot.Strategies;
using Sanderling.ABot.Bot.Task;

namespace AbyssalBot.Domain.Tests;

public class PriorityManagerTests
{
    private readonly NpcInfoProvider _npcInfoProvider;
    private readonly ShipFit _shipFit;
    private readonly PriorityManager _sut;

    public PriorityManagerTests()
    {
        _npcInfoProvider = new NpcInfoProvider();
        _shipFit = Substitute.For<ShipFit>();
        _shipFit.MaxTargetingRange.Returns(50000);
        _sut = new PriorityManager(_shipFit, _npcInfoProvider);
    }

    #region GetEnemies Tests

    [Fact]
    public void Given_NoEnemies_When_GettingEnemies_Then_ReturnsEmptyArray()
    {
        // Arrange
        var overview = CreateOverviewProvider(new List<IOverviewEntry>());

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().BeEmpty("There are no enemies in the overview");
    }

    [Fact]
    public void Given_OnlyFriendlies_When_GettingEnemies_Then_ReturnsEmptyArray()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Friendly Ship 1", distance: 10000, isEnemy: false),
            CreateOverviewEntry("Friendly Ship 2", distance: 20000, isEnemy: false)
        };
        var overview = CreateOverviewProvider(entries);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().BeEmpty("All ships are friendly");
    }

    [Fact]
    public void Given_EnemiesWithinRange_When_GettingEnemies_Then_ReturnsEnemies()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", distance: 10000, isEnemy: true),
            CreateOverviewEntry("Striking Damavik", "Striking Damavik", distance: 20000, isEnemy: true)
        };
        var overview = CreateOverviewProvider(entries);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().HaveCount(2, "Both enemies are within targeting range");
    }

    [Fact]
    public void Given_EnemiesBeyondRange_When_GettingEnemies_Then_FiltersOutDistantEnemies()
    {
        // Arrange
        _shipFit.MaxTargetingRange.Returns(30000);
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", distance: 10000, isEnemy: true),
            CreateOverviewEntry("Striking Damavik", "Striking Damavik", distance: 40000, isEnemy: true),
            CreateOverviewEntry("Starving Damavik", "Starving Damavik", distance: 25000, isEnemy: true)
        };
        var overview = CreateOverviewProvider(entries);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().HaveCount(2, "Only enemies within 30000m should be included");
        result.Should().Contain(e => e.Name == "Ghosting Damavik");
        result.Should().Contain(e => e.Name == "Starving Damavik");
        result.Should().NotContain(e => e.Name == "Striking Damavik");
    }

    [Fact]
    public void Given_ExtractionNodes_When_GettingEnemies_Then_FiltersThemOut()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", distance: 10000, isEnemy: true),
            CreateOverviewEntry("Triglavian Extraction Node", "Triglavian Extraction Node", distance: 15000, isEnemy: true),
            CreateOverviewEntry("Triglavian Extraction SubNode", "Triglavian Extraction SubNode", distance: 18000, isEnemy: true)
        };
        var overview = CreateOverviewProvider(entries);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().HaveCount(1, "Extraction nodes should be filtered out");
        result[0].Name.Should().Be("Ghosting Damavik");
    }

    [Fact]
    public void Given_VilaSwarmer_When_GettingEnemies_Then_FiltersThemOut()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", distance: 10000, isEnemy: true),
            CreateOverviewEntry("Vila Swarmer 1", "Vila Swarmer", distance: 5000, isEnemy: true),
            CreateOverviewEntry("Vila Swarmer 2", "Vila Swarmer", distance: 8000, isEnemy: true)
        };
        var overview = CreateOverviewProvider(entries);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().HaveCount(1, "Vila Swarmers should be filtered out");
        result[0].Name.Should().Be("Ghosting Damavik");
    }

    [Fact]
    public void Given_MixedEnemies_When_GettingEnemies_Then_SortsByPriorityThenDistance()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Lucid Escort", "Lucid Escort", distance: 5000, isEnemy: true),
            CreateOverviewEntry("Anchoring Damavik", "Anchoring Damavik", distance: 15000, isEnemy: true),
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", distance: 10000, isEnemy: true),
            CreateOverviewEntry("Lucid Firewatcher", "Lucid Firewatcher", distance: 8000, isEnemy: true)
        };
        var overview = CreateOverviewProvider(entries);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().HaveCount(4);
        // Priority order: Anchoring (1), Firewatcher (2), then by DPS/distance
        result[0].Name.Should().Be("Anchoring Damavik", "Highest priority");
        result[1].Name.Should().Be("Lucid Firewatcher", "Second highest priority");
    }

    [Fact]
    public void Given_SamePriorityEnemies_When_GettingEnemies_Then_SortsByDistance()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", distance: 20000, isEnemy: true),
            CreateOverviewEntry("Tangling Damavik", "Tangling Damavik", distance: 10000, isEnemy: true),
            CreateOverviewEntry("Starving Damavik", "Starving Damavik", distance: 15000, isEnemy: true)
        };
        var overview = CreateOverviewProvider(entries);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().HaveCount(3);
        // All have same DPS, should be sorted by distance
        result[0].Distance.Should().BeLessThan(result[1].Distance);
        result[1].Distance.Should().BeLessThan(result[2].Distance);
    }

    [Fact]
    public void Given_HighPriorityTargetsAtVariousDistances_When_GettingEnemies_Then_PrioritizesCorrectly()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Renewing Leshak", "Renewing Leshak", distance: 25000, isEnemy: true), // Priority 3
            CreateOverviewEntry("Anchoring Damavik", "Anchoring Damavik", distance: 30000, isEnemy: true), // Priority 1
            CreateOverviewEntry("Lucid Firewatcher", "Lucid Firewatcher", distance: 5000, isEnemy: true), // Priority 2
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", distance: 8000, isEnemy: true) // Priority based on DPS
        };
        var overview = CreateOverviewProvider(entries);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().HaveCount(4);
        result[0].Name.Should().Be("Anchoring Damavik", "Priority 1 comes first regardless of distance");
        result[1].Name.Should().Be("Lucid Firewatcher", "Priority 2 comes second");
        result[2].Name.Should().Be("Renewing Leshak", "Priority 3 comes third");
    }

    [Fact]
    public void Given_NullOverviewEntries_When_GettingEnemies_Then_ReturnsEmptyArray()
    {
        // Arrange
        var overview = Substitute.For<IOverviewProvider>();
        overview.Entries.Returns((IOverviewEntry[]?)null);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().BeEmpty("Null entries should return empty array");
    }

    [Fact]
    public void Given_EntriesWithNullDistance_When_GettingEnemies_Then_TreatsAsMaxDistance()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", distance: 10000, isEnemy: true),
            CreateOverviewEntry("Unknown Enemy", "Unknown Enemy", distance: null, isEnemy: true)
        };
        var overview = CreateOverviewProvider(entries);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        // The entry with null distance should be sorted last
        result[0].Name.Should().Be("Ghosting Damavik");
    }

    [Fact]
    public void Given_ComplexBattleScenario_When_GettingEnemies_Then_AppliesAllFiltersAndSorting()
    {
        // Arrange
        _shipFit.MaxTargetingRange.Returns(40000);
        var entries = new List<IOverviewEntry>
        {
            // Valid targets
            CreateOverviewEntry("Anchoring Vila Damavik", "Anchoring Vila Damavik", distance: 15000, isEnemy: true),
            CreateOverviewEntry("Lucid Firewatcher", "Lucid Firewatcher", distance: 20000, isEnemy: true),
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", distance: 10000, isEnemy: true),

            // Should be filtered out
            CreateOverviewEntry("Vila Swarmer", "Vila Swarmer", distance: 5000, isEnemy: true), // Filtered by type
            CreateOverviewEntry("Extraction Node", "Triglavian Extraction Node", distance: 8000, isEnemy: true), // Filtered by name
            CreateOverviewEntry("Distant Enemy", "Ghosting Damavik", distance: 50000, isEnemy: true), // Beyond range
            CreateOverviewEntry("Friendly", "Friendly Ship", distance: 12000, isEnemy: false) // Not enemy
        };
        var overview = CreateOverviewProvider(entries);

        // Act
        var result = _sut.GetEnemies(overview);

        // Assert
        result.Should().HaveCount(3, "Should filter out Vila Swarmer, Extraction, distant, and friendly");
        result[0].Name.Should().Be("Anchoring Vila Damavik", "Anchoring has priority 1");
        result[1].Name.Should().Be("Lucid Firewatcher", "Firewatcher has priority 2");
        result[2].Name.Should().Be("Ghosting Damavik", "Regular enemy sorted by DPS");
    }

    #endregion

    #region Helper Methods

    private static IOverviewProvider CreateOverviewProvider(List<IOverviewEntry> entries)
    {
        var provider = Substitute.For<IOverviewProvider>();
        provider.Entries.Returns(entries.ToArray());
        return provider;
    }

    private static IOverviewEntry CreateOverviewEntry(
        string name,
        string? type = null,
        int? distance = 10000,
        bool isEnemy = true,
        bool targeting = false,
        bool targetedByMe = false)
    {
        var entry = Substitute.For<IOverviewEntry>();
        entry.Name.Returns(name);
        entry.Type.Returns(type ?? name);
        entry.Distance.Returns(distance);
        entry.IsEnemy.Returns(isEnemy);

        var indications = new Sanderling.Interface.MemoryStruct.OverviewWindowEntryCommonIndications
        {
            Targeting = targeting,
            TargetedByMe = targetedByMe
        };
        entry.CommonIndications.Returns(indications);

        return entry;
    }

    #endregion
}
