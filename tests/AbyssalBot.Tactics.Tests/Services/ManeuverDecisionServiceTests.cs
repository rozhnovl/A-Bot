using AbyssalBot.Domain.Enums;
using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services;
using AbyssalBot.Tactics.Tests.Builders;

namespace AbyssalBot.Tactics.Tests.Services;

public class ManeuverDecisionServiceTests
{
    private readonly ManeuverDecisionService _sut;

    public ManeuverDecisionServiceTests()
    {
        _sut = new ManeuverDecisionService();
    }

    public class DecideManeuver : ManeuverDecisionServiceTests
    {
        [Fact]
        public void Should_OrbitBeacon_When_HighDps()
        {
            // Arrange
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .WithDistance(5000)
                .Build();

            // Act
            var decision = _sut.DecideManeuver(
                ShipManeuverType.None,
                250, // High DPS
                orbitBeacon,
                5000
            );

            // Assert
            decision.Should().NotBeNull();
            decision!.ManeuverType.Should().Be(ShipManeuverType.Orbit);
            decision.Target.Should().Be(orbitBeacon);
            decision.Distance.Should().Be(5000);
        }

        [Fact]
        public void Should_KeepAtRange_When_LowDps()
        {
            // Arrange
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .WithDistance(5000)
                .Build();

            // Act
            var decision = _sut.DecideManeuver(
                ShipManeuverType.None,
                50, // Low DPS
                orbitBeacon,
                5000
            );

            // Assert
            decision.Should().NotBeNull();
            decision!.ManeuverType.Should().Be(ShipManeuverType.KeepAtRange);
            decision.Target.Should().Be(orbitBeacon);
            decision.Distance.Should().Be(500);
        }

        [Fact]
        public void Should_ReturnNull_When_AlreadyOrbiting()
        {
            // Arrange
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .Build();

            // Act
            var decision = _sut.DecideManeuver(
                ShipManeuverType.Orbit,
                250,
                orbitBeacon,
                5000
            );

            // Assert
            decision.Should().BeNull();
        }

        [Fact]
        public void Should_ReturnNull_When_AlreadyKeepingAtRange()
        {
            // Arrange
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .Build();

            // Act
            var decision = _sut.DecideManeuver(
                ShipManeuverType.KeepAtRange,
                50,
                orbitBeacon,
                500
            );

            // Assert
            decision.Should().BeNull();
        }

        [Fact]
        public void Should_ReturnNull_When_NoBeaconAvailable()
        {
            // Act
            var decision = _sut.DecideManeuver(
                ShipManeuverType.None,
                250,
                null,
                0
            );

            // Assert
            decision.Should().BeNull();
        }

        [Theory]
        [InlineData(ShipManeuverType.None, 250, true)]
        [InlineData(ShipManeuverType.Stopped, 250, true)]
        [InlineData(ShipManeuverType.None, 50, true)]
        [InlineData(ShipManeuverType.Orbit, 250, false)]
        [InlineData(ShipManeuverType.KeepAtRange, 50, false)]
        [InlineData(ShipManeuverType.Approach, 50, false)]
        public void Should_DecideCorrectly_ForVariousManeuverStates(
            ShipManeuverType currentManeuver,
            double incomingDps,
            bool shouldChangeManeuver)
        {
            // Arrange
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .Build();

            // Act
            var decision = _sut.DecideManeuver(
                currentManeuver,
                incomingDps,
                orbitBeacon,
                5000
            );

            // Assert
            if (shouldChangeManeuver)
            {
                decision.Should().NotBeNull();
            }
            else
            {
                decision.Should().BeNull();
            }
        }
    }

    public class ShouldActivateMWD : ManeuverDecisionServiceTests
    {
        [Fact]
        public void Should_ActivateMWD_When_FarFromTarget()
        {
            // Act
            var shouldActivate = _sut.ShouldActivateMWD(
                50, // Low DPS
                10000, // Far distance
                ShipManeuverType.Approach
            );

            // Assert
            shouldActivate.Should().BeTrue();
        }

        [Fact]
        public void Should_DeactivateMWD_When_CloseToTarget()
        {
            // Act
            var shouldActivate = _sut.ShouldActivateMWD(
                50,
                1500, // Close distance
                ShipManeuverType.Approach
            );

            // Assert
            shouldActivate.Should().BeFalse();
        }

        [Fact]
        public void Should_DeactivateMWD_When_HighDpsAndOrbiting()
        {
            // Act
            var shouldActivate = _sut.ShouldActivateMWD(
                250, // High DPS
                5000,
                ShipManeuverType.Orbit
            );

            // Assert
            shouldActivate.Should().BeFalse();
        }

