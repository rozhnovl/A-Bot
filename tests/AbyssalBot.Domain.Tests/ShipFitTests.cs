using Sanderling.ABot.Bot;
using Sanderling.Interface.MemoryStruct;
using WindowsInput.Native;

namespace AbyssalBot.Domain.Tests;

public class ShipFitTests
{
    #region GetAlwaysActiveModules Tests

    [Fact]
    public void Given_FitWithHardeners_When_GettingAlwaysActiveModules_Then_ReturnsOnlyHardeners()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[2], ShipFit.ModuleType.ShieldBooster);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var modules = shipFit.GetAlwaysActiveModules().ToList();

        // Assert
        modules.Should().HaveCount(2, "Only hardeners should be returned");
        modules.Should().AllSatisfy(m => m.Type.Should().Be(ShipFit.ModuleType.Hardener));
    }

    [Fact]
    public void Given_FitWithoutHardeners_When_GettingAlwaysActiveModules_Then_ReturnsEmpty()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[1], ShipFit.ModuleType.ShieldBooster);
        AddModule(fitInfo[2], ShipFit.ModuleType.MWD);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var modules = shipFit.GetAlwaysActiveModules().ToList();

        // Assert
        modules.Should().BeEmpty("No hardeners in fit");
    }

    [Fact]
    public void Given_FitWithMultipleHardeners_When_GettingAlwaysActiveModules_Then_ReturnsAllHardeners()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[2], ShipFit.ModuleType.Hardener);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var modules = shipFit.GetAlwaysActiveModules().ToList();

        // Assert
        modules.Should().HaveCount(4, "All hardeners should be returned");
    }

    #endregion

    #region GetShieldBoostersModules Tests

    [Fact]
    public void Given_FitWithShieldBoosters_When_GettingShieldBoosters_Then_ReturnsOnlyShieldBoosters()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[1], ShipFit.ModuleType.ShieldBooster);
        AddModule(fitInfo[1], ShipFit.ModuleType.ShieldBooster);
        AddModule(fitInfo[2], ShipFit.ModuleType.Hardener);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var modules = shipFit.GetShieldBoostersModules().ToList();

        // Assert
        modules.Should().HaveCount(2, "Only shield boosters should be returned");
        modules.Should().AllSatisfy(m => m.Type.Should().Be(ShipFit.ModuleType.ShieldBooster));
    }

    [Fact]
    public void Given_FitWithoutShieldBoosters_When_GettingShieldBoosters_Then_ReturnsEmpty()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[2], ShipFit.ModuleType.MWD);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var modules = shipFit.GetShieldBoostersModules().ToList();

        // Assert
        modules.Should().BeEmpty("No shield boosters in fit");
    }

    #endregion

    #region GetWeapon Tests

    [Fact]
    public void Given_FitWithWeapon_When_GettingWeapon_Then_ReturnsWeapon()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var weapon = shipFit.GetWeapon();

        // Assert
        weapon.Should().NotBeNull("Weapon should be found");
        weapon!.Type.Should().Be(ShipFit.ModuleType.Weapon);
    }

    [Fact]
    public void Given_FitWithoutWeapon_When_GettingWeapon_Then_ReturnsNull()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[2], ShipFit.ModuleType.ShieldBooster);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var weapon = shipFit.GetWeapon();

        // Assert
        weapon.Should().BeNull("No weapon in fit");
    }

    [Fact]
    public void Given_FitWithMultipleWeapons_When_GettingWeapon_Then_ReturnsFirstWeapon()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        var firstWeapon = AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var weapon = shipFit.GetWeapon();

        // Assert
        weapon.Should().NotBeNull("Should return first weapon");
        weapon.Should().BeSameAs(firstWeapon);
    }

    #endregion

    #region GetMWD Tests

    [Fact]
    public void Given_FitWithMWD_When_GettingMWD_Then_ReturnsMWD()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[1], ShipFit.ModuleType.MWD);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var mwd = shipFit.GetMWD();

        // Assert
        mwd.Should().NotBeNull("MWD should be found");
        mwd!.Type.Should().Be(ShipFit.ModuleType.MWD);
    }

    [Fact]
    public void Given_FitWithoutMWD_When_GettingMWD_Then_ReturnsNull()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var mwd = shipFit.GetMWD();

        // Assert
        mwd.Should().BeNull("No MWD in fit");
    }

    #endregion

    #region GetAllByType Tests

    [Fact]
    public void Given_FitWithMultipleHardeners_When_GettingAllHardeners_Then_ReturnsAllHardeners()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[2], ShipFit.ModuleType.ShieldBooster);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var hardeners = shipFit.GetAllByType(ShipFit.ModuleType.Hardener).ToList();

        // Assert
        hardeners.Should().HaveCount(3, "All hardeners should be returned");
        hardeners.Should().AllSatisfy(m => m.Type.Should().Be(ShipFit.ModuleType.Hardener));
    }

    [Fact]
    public void Given_FitWithNoMatchingModules_When_GettingAllByType_Then_ReturnsEmpty()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var mwds = shipFit.GetAllByType(ShipFit.ModuleType.MWD).ToList();

        // Assert
        mwds.Should().BeEmpty("No MWDs in fit");
    }

    [Fact]
    public void Given_FitWithMixedModules_When_GettingAllWeapons_Then_ReturnsOnlyWeapons()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        AddModule(fitInfo[2], ShipFit.ModuleType.ShieldBooster);

        var shipFit = new ShipFit(shipUi, fitInfo);

        // Act
        var weapons = shipFit.GetAllByType(ShipFit.ModuleType.Weapon).ToList();

        // Assert
        weapons.Should().HaveCount(2, "Only weapons should be returned");
        weapons.Should().AllSatisfy(m => m.Type.Should().Be(ShipFit.ModuleType.Weapon));
    }

    #endregion

    #region ModuleInfo.EnsureActive Tests

    [Fact]
    public void Given_InactiveModule_When_EnsuringActive_Then_ReturnsActivationTask()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var uiModule = CreateModuleButton(isActive: false);
        var module = new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener)
        {
            UiModule = uiModule
        };

        // Act
        var task = module.EnsureActive(bot, shouldBeActive: true, shouldBeOverloaded: false);

        // Assert
        task.Should().NotBeNull("Should return activation task");
        task.Should().BeOfType<ModuleToggleTask>();
    }

    [Fact]
    public void Given_ActiveModule_When_EnsuringActive_Then_ReturnsNull()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var uiModule = CreateModuleButton(isActive: true);
        var module = new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener)
        {
            UiModule = uiModule
        };

        // Act
        var task = module.EnsureActive(bot, shouldBeActive: true, shouldBeOverloaded: false);

        // Assert
        task.Should().BeNull("Module already active");
    }

    [Fact]
    public void Given_ActiveModule_When_EnsuringInactive_Then_ReturnsDeactivationTask()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var uiModule = CreateModuleButton(isActive: true);
        var module = new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener)
        {
            UiModule = uiModule
        };

        // Act
        var task = module.EnsureActive(bot, shouldBeActive: false, shouldBeOverloaded: false);

        // Assert
        task.Should().NotBeNull("Should return deactivation task");
        task.Should().BeOfType<ModuleToggleTask>();
    }

    [Fact]
    public void Given_InactiveModule_When_EnsuringInactive_Then_ReturnsNull()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var uiModule = CreateModuleButton(isActive: false);
        var module = new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener)
        {
            UiModule = uiModule
        };

        // Act
        var task = module.EnsureActive(bot, shouldBeActive: false, shouldBeOverloaded: false);

        // Assert
        task.Should().BeNull("Module already inactive");
    }

    [Fact]
    public void Given_ModuleWithNullActiveState_When_EnsuringActive_Then_AssumesActiveAndReturnsNull()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var uiModule = CreateModuleButton(isActive: null);
        var module = new ShipFit.ModuleInfo(ShipFit.ModuleType.Hardener)
        {
            UiModule = uiModule
        };

        // Act
        var task = module.EnsureActive(bot, shouldBeActive: true, shouldBeOverloaded: false);

        // Assert
        task.Should().BeNull("Null active state is treated as active (true)");
    }

    #endregion

    #region Module Organization Tests

    [Fact]
    public void Given_ModulesInThreeRows_When_CreatingFit_Then_OrganizesModulesCorrectly()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        var highModule = AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        var midModule = AddModule(fitInfo[1], ShipFit.ModuleType.Hardener);
        var lowModule = AddModule(fitInfo[2], ShipFit.ModuleType.Etc);

        // Act
        var shipFit = new ShipFit(shipUi, fitInfo);

        // Assert
        shipFit.GetWeapon().Should().BeSameAs(highModule, "Weapon should be in high slot");
        shipFit.GetAlwaysActiveModules().Should().Contain(midModule, "Hardener should be in mid slot");
    }

    [Fact]
    public void Given_MoreUIModulesThanFitInfo_When_CreatingFit_Then_AssignsExtraAsEtcType()
    {
        // Arrange
        var (shipUi, fitInfo) = CreateTestFitSetup();
        // Only define first module in row 0, the rest will be auto-assigned as Etc
        AddModule(fitInfo[0], ShipFit.ModuleType.Weapon);
        fitInfo[0] = new[] { fitInfo[0][0] }; // Only one module defined

        // Act
        var shipFit = new ShipFit(shipUi, fitInfo);

        // Assert
        var allModules = shipFit.GetAllByType(ShipFit.ModuleType.Weapon)
            .Concat(shipFit.GetAllByType(ShipFit.ModuleType.Etc))
            .ToList();
        allModules.Should().Contain(m => m.Type == ShipFit.ModuleType.Etc,
            "Extra modules should be assigned as Etc type");
    }

    #endregion

    #region Helper Methods

    private static (IShipUi shipUi, ShipFit.ModuleInfo[][] fitInfo) CreateTestFitSetup()
    {
        var shipUi = Substitute.For<IShipUi>();

        // Create 3 rows of module buttons (High, Mid, Low)
        var highModules = CreateModuleRow(yPosition: 100, count: 3);
        var midModules = CreateModuleRow(yPosition: 200, count: 3);
        var lowModules = CreateModuleRow(yPosition: 300, count: 3);

        var allModules = highModules.Concat(midModules).Concat(lowModules).ToArray();
        shipUi.ModuleButtons.Returns(allModules);

        // Initialize empty fit info for 3 rows
        var fitInfo = new[]
        {
            new List<ShipFit.ModuleInfo>(), // High
            new List<ShipFit.ModuleInfo>(), // Mid
            new List<ShipFit.ModuleInfo>()  // Low
        };

        return (shipUi, fitInfo.Select(l => l.ToArray()).ToArray());
    }

    private static ShipUIModuleButton[] CreateModuleRow(int yPosition, int count)
    {
        var modules = new List<ShipUIModuleButton>();
        for (int i = 0; i < count; i++)
        {
            var module = Substitute.For<ShipUIModuleButton>();
            var region = new Region2D { Min0 = i * 100, Min1 = yPosition };
            var uiNode = Substitute.For<IUIElement>();
            uiNode.Region.Returns(region);
            module.UINode.Returns(uiNode);
            module.IsActive.Returns(false);
            module.IsBusy.Returns(false);
            modules.Add(module);
        }
        return modules.ToArray();
    }

    private static ShipFit.ModuleInfo AddModule(ShipFit.ModuleInfo[][] fitInfo, int row, ShipFit.ModuleType type)
    {
        var module = new ShipFit.ModuleInfo(type);
        var list = fitInfo[row].ToList();
        list.Add(module);
        fitInfo[row] = list.ToArray();
        return module;
    }

    private static ShipFit.ModuleInfo AddModule(List<ShipFit.ModuleInfo> row, ShipFit.ModuleType type)
    {
        var module = new ShipFit.ModuleInfo(type);
        row.Add(module);
        return module;
    }

    private static ShipUIModuleButton CreateModuleButton(bool? isActive = false, bool isBusy = false)
    {
        var button = Substitute.For<ShipUIModuleButton>();
        button.IsActive.Returns(isActive);
        button.IsBusy.Returns(isBusy);
        return button;
    }

    #endregion
}
