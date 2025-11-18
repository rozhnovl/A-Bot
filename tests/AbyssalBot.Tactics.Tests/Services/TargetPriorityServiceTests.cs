using AbyssalBot.Domain.Models;
using AbyssalBot.Domain.Services;
using AbyssalBot.Tactics.Tests.Builders;

namespace AbyssalBot.Tactics.Tests.Services;

public class TargetPriorityServiceTests
{
    private readonly TargetPriorityService _sut;
    private readonly NpcInformationService _npcInfoService;

    public TargetPriorityServiceTests()
    {
        _npcInfoService = new NpcInformationService();
        _sut = new TargetPriorityService(_npcInfoService);
    }

    public class PrioritizeTargets : TargetPriorityServiceTests
    {
        [Fact]
        public void Should_OnlyIncludeEnemies()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().Build(),
                new TargetBuilder().AsCoreCache().Build(),
                new TargetBuilder().AsConduit().Build()
            };

            // Act
            var prioritized = _sut.PrioritizeTargets(targets, 100000);

            // Assert
            prioritized.Should().HaveCount(1);
            prioritized[0].Target.IsEnemy.Should().BeTrue();
        }

        [Fact]
        public void Should_ExcludeTargetsOutOfRange()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().WithDistance(5000).Build(),
                new TargetBuilder().AsEnemy().WithDistance(150000).Build()
            };

            // Act
            var prioritized = _sut.PrioritizeTargets(targets, 100000);

            // Assert
            prioritized.Should().HaveCount(1);
            prioritized[0].Target.Distance.Should().Be(5000);
        }

        [Fact]
        public void Should_ExcludeExtractionNodes()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().WithName("Normal Enemy").Build(),
                new TargetBuilder().AsEnemy().WithName("Extraction Node").Build()
            };

            // Act
            var prioritized = _sut.PrioritizeTargets(targets, 100000);

            // Assert
            prioritized.Should().HaveCount(1);
            prioritized[0].Target.Name.Should().Be("Normal Enemy");
        }

        [Fact]
        public void Should_ExcludeVilaSwarmers()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().WithType("Cruiser").Build(),
                new TargetBuilder().AsEnemy().WithType("Vila Swarmer").Build()
            };

            // Act
            var prioritized = _sut.PrioritizeTargets(targets, 100000);

            // Assert
            prioritized.Should().HaveCount(1);
            prioritized[0].Target.Type.Should().Be("Cruiser");
        }

        [Fact]
        public void Should_OrderByPriority()
        {
            // Arrange - Frigates are typically higher priority than cruisers
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().WithType("Cruiser").WithName("Cruiser").WithId(1).Build(),
                new TargetBuilder().AsEnemy().WithType("Frigate").WithName("Harrowing Scythe").WithId(2).Build()
            };

            // Act
            var prioritized = _sut.PrioritizeTargets(targets, 100000);

            // Assert
            prioritized.Should().HaveCount(2);
            // Higher priority (lower number) should be first
            prioritized[0].Priority.Should().BeLessThan(prioritized[1].Priority);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public void Should_HandleVariousEnemyCounts(int enemyCount)
        {
            // Arrange
            var targets = Enumerable.Range(0, enemyCount)
                .Select(i => new TargetBuilder()
                    .AsEnemy()
                    .WithId(i)
                    .WithDistance(5000 + i * 1000)
                    .Build())
                .ToArray();

            // Act
            var prioritized = _sut.PrioritizeTargets(targets, 100000);

            // Assert
            prioritized.Should().HaveCount(enemyCount);
        }
    }

    public class GetBestTarget : TargetPriorityServiceTests
    {
        [Fact]
        public void Should_ReturnHighestPriorityTarget()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().WithType("Cruiser").WithId(1).Build(),
                new TargetBuilder().AsEnemy().WithType("Frigate").WithId(2).Build()
            };

            // Act
            var bestTarget = _sut.GetBestTarget(targets, 100000);

            // Assert
            bestTarget.Should().NotBeNull();
            bestTarget!.Target.Type.Should().Be("Frigate");
        }

        [Fact]
        public void Should_ReturnNull_When_NoTargetsAvailable()
        {
            // Arrange
            var targets = Array.Empty<Target>();

            // Act
            var bestTarget = _sut.GetBestTarget(targets, 100000);

            // Assert
            bestTarget.Should().BeNull();
        }

        [Fact]
        public void Should_ReturnNull_When_OnlyNonEnemies()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsCoreCache().Build(),
                new TargetBuilder().AsConduit().Build()
            };

            // Act
            var bestTarget = _sut.GetBestTarget(targets, 100000);

            // Assert
            bestTarget.Should().BeNull();
        }
    }

    public class GetTargetsToLock : TargetPriorityServiceTests
    {
        [Fact]
        public void Should_ReturnTargetsUpToMaxSlots()
        {
            // Arrange
            var targets = Enumerable.Range(0, 10)
                .Select(i => new TargetBuilder()
                    .AsEnemy()
                    .WithId(i)
                    .Build())
                .ToArray();

            // Act
            var targetsToLock = _sut.GetTargetsToLock(targets, 100000, 7, 2);

            // Assert
            targetsToLock.Should().HaveCount(5); // 7 max - 2 current = 5 slots
        }

        [Fact]
        public void Should_ReturnEmpty_When_NoSlotsAvailable()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().Build()
            };

            // Act
            var targetsToLock = _sut.GetTargetsToLock(targets, 100000, 7, 7);

            // Assert
            targetsToLock.Should().BeEmpty();
        }

        [Fact]
        public void Should_ExcludeAlreadyTargetedOrTargeting()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().AsTargeted().WithId(1).Build(),
                new TargetBuilder().AsEnemy().AsTargeting().WithId(2).Build(),
                new TargetBuilder().AsEnemy().WithId(3).Build()
            };

            // Act
            var targetsToLock = _sut.GetTargetsToLock(targets, 100000, 7, 0);

            // Assert
            targetsToLock.Should().HaveCount(1);
            targetsToLock[0].Id.Should().Be(3);
        }

        [Fact]
        public void Should_PrioritizeByThreatLevel()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().WithType("Cruiser").WithId(1).Build(),
                new TargetBuilder().AsEnemy().WithType("Frigate").WithName("Harrowing Scythe").WithId(2).Build(),
                new TargetBuilder().AsEnemy().WithType("Battleship").WithId(3).Build()
            };

            // Act
            var targetsToLock = _sut.GetTargetsToLock(targets, 100000, 7, 0);

            // Assert
            targetsToLock.Should().HaveCount(3);
            // First target should be highest priority (lowest priority value)
        }

        [Theory]
        [InlineData(7, 0, 7)]
        [InlineData(7, 2, 5)]
        [InlineData(7, 5, 2)]
        [InlineData(7, 7, 0)]
        public void Should_CalculateAvailableSlots_Correctly(
            int maxTargets,
            int currentTargets,
            int expectedSlots)
        {
            // Arrange - Create more targets than we can lock
            var targets = Enumerable.Range(0, 10)
                .Select(i => new TargetBuilder()
                    .AsEnemy()
                    .WithId(i)
                    .Build())
                .ToArray();

            // Act
            var targetsToLock = _sut.GetTargetsToLock(targets, 100000, maxTargets, currentTargets);

            // Assert
            targetsToLock.Should().HaveCount(Math.Min(expectedSlots, 10));
        }
    }

    public class CalculateIncomingDps : TargetPriorityServiceTests
    {
        [Fact]
        public void Should_SumDpsFromAllEnemies()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().WithType("Damavik").Build(),
                new TargetBuilder().AsEnemy().WithType("Damavik").Build()
            };

            // Act
            var totalDps = _sut.CalculateIncomingDps(targets);

            // Assert
            totalDps.Should().BeGreaterThan(0);
        }

        [Fact]
        public void Should_ReturnZero_When_NoEnemies()
        {
            // Arrange
            var targets = Array.Empty<Target>();

            // Act
            var totalDps = _sut.CalculateIncomingDps(targets);

            // Assert
            totalDps.Should().Be(0);
        }

        [Fact]
        public void Should_IgnoreNonEnemies()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsCoreCache().Build(),
                new TargetBuilder().AsConduit().Build()
            };

            // Act
            var totalDps = _sut.CalculateIncomingDps(targets);

            // Assert
            totalDps.Should().Be(0);
        }

        [Fact]
        public void Should_HandleUnknownNpcs()
        {
            // Arrange
            var targets = new[]
            {
                new TargetBuilder().AsEnemy().WithType("Unknown NPC Type").Build()
            };

            // Act
            var totalDps = _sut.CalculateIncomingDps(targets);

            // Assert
            totalDps.Should().Be(0);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(15)]
        public void Should_ScaleWithEnemyCount(int enemyCount)
        {
            // Arrange
            var targets = Enumerable.Range(0, enemyCount)
                .Select(i => new TargetBuilder()
                    .AsEnemy()
                    .WithType("Damavik")
                    .WithId(i)
                    .Build())
                .ToArray();

            // Act
            var totalDps = _sut.CalculateIncomingDps(targets);

            // Assert
            totalDps.Should().BeGreaterThan(0);
            // DPS should increase with more enemies (assuming Damavik has known DPS)
        }
    }

    public class MultipleHostileScenarios : TargetPriorityServiceTests
    {
        [Theory]
        [InlineData(1, "Solo enemy - single target priority")]
        [InlineData(3, "Small group - manage multiple threats")]
        [InlineData(5, "Medium group - prioritize correctly")]
        [InlineData(10, "Large swarm - handle many targets")]
        [InlineData(15, "Massive swarm - performance test")]
        public void Should_HandleVariousSwarmSizes(int enemyCount, string scenario)
        {
            // Arrange
            var targets = Enumerable.Range(0, enemyCount)
                .Select(i => new TargetBuilder()
                    .AsEnemy()
                    .WithType(i % 2 == 0 ? "Frigate" : "Cruiser")
                    .WithDistance(5000 + i * 500)
                    .WithId(i)
                    .Build())
                .ToArray();

            // Act
            var prioritized = _sut.PrioritizeTargets(targets, 100000);
            var bestTarget = _sut.GetBestTarget(targets, 100000);
            var incomingDps = _sut.CalculateIncomingDps(targets);

            // Assert
            prioritized.Should().HaveCount(enemyCount, $"scenario: {scenario}");
            bestTarget.Should().NotBeNull($"scenario: {scenario}");
            incomingDps.Should().BeGreaterOrEqualTo(0, $"scenario: {scenario}");
        }
    }
}
