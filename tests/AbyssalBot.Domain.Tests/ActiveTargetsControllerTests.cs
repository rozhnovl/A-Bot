using Sanderling.ABot.Bot;
using Sanderling.Parse;

namespace AbyssalBot.Domain.Tests;

public class ActiveTargetsControllerTests
{
    #region Constructor and Basic Properties Tests

    [Fact]
    public void Given_NoTargets_When_CreatingController_Then_HasEmptyList()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var memory = CreateMemoryMeasurement(new List<ITarget>());

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.List.Should().BeEmpty("No targets in memory");
        controller.Count.Should().Be(0);
        controller.ActiveTarget.Should().BeNull("No selected target");
    }

    [Fact]
    public void Given_MultipleTargets_When_CreatingController_Then_HasAllTargets()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var targets = new List<ITarget>
        {
            CreateTarget("Target 1", isSelected: false),
            CreateTarget("Target 2", isSelected: false),
            CreateTarget("Target 3", isSelected: false)
        };
        var memory = CreateMemoryMeasurement(targets);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.List.Should().HaveCount(3, "All targets should be in list");
        controller.Count.Should().Be(3);
    }

    [Fact]
    public void Given_NullTargetArray_When_CreatingController_Then_HandlesSafely()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var memory = Substitute.For<IMemoryMeasurement>();
        memory.Target.Returns((ITarget[]?)null);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.List.Should().BeNull("Null targets should result in null list");
        controller.Count.Should().Be(0, "Count should be 0 for null list");
    }

    #endregion

    #region Active Target Tests

    [Fact]
    public void Given_OneSelectedTarget_When_CreatingController_Then_SetsActiveTarget()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var targets = new List<ITarget>
        {
            CreateTarget("Target 1", isSelected: false),
            CreateTarget("Target 2", isSelected: true),
            CreateTarget("Target 3", isSelected: false)
        };
        var memory = CreateMemoryMeasurement(targets);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.ActiveTarget.Should().NotBeNull("One target is selected");
        controller.ActiveTarget!.Distance.Should().Be(15000, "Should be Target 2");
    }

    [Fact]
    public void Given_NoSelectedTarget_When_CreatingController_Then_ActiveTargetIsNull()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var targets = new List<ITarget>
        {
            CreateTarget("Target 1", isSelected: false),
            CreateTarget("Target 2", isSelected: false)
        };
        var memory = CreateMemoryMeasurement(targets);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.ActiveTarget.Should().BeNull("No target is selected");
    }

    [Fact]
    public void Given_MultipleSelectedTargets_When_CreatingController_Then_SetsFirstSelected()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var targets = new List<ITarget>
        {
            CreateTarget("Target 1", distance: 5000, isSelected: true),
            CreateTarget("Target 2", distance: 10000, isSelected: true),
            CreateTarget("Target 3", distance: 15000, isSelected: false)
        };
        var memory = CreateMemoryMeasurement(targets);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.ActiveTarget.Should().NotBeNull("At least one target is selected");
        controller.ActiveTarget!.Distance.Should().Be(5000, "Should be first selected target");
    }

    #endregion

    #region Count Property Tests

    [Fact]
    public void Given_ThreeTargets_When_AccessingCount_Then_ReturnsThree()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var targets = new List<ITarget>
        {
            CreateTarget("Target 1"),
            CreateTarget("Target 2"),
            CreateTarget("Target 3")
        };
        var memory = CreateMemoryMeasurement(targets);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.Count.Should().Be(3);
    }

    [Fact]
    public void Given_NullList_When_AccessingCount_Then_ReturnsZero()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var memory = Substitute.For<IMemoryMeasurement>();
        memory.Target.Returns((ITarget[]?)null);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.Count.Should().Be(0, "Null list should have count 0");
    }

    [Fact]
    public void Given_EmptyTargetArray_When_AccessingCount_Then_ReturnsZero()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var memory = CreateMemoryMeasurement(new List<ITarget>());

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.Count.Should().Be(0);
    }

    #endregion

    #region Target Conversion Tests

    [Fact]
    public void Given_TargetsInMemory_When_CreatingController_Then_WrapsAsSimpleTargetInfo()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var targets = new List<ITarget>
        {
            CreateTarget("Target 1"),
            CreateTarget("Target 2")
        };
        var memory = CreateMemoryMeasurement(targets);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.List.Should().AllBeAssignableTo<ITarget>("All targets should implement ITarget interface");
        controller.List.Should().HaveCount(2);
    }

    [Fact]
    public void Given_SelectedTargetInMemory_When_CreatingController_Then_WrapsActiveTargetAsSimpleTargetInfo()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var targets = new List<ITarget>
        {
            CreateTarget("Target 1", isSelected: true)
        };
        var memory = CreateMemoryMeasurement(targets);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.ActiveTarget.Should().BeAssignableTo<ITarget>("Active target should implement ITarget interface");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Given_SingleTarget_When_CreatingController_Then_HasOneTarget()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var targets = new List<ITarget>
        {
            CreateTarget("Only Target", isSelected: true)
        };
        var memory = CreateMemoryMeasurement(targets);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.Count.Should().Be(1);
        controller.ActiveTarget.Should().NotBeNull();
        controller.List.Should().HaveCount(1);
    }

    [Fact]
    public void Given_ManyTargets_When_CreatingController_Then_TracksAllTargets()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var targets = new List<ITarget>();
        for (int i = 0; i < 10; i++)
        {
            targets.Add(CreateTarget($"Target {i}", isSelected: i == 5));
        }
        var memory = CreateMemoryMeasurement(targets);

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.Count.Should().Be(10, "Should track all 10 targets");
        controller.ActiveTarget.Should().NotBeNull("Target 5 is selected");
    }

    [Fact]
    public void Given_TargetsWithNullIsSelected_When_CreatingController_Then_TreatsAsNotSelected()
    {
        // Arrange
        var bot = Substitute.For<Bot>();
        var target1 = Substitute.For<ITarget>();
        target1.IsSelected.Returns((bool?)null);
        target1.Distance.Returns(10000);

        var target2 = CreateTarget("Target 2", isSelected: false);

        var memory = CreateMemoryMeasurement(new List<ITarget> { target1, target2 });

        // Act
        var controller = new ActiveTargetsContoller(bot, memory);

        // Assert
        controller.ActiveTarget.Should().BeNull("Null IsSelected should be treated as false");
    }

    #endregion

    #region Helper Methods

    private static IMemoryMeasurement CreateMemoryMeasurement(List<ITarget> targets)
    {
        var memory = Substitute.For<IMemoryMeasurement>();
        memory.Target.Returns(targets.ToArray());
        return memory;
    }

    private static ITarget CreateTarget(string name = "Target", int distance = 10000, bool isSelected = false)
    {
        var target = Substitute.For<ITarget>();
        target.Distance.Returns(distance);
        target.IsSelected.Returns(isSelected);

        // Adjust distance based on selection for easier testing
        if (isSelected)
        {
            target.Distance.Returns(distance == 10000 ? 15000 : distance);
        }

        return target;
    }

    #endregion
}
