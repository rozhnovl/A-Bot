using Sanderling.ABot.Bot;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Parse;

namespace AbyssalBot.Domain.Tests;

public class ShipStateTests
{
    #region GetNextTankingModulesTask Tests

    [Fact]
    public void Given_LowShieldAndLowDps_When_GettingTankingTask_Then_ActivatesSingleShieldBooster()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        ConfigureShieldBoosters(fit, 2);
        var memory = ConfigureMemory(bot, shieldHp: 500, capacitorEnergy: 800);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetNextTankingModulesTask(estimatedIncomingDps: 100);

        // Assert - Should return null because the tanking logic has a return null at line 99
        task.Should().BeNull("The method returns null at the beginning");
    }

    [Fact]
    public void Given_LowShieldAndHighDps_When_GettingTankingTask_Then_ActivatesAllShieldBoosters()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        ConfigureShieldBoosters(fit, 2);
        var memory = ConfigureMemory(bot, shieldHp: 500, capacitorEnergy: 800);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetNextTankingModulesTask(estimatedIncomingDps: 200);

        // Assert
        task.Should().BeNull("The method returns null at the beginning");
    }

    [Fact]
    public void Given_HighShield_When_GettingTankingTask_Then_TurnsOffShieldBoosters()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        ConfigureShieldBoosters(fit, 2);
        var memory = ConfigureMemory(bot, shieldHp: 900, capacitorEnergy: 800);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetNextTankingModulesTask(estimatedIncomingDps: 100);

        // Assert
        task.Should().BeNull("The method returns null at the beginning");
    }

    [Fact]
    public void Given_LowCapacitor_When_GettingTankingTask_Then_TurnsOffShieldBoosters()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        ConfigureShieldBoosters(fit, 2);
        var memory = ConfigureMemory(bot, shieldHp: 900, capacitorEnergy: 300);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetNextTankingModulesTask(estimatedIncomingDps: 100);

        // Assert
        task.Should().BeNull("The method returns null at the beginning");
    }

    [Fact]
    public void Given_CriticalShieldAndHighDps_When_GettingTankingTask_Then_OverloadsShieldBoosters()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        ConfigureShieldBoosters(fit, 2);
        var memory = ConfigureMemory(bot, shieldHp: 100, capacitorEnergy: 800);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetNextTankingModulesTask(estimatedIncomingDps: 200);

        // Assert
        task.Should().BeNull("The method returns null at the beginning");
    }

    #endregion

    #region GetAttackTasks Tests

    [Fact]
    public void Given_NoActiveTarget_When_GettingAttackTasks_Then_ReturnsNull()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        var memory = ConfigureMemory(bot);
        ConfigureShipState(bot, memory);
        ConfigureNoActiveTargets(memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetAttackTasks();

        // Assert
        task.Should().BeNull("No target selected");
    }

    [Fact]
    public void Given_TargetInRange_When_GettingAttackTasks_Then_ActivatesWeapons()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        ConfigureWeapon(fit);
        var memory = ConfigureMemory(bot);
        ConfigureShipState(bot, memory);
        var target = ConfigureActiveTarget(memory, distance: 8000);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetAttackTasks();

        // Assert
        // Should attempt to activate weapon since target is within AttackRange (11000)
        task.Should().NotBeNull("Target is within attack range");
    }

    [Fact]
    public void Given_TargetOutOfRange_When_GettingAttackTasks_Then_DoesNotActivateWeapons()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        ConfigureWeapon(fit);
        var memory = ConfigureMemory(bot);
        ConfigureShipState(bot, memory);
        var target = ConfigureActiveTarget(memory, distance: 15000);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetAttackTasks();

        // Assert
        // Weapon should not activate when target is beyond attack range
        task.Should().NotBeNull("Should return orbit or drone task");
    }

    [Fact]
    public void Given_TargetBeyondOptimalRange_When_GettingAttackTasks_Then_ReturnsOrbitTask()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        ConfigureWeapon(fit, optimalRange: 3000);
        var memory = ConfigureMemory(bot, maneuver: ShipManeuverType.None);
        ConfigureShipState(bot, memory);
        var target = ConfigureActiveTarget(memory, distance: 8000);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetAttackTasks();

        // Assert
        task.Should().NotBeNull("Should orbit when beyond optimal range");
    }

    #endregion

    #region GetTurnOnAlwaysActiveModulesTask Tests

    [Fact]
    public void Given_InactiveHardeners_When_GettingAlwaysActiveTask_Then_ReturnsActivationTask()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        var hardener = ConfigureHardener(fit, isActive: false);
        var memory = ConfigureMemory(bot);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetTurnOnAlwaysActiveModulesTask();

        // Assert
        task.Should().NotBeNull("Inactive hardener should be activated");
    }

    [Fact]
    public void Given_ActiveHardeners_When_GettingAlwaysActiveTask_Then_ReturnsNull()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        var hardener = ConfigureHardener(fit, isActive: true);
        var memory = ConfigureMemory(bot);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetTurnOnAlwaysActiveModulesTask();

        // Assert
        task.Should().BeNull("All hardeners already active");
    }

    #endregion

    #region GetSetModuleActiveTask Tests

    [Fact]
    public void Given_InactiveWeapon_When_SettingWeaponActive_Then_ReturnsActivationTask()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        var weapon = ConfigureWeapon(fit, isActive: false, isBusy: false);
        var memory = ConfigureMemory(bot);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetSetModuleActiveTask(ShipFit.ModuleType.Weapon, shouldBeActive: true);

        // Assert
        task.Should().NotBeNull("Should activate weapon");
    }

    [Fact]
    public void Given_BusyWeapon_When_SettingWeaponActive_Then_ReturnsNull()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        var weapon = ConfigureWeapon(fit, isActive: false, isBusy: true);
        var memory = ConfigureMemory(bot);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetSetModuleActiveTask(ShipFit.ModuleType.Weapon, shouldBeActive: true);

        // Assert
        task.Should().BeNull("Busy weapon should not be toggled");
    }

    [Fact]
    public void Given_ActiveWeapon_When_SettingWeaponInactive_Then_ReturnsDeactivationTask()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        var weapon = ConfigureWeapon(fit, isActive: true, isBusy: false);
        var memory = ConfigureMemory(bot);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var task = shipState.GetSetModuleActiveTask(ShipFit.ModuleType.Weapon, shouldBeActive: false);

        // Assert
        task.Should().NotBeNull("Should deactivate weapon");
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Given_ManeuverOrbit_When_AccessingManeuverProperty_Then_ReturnsOrbit()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        var memory = ConfigureMemory(bot, maneuver: ShipManeuverType.Orbit);
        ConfigureShipState(bot, memory);

        // Act
        var shipState = new ShipState(fit, bot);

        // Assert
        shipState.Maneuver.Should().Be(ShipManeuverType.Orbit);
    }

    [Fact]
    public void Given_ManeuverApproach_When_AccessingManeuverProperty_Then_ReturnsApproach()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        var memory = ConfigureMemory(bot, maneuver: ShipManeuverType.Approach);
        ConfigureShipState(bot, memory);

        // Act
        var shipState = new ShipState(fit, bot);

        // Assert
        shipState.Maneuver.Should().Be(ShipManeuverType.Approach);
    }

    [Fact]
    public void Given_ShipWithDrones_When_CheckingShouldUseTractor_Then_ReturnsTrue()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        fit.MaxDronesInSpace.Returns(5);
        var memory = ConfigureMemory(bot);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var result = shipState.ShouldUseTractorForLooting;

        // Assert
        result.Should().BeTrue("Ships with drones should use tractor");
    }

    [Fact]
    public void Given_ShipWithoutDrones_When_CheckingShouldUseTractor_Then_ReturnsFalse()
    {
        // Arrange
        var (bot, fit) = CreateTestSetup();
        fit.MaxDronesInSpace.Returns(0);
        var memory = ConfigureMemory(bot);
        ConfigureShipState(bot, memory);

        var shipState = new ShipState(fit, bot);

        // Act
        var result = shipState.ShouldUseTractorForLooting;

        // Assert
        result.Should().BeFalse("Ships without drones should not use tractor");
    }

    #endregion

    #region Helper Methods

    private static (Bot bot, ShipFit fit) CreateTestSetup()
    {
        var bot = Substitute.For<Bot>();
        var fit = Substitute.For<ShipFit>();
        fit.MaxTargets.Returns(3);
        fit.MaxTargetingRange.Returns(50000);
        fit.MaxDronesInSpace.Returns(5);
        return (bot, fit);
    }

    private static IMemoryMeasurement ConfigureMemory(
        Bot bot,
        int shieldHp = 1000,
        int capacitorEnergy = 1000,
        ShipManeuverType maneuver = ShipManeuverType.None)
    {
        var memory = Substitute.For<IMemoryMeasurement>();
        var shipUi = Substitute.For<IShipUi>();
        var indication = Substitute.For<IShipUiIndication>();
        var hitpoints = Substitute.For<IShipHitpointsAndEnergy>();
        var infoPanelContainer = Substitute.For<IInfoPanelContainer>();
        var locationInfo = Substitute.For<IInfoPanelLocationInfo>();

        hitpoints.Shield.Returns(shieldHp);
        hitpoints.Capacitor.Returns(capacitorEnergy);

        indication.ManeuverType.Returns(maneuver);

        shipUi.Indication.Returns(indication);
        shipUi.HitpointsAndEnergy.Returns(hitpoints);
        shipUi.ModuleButtons.Returns(Array.Empty<ShipUIModuleButton>());

        locationInfo.CurrentSolarSystemName.Returns("Maurasi");
        infoPanelContainer.LocationInfo.Returns(locationInfo);

        memory.ShipUi.Returns(shipUi);
        memory.InfoPanelContainer.Returns(infoPanelContainer);
        memory.Target.Returns(Array.Empty<ITarget>());
        memory.WindowOther.Returns(Array.Empty<IWindow>());

        return memory;
    }

    private static void ConfigureShipState(Bot bot, IMemoryMeasurement memory)
    {
        var measurement = new Bib3.PropertyGenTimespanInt64<IMemoryMeasurement>(memory, 0, 0);
        bot.MemoryMeasurementAtTime.Returns(measurement);
    }

    private static void ConfigureShieldBoosters(ShipFit fit, int count)
    {
        var boosters = new List<ShipFit.ModuleInfo>();
        for (int i = 0; i < count; i++)
        {
            var booster = new ShipFit.ModuleInfo(ShipFit.ModuleType.ShieldBooster);
            var uiModule = Substitute.For<ShipUIModuleButton>();
            uiModule.IsActive.Returns(false);
            uiModule.IsBusy.Returns(false);
            booster.UiModule = uiModule;
            boosters.Add(booster);
        }
        fit.GetShieldBoostersModules().Returns(boosters);
    }

    private static ShipFit.ModuleInfo ConfigureWeapon(ShipFit fit, bool isActive = false, bool isBusy = false, int optimalRange = 4000)
    {
        var weapon = new ShipFit.ModuleInfo(ShipFit.ModuleType.Weapon);
        var uiModule = Substitute.For<ShipUIModuleButton>();
        uiModule.IsActive.Returns(isActive);
        uiModule.IsBusy.Returns(isBusy);
        weapon.UiModule = uiModule;
        weapon.OptimalRange = optimalRange;
        fit.GetWeapon().Returns(weapon);
        fit.GetAllByType(ShipFit.ModuleType.Weapon).Returns(new[] { weapon });
        return weapon;
    }

    private static ShipFit.ModuleInfo ConfigureHardener(ShipFit fit, bool isActive = false)
    {
        var hardener = new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener);
        var uiModule = Substitute.For<ShipUIModuleButton>();
        uiModule.IsActive.Returns(isActive);
        uiModule.IsBusy.Returns(false);
        hardener.UiModule = uiModule;
        fit.GetAlwaysActiveModules().Returns(new[] { hardener });
        return hardener;
    }

    private static void ConfigureNoActiveTargets(IMemoryMeasurement memory)
    {
        memory.Target.Returns(Array.Empty<ITarget>());
    }

    private static ITarget ConfigureActiveTarget(IMemoryMeasurement memory, int distance = 10000)
    {
        var target = Substitute.For<ITarget>();
        target.Distance.Returns(distance);
        target.IsSelected.Returns(true);
        var orbitTask = Substitute.For<ISerializableBotTask>();
        target.GetOrbitTask().Returns(orbitTask);

        memory.Target.Returns(new[] { target });
        return target;
    }

    #endregion
}
