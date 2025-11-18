using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services;
using AbyssalBot.Tactics.Tests.Builders;

namespace AbyssalBot.Tactics.Tests.Services;

public class DroneControlServiceTests
{
    private readonly DroneControlService _sut;

    public DroneControlServiceTests()
    {
        _sut = new DroneControlService();
    }

    public class DecideLaunchDrones : DroneControlServiceTests
    {
        [Fact]
        public void Should_LaunchDrones_When_DronesInBayAndSpaceAvailable()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInBay(5)
                .WithDronesInSpace(0)
                .WithMaxDrones(5)
                .Build();

            // Act
            var decision = _sut.DecideLaunchDrones(droneState);

            // Assert
            decision.Should().NotBeNull();
            decision.Should().BeOfType<LaunchDronesDecision>();
        }

        [Fact]
        public void Should_NotLaunchDrones_When_AllDronesInSpace()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInBay(0)
                .WithDronesInSpace(5)
                .WithMaxDrones(5)
                .Build();

            // Act
            var decision = _sut.DecideLaunchDrones(droneState);

            // Assert
            decision.Should().BeNull();
        }

        [Fact]
        public void Should_NotLaunchDrones_When_NoDronesInBay()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInBay(0)
                .WithDronesInSpace(3)
                .WithMaxDrones(5)
                .Build();

            // Act
            var decision = _sut.DecideLaunchDrones(droneState);

            // Assert
            decision.Should().BeNull();
        }

        [Fact]
        public void Should_LaunchDrones_When_PartiallyDeployed()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInBay(2)
                .WithDronesInSpace(3)
                .WithMaxDrones(5)
                .Build();

            // Act
            var decision = _sut.DecideLaunchDrones(droneState);

            // Assert
            decision.Should().NotBeNull();
        }
    }

    public class DecideEngageDrones : DroneControlServiceTests
    {
        [Fact]
        public void Should_EngageDrones_When_IdleDronesAndValidTarget()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithIdleDrones(5)
                .Build();

            var target = new TargetBuilder()
                .AsEnemy()
                .AsTargeted()
                .WithDistance(10000)
                .Build();

            // Act
            var decision = _sut.DecideEngageDrones(droneState, target);

            // Assert
            decision.Should().NotBeNull();
            decision.Should().BeOfType<EngageDronesDecision>();
            decision!.Target.Should().Be(target);
        }

        [Fact]
        public void Should_NotEngageDrones_When_NoIdleDrones()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithEngagingDrones(5)
                .Build();

            var target = new TargetBuilder()
                .AsEnemy()
                .AsTargeted()
                .Build();

            // Act
            var decision = _sut.DecideEngageDrones(droneState, target);

            // Assert
            decision.Should().BeNull();
        }

        [Fact]
        public void Should_NotEngageDrones_When_TargetNotTargeted()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithIdleDrones(5)
                .Build();

            var target = new TargetBuilder()
                .AsEnemy()
                .Build();

            // Act
            var decision = _sut.DecideEngageDrones(droneState, target);

            // Assert
            decision.Should().BeNull();
        }

        [Fact]
        public void Should_NotEngageDrones_When_TargetTooFar()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithIdleDrones(5)
                .Build();

            var target = new TargetBuilder()
                .AsEnemy()
                .AsTargeted()
                .WithDistance(60000) // Beyond max drone range
                .Build();

            // Act
            var decision = _sut.DecideEngageDrones(droneState, target);

            // Assert
            decision.Should().BeNull();
        }

        [Fact]
        public void Should_NotEngageDrones_When_AlreadyAssigned()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithIdleDrones(5)
                .Build();

            var target = new TargetBuilder()
                .AsEnemy()
                .AsTargeted()
                .WithDroneAssigned()
                .Build();

            // Act
            var decision = _sut.DecideEngageDrones(droneState, target);

            // Assert
            decision.Should().BeNull();
        }

        [Theory]
        [InlineData(1000, true)]
        [InlineData(10000, true)]
        [InlineData(30000, true)]
        [InlineData(54999, true)]
        [InlineData(55001, false)]
        [InlineData(100000, false)]
        public void Should_RespectMaxDroneRange(int distance, bool shouldEngage)
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithIdleDrones(5)
                .Build();

            var target = new TargetBuilder()
                .AsEnemy()
                .AsTargeted()
                .WithDistance(distance)
                .Build();

            // Act
            var decision = _sut.DecideEngageDrones(droneState, target);

            // Assert
            if (shouldEngage)
            {
                decision.Should().NotBeNull();
            }
            else
            {
                decision.Should().BeNull();
            }
        }
    }

    public class DecideReturnDrones : DroneControlServiceTests
    {
        [Fact]
        public void Should_ReturnDrones_When_NoEnemiesRemaining()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithEngagingDrones(5)
                .Build();

            // Act
            var decision = _sut.DecideReturnDrones(droneState, noEnemiesRemaining: true);

            // Assert
            decision.Should().NotBeNull();
            decision.Should().BeOfType<ReturnDronesDecision>();
        }

        [Fact]
        public void Should_NotReturnDrones_When_EnemiesStillPresent()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithEngagingDrones(5)
                .Build();

            // Act
            var decision = _sut.DecideReturnDrones(droneState, noEnemiesRemaining: false);

            // Assert
            decision.Should().BeNull();
        }

        [Fact]
        public void Should_NotReturnDrones_When_AlreadyReturning()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithReturningDrones(5)
                .Build();

            // Act
            var decision = _sut.DecideReturnDrones(droneState, noEnemiesRemaining: true);

            // Assert
            decision.Should().BeNull();
        }

        [Fact]
        public void Should_NotReturnDrones_When_NoDronesInSpace()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(0)
                .WithDronesInBay(5)
                .Build();

            // Act
            var decision = _sut.DecideReturnDrones(droneState, noEnemiesRemaining: true);

            // Assert
            decision.Should().BeNull();
        }
    }

    public class GetDroneDecisions : DroneControlServiceTests
    {
        [Fact]
        public void Should_ReturnDrones_When_NoEnemiesRemaining()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithEngagingDrones(5)
                .Build();

            // Act
            var decisions = _sut.GetDroneDecisions(droneState, null, noEnemiesRemaining: true).ToList();

            // Assert
            decisions.Should().HaveCount(1);
            decisions[0].Should().BeOfType<ReturnDronesDecision>();
        }

        [Fact]
        public void Should_LaunchAndEngageDrones_When_EnemiesPresent()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInBay(5)
                .WithDronesInSpace(0)
                .WithMaxDrones(5)
                .Build();

            var target = new TargetBuilder()
                .AsEnemy()
                .AsTargeted()
                .Build();

            // Act
            var decisions = _sut.GetDroneDecisions(droneState, target, noEnemiesRemaining: false).ToList();

            // Assert
            decisions.Should().Contain(d => d is LaunchDronesDecision);
        }

        [Fact]
        public void Should_EngageIdleDrones_When_TargetAvailable()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithIdleDrones(5)
                .Build();

            var target = new TargetBuilder()
                .AsEnemy()
                .AsTargeted()
                .Build();

            // Act
            var decisions = _sut.GetDroneDecisions(droneState, target, noEnemiesRemaining: false).ToList();

            // Assert
            decisions.Should().Contain(d => d is EngageDronesDecision);
        }

        [Fact]
        public void Should_NotGenerateDecisions_When_AllDronesEngaged()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInBay(0)
                .WithDronesInSpace(5)
                .WithEngagingDrones(5)
                .Build();

            var target = new TargetBuilder()
                .AsEnemy()
                .AsTargeted()
                .WithDroneAssigned()
                .Build();

            // Act
            var decisions = _sut.GetDroneDecisions(droneState, target, noEnemiesRemaining: false).ToList();

            // Assert
            decisions.Should().BeEmpty();
        }

        [Fact]
        public void Should_HandleNoActiveTarget()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithIdleDrones(5)
                .Build();

            // Act
            var decisions = _sut.GetDroneDecisions(droneState, null, noEnemiesRemaining: false).ToList();

            // Assert
            decisions.Should().NotContain(d => d is EngageDronesDecision);
        }
    }

    public class WeaponManagementScenarios : DroneControlServiceTests
    {
        [Fact]
        public void Should_CoordinateDronesWithWeapons_InCombat()
        {
            // Arrange - Drones deployed and engaging
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithEngagingDrones(3)
                .WithIdleDrones(2)
                .Build();

            var target = new TargetBuilder()
                .AsEnemy()
                .AsTargeted()
                .Build();

            // Act
            var decisions = _sut.GetDroneDecisions(droneState, target, noEnemiesRemaining: false).ToList();

            // Assert
            decisions.Should().Contain(d => d is EngageDronesDecision,
                "idle drones should engage target");
        }

        [Fact]
        public void Should_RecallDrones_WhenCombatComplete()
        {
            // Arrange
            var droneState = new DroneStateBuilder()
                .WithDronesInSpace(5)
                .WithEngagingDrones(5)
                .Build();

            // Act
            var decisions = _sut.GetDroneDecisions(droneState, null, noEnemiesRemaining: true).ToList();

            // Assert
            decisions.Should().HaveCount(1);
            decisions[0].Should().BeOfType<ReturnDronesDecision>();
        }
    }
}
