using Sanderling.ABot.Bot;
using Sanderling.ABot.Bot.Strategies;

namespace AbyssalBot.Domain.Tests;

public class NpcInfoProviderTests
{
    private readonly NpcInfoProvider _sut;

    public NpcInfoProviderTests()
    {
        _sut = new NpcInfoProvider();
    }

    #region CalcTargetPriority Tests

    [Fact]
    public void Given_AnchoringEnemy_When_CalculatingPriority_Then_ReturnsHighestPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Anchoring Damavik");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(1, "Anchoring enemies should have the highest priority");
    }

    [Fact]
    public void Given_FirewatcherEnemy_When_CalculatingPriority_Then_ReturnsSecondPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Lucid Firewatcher");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(2, "Firewatcher enemies should have priority 2");
    }

    [Fact]
    public void Given_RenewingEnemy_When_CalculatingPriority_Then_ReturnsThirdPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Renewing Leshak");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(3, "Renewing enemies should have priority 3");
    }

    [Fact]
    public void Given_PlateforgerEnemy_When_CalculatingPriority_Then_ReturnsThirdPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Plateforger Tessella");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(3, "Plateforger enemies should have priority 3");
    }

    [Fact]
    public void Given_FieldweaverEnemy_When_CalculatingPriority_Then_ReturnsThirdPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Fieldweaver Tessella");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(3, "Fieldweaver enemies should have priority 3");
    }

    [Fact]
    public void Given_EntanglerEnemy_When_CalculatingPriority_Then_ReturnsSixthPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Ephialtes Entangler");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(6, "Entangler enemies should have priority 6");
    }

    [Fact]
    public void Given_SnarecasterEnemy_When_CalculatingPriority_Then_ReturnsSixthPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Snarecaster Tessella");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(6, "Snarecaster enemies should have priority 6");
    }

    [Fact]
    public void Given_ScyllaEnemy_When_CalculatingPriority_Then_ReturnsEighthPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Scylla Something");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(8, "Scylla enemies should have priority 8");
    }

    [Fact]
    public void Given_TyrannosEnemy_When_CalculatingPriority_Then_ReturnsEighthPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Tyrannos Something");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(8, "Tyrannos enemies should have priority 8");
    }

    [Fact]
    public void Given_ExtractionEnemy_When_CalculatingPriority_Then_ReturnsTenthPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Triglavian Extraction Node");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(10, "Extraction enemies should have priority 10");
    }

    [Fact]
    public void Given_BioadaptiveCache_When_CalculatingPriority_Then_ReturnsVeryLowPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Triglavian Bioadaptive Cache");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(1000, "Bioadaptive caches should have very low priority (1000)");
    }

    [Fact]
    public void Given_DrifterBattleship_When_CalculatingPriority_Then_ReturnsVeryLowPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Drifter Assault Battleship", "Drifter Assault Battleship");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(9000, "Drifter Battleships should have very low priority (9000)");
    }

    [Fact]
    public void Given_GuristasEnemy_When_CalculatingPriority_Then_ReturnsLowestPriority()
    {
        // Arrange
        var entry = CreateOverviewEntry("Guristas Despoiler");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert
        priority.Should().Be(20, "Guristas enemies should have priority 20");
    }

    [Fact]
    public void Given_RegularHighDpsEnemy_When_CalculatingPriority_Then_ReturnsPriorityBasedOnDps()
    {
        // Arrange
        var entry = CreateOverviewEntry("Starving Vedmak", "Starving Vedmak");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert - Priority is 800 - DPS (800 - 237.6 = 562.4)
        priority.Should().BeInRange(562, 563, "Priority should be calculated as 800 - DPS");
    }

    [Fact]
    public void Given_RegularLowDpsEnemy_When_CalculatingPriority_Then_ReturnsPriorityBasedOnDps()
    {
        // Arrange
        var entry = CreateOverviewEntry("Lucid Escort", "Lucid Escort");

        // Act
        var priority = _sut.CalcTargetPriority(entry);

        // Assert - Priority is 800 - DPS (800 - 24 = 776)
        priority.Should().Be(776, "Priority should be calculated as 800 - DPS");
    }

    #endregion

    #region CalculateApproximateDps Tests

    [Fact]
    public void Given_SingleKnownEnemy_When_CalculatingDps_Then_ReturnsCorrectDps()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Sparkneedle Tessella", "Sparkneedle Tessella", isEnemy: true)
        };

        // Act
        var dps = _sut.CalculateApproximateDps(entries);

        // Assert
        dps.Should().Be(25, "Sparkneedle Tessella has 25 DPS");
    }

    [Fact]
    public void Given_MultipleEnemies_When_CalculatingDps_Then_ReturnsSumOfDps()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Sparkneedle Tessella", "Sparkneedle Tessella", isEnemy: true),
            CreateOverviewEntry("Emberneedle Tessella", "Emberneedle Tessella", isEnemy: true),
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", isEnemy: true)
        };

        // Act
        var dps = _sut.CalculateApproximateDps(entries);

        // Assert
        dps.Should().Be(86, "Total DPS should be 25 + 25 + 36 = 86");
    }

    [Fact]
    public void Given_HighDpsEnemies_When_CalculatingDps_Then_ReturnsCorrectSum()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Starving Vedmak", "Starving Vedmak", isEnemy: true),
            CreateOverviewEntry("Harrowing Vedmak", "Harrowing Vedmak", isEnemy: true)
        };

        // Act
        var dps = _sut.CalculateApproximateDps(entries);

        // Assert
        dps.Should().Be(475.2, "Total DPS should be 237.6 + 237.6 = 475.2");
    }

    [Fact]
    public void Given_ZeroDpsEnemies_When_CalculatingDps_Then_ReturnsZero()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Vila Swarmer", "Vila Swarmer", isEnemy: true),
            CreateOverviewEntry("Triglavian Bioadaptive Cache", "Triglavian Bioadaptive Cache", isEnemy: true)
        };

        // Act
        var dps = _sut.CalculateApproximateDps(entries);

        // Assert
        dps.Should().Be(0, "Both enemies have 0 DPS");
    }

    [Fact]
    public void Given_EmptyList_When_CalculatingDps_Then_ReturnsZero()
    {
        // Arrange
        var entries = new List<IOverviewEntry>();

        // Act
        var dps = _sut.CalculateApproximateDps(entries);

        // Assert
        dps.Should().Be(0, "Empty list should return 0 DPS");
    }

    [Fact]
    public void Given_UnknownEnemy_When_CalculatingDps_Then_ThrowsException()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Unknown Enemy Type", "Unknown Enemy Type", isEnemy: true)
        };

        // Act
        var act = () => _sut.CalculateApproximateDps(entries);

        // Assert
        act.Should().Throw<KeyNotFoundException>("Unknown enemy types should throw exception");
    }

    [Fact]
    public void Given_OverviewProviderWithEnemies_When_CalculatingDps_Then_ReturnsOnlyEnemyDps()
    {
        // Arrange
        var overviewProvider = Substitute.For<IOverviewProvider>();
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Sparkneedle Tessella", "Sparkneedle Tessella", isEnemy: true),
            CreateOverviewEntry("Friendly Ship", "Friendly Ship", isEnemy: false),
            CreateOverviewEntry("Ghosting Damavik", "Ghosting Damavik", isEnemy: true)
        };
        overviewProvider.Entries.Returns(entries.ToArray());

        // Act
        var dps = _sut.CalculateApproximateDps(overviewProvider);

        // Assert
        dps.Should().Be(61, "Should only count enemy DPS: 25 + 36 = 61");
    }

    [Fact]
    public void Given_VilaDamavikVariants_When_CalculatingDps_Then_ReturnsCorrectCombinedDps()
    {
        // Arrange
        var entries = new List<IOverviewEntry>
        {
            CreateOverviewEntry("Striking Vila Damavik", "Striking Vila Damavik", isEnemy: true),
            CreateOverviewEntry("Tangling Vila Damavik", "Tangling Vila Damavik", isEnemy: true)
        };

        // Act
        var dps = _sut.CalculateApproximateDps(entries);

        // Assert
        dps.Should().Be(98, "Vila Damavik variants should have 49 DPS each (9 + 40)");
    }

    #endregion

    #region IsOrbitBeacon Tests

    [Fact]
    public void Given_LeshakEnemy_When_CheckingOrbitBeacon_Then_ReturnsTrue()
    {
        // Arrange
        var entry = CreateOverviewEntry("Striking Leshak");

        // Act
        var result = _sut.IsOrbitBeacon(entry);

        // Assert
        result.Should().BeTrue("Leshak enemies should be orbited");
    }

    [Fact]
    public void Given_OvermindEnemy_When_CheckingOrbitBeacon_Then_ReturnsTrue()
    {
        // Arrange
        var entry = CreateOverviewEntry("Hadal Abyssal Overmind");

        // Act
        var result = _sut.IsOrbitBeacon(entry);

        // Assert
        result.Should().BeTrue("Overmind enemies should be orbited");
    }

    [Fact]
    public void Given_BattleshipEnemy_When_CheckingOrbitBeacon_Then_ReturnsTrue()
    {
        // Arrange
        var entry = CreateOverviewEntry("Drifter Assault Battleship");

        // Act
        var result = _sut.IsOrbitBeacon(entry);

        // Assert
        result.Should().BeTrue("Battleship enemies should be orbited");
    }

    [Fact]
    public void Given_SmallShipEnemy_When_CheckingOrbitBeacon_Then_ReturnsFalse()
    {
        // Arrange
        var entry = CreateOverviewEntry("Ghosting Damavik");

        // Act
        var result = _sut.IsOrbitBeacon(entry);

        // Assert
        result.Should().BeFalse("Small ships should not require orbiting");
    }

    [Fact]
    public void Given_FrigateEnemy_When_CheckingOrbitBeacon_Then_ReturnsFalse()
    {
        // Arrange
        var entry = CreateOverviewEntry("Lucid Escort");

        // Act
        var result = _sut.IsOrbitBeacon(entry);

        // Assert
        result.Should().BeFalse("Frigates should not require orbiting");
    }

    #endregion

    #region Helper Methods

    private static IOverviewEntry CreateOverviewEntry(string name, string? type = null, bool isEnemy = true)
    {
        var entry = Substitute.For<IOverviewEntry>();
        entry.Name.Returns(name);
        entry.Type.Returns(type ?? name); // Use name as type if type not specified
        entry.IsEnemy.Returns(isEnemy);
        return entry;
    }

    #endregion
}