        [Theory]
        [InlineData(50, 1000, ShipManeuverType.Approach, false)]
        [InlineData(50, 2500, ShipManeuverType.Approach, true)]
        [InlineData(50, 5000, ShipManeuverType.Approach, true)]
        [InlineData(250, 5000, ShipManeuverType.Orbit, false)]
        [InlineData(50, 5000, ShipManeuverType.KeepAtRange, true)]
        public void Should_DecideCorrectly_ForVariousScenarios(
            double incomingDps,
            int distance,
            ShipManeuverType maneuver,
            bool expectedResult)
        {
            // Act
            var shouldActivate = _sut.ShouldActivateMWD(incomingDps, distance, maneuver);

            // Assert
            shouldActivate.Should().Be(expectedResult);
        }
    }

    public class SelectOrbitBeacon : ManeuverDecisionServiceTests
    {
        [Fact]
        public void Should_PreferOrbitBeaconNpc_OverCache()
        {
            // Arrange
            var npcService = new NpcInformationService();
            var orbitBeaconNpc = new TargetBuilder()
                .WithName("Ephemeral Cloudbreak")
                .WithType("Cloudbreak")
                .AsEnemy()
                .Build();
            var coreCache = new TargetBuilder()
                .AsCoreCache()
                .Build();

            var targets = new[] { orbitBeaconNpc, coreCache };

            // Act
            var beacon = _sut.SelectOrbitBeacon(targets, coreCache, null, npcService);

            // Assert
            beacon.Should().Be(orbitBeaconNpc);
        }

        [Fact]
        public void Should_UseCoreCache_When_NoOrbitBeaconNpc()
        {
            // Arrange
            var npcService = new NpcInformationService();
            var coreCache = new TargetBuilder()
                .AsCoreCache()
                .Build();
            var enemy = new TargetBuilder()
                .WithName("Random Enemy")
                .AsEnemy()
                .Build();

            var targets = new[] { enemy };

            // Act
            var beacon = _sut.SelectOrbitBeacon(targets, coreCache, null, npcService);

            // Assert
            beacon.Should().Be(coreCache);
        }

        [Fact]
        public void Should_UseConduit_When_NoCacheOrBeacon()
        {
            // Arrange
            var npcService = new NpcInformationService();
            var conduit = new TargetBuilder()
                .AsConduit()
                .Build();
            var enemy = new TargetBuilder()
                .WithName("Random Enemy")
                .AsEnemy()
                .Build();

            var targets = new[] { enemy };

            // Act
            var beacon = _sut.SelectOrbitBeacon(targets, null, conduit, npcService);

            // Assert
            beacon.Should().Be(conduit);
        }

        [Fact]
        public void Should_ReturnNull_When_NoBeaconOptions()
        {
            // Arrange
            var npcService = new NpcInformationService();
            var enemy = new TargetBuilder()
                .WithName("Random Enemy")
                .AsEnemy()
                .Build();

            var targets = new[] { enemy };

            // Act
            var beacon = _sut.SelectOrbitBeacon(targets, null, null, npcService);

            // Assert
            beacon.Should().BeNull();
        }
    }

    public class ManeuverOptimizationScenarios : ManeuverDecisionServiceTests
    {
        [Fact]
        public void Should_OptimizeForDefense_When_TakingHeavyDamage()
        {
            // Arrange - Ship taking 300+ DPS
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .WithDistance(8000)
                .Build();

            // Act
            var decision = _sut.DecideManeuver(
                ShipManeuverType.None,
                300,
                orbitBeacon,
                8000
            );

            // Assert
            decision.Should().NotBeNull();
            decision!.ManeuverType.Should().Be(ShipManeuverType.Orbit,
                "orbiting reduces incoming damage through angular velocity");
        }

        [Fact]
        public void Should_OptimizeForDamageApplication_When_LowThreat()
        {
            // Arrange - Ship taking minimal DPS
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .WithDistance(1000)
                .Build();

            // Act
            var decision = _sut.DecideManeuver(
                ShipManeuverType.None,
                25,
                orbitBeacon,
                1000
            );

            // Assert
            decision.Should().NotBeNull();
            decision!.ManeuverType.Should().Be(ShipManeuverType.KeepAtRange,
                "keeping at range maintains stable position for better damage application");
        }

        [Theory]
        [InlineData(500, ShipManeuverType.Orbit)]  // Very high DPS
        [InlineData(300, ShipManeuverType.Orbit)]  // High DPS
        [InlineData(150, ShipManeuverType.Orbit)]  // Medium-high DPS
        [InlineData(100, ShipManeuverType.KeepAtRange)]  // Medium-low DPS
        [InlineData(50, ShipManeuverType.KeepAtRange)]   // Low DPS
        public void Should_SelectCorrectManeuver_BasedOnDpsThreshold(
            double incomingDps,
            ShipManeuverType expectedManeuver)
        {
            // Arrange
            var orbitBeacon = new TargetBuilder()
                .AsCoreCache()
                .Build();

            // Act
            var decision = _sut.DecideManeuver(
                ShipManeuverType.None,
                incomingDps,
                orbitBeacon,
                5000
            );

            // Assert
            decision.Should().NotBeNull();
            decision!.ManeuverType.Should().Be(expectedManeuver);
        }
    }
}
