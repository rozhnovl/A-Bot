# AbyssalBot Domain Tests

Comprehensive unit tests for the AbyssalBot core domain logic.

## Overview

This test project provides extensive test coverage for the bot's critical decision-making components using xUnit, FluentAssertions, and NSubstitute.

## Test Structure

All tests follow the **AAA (Arrange-Act-Assert)** pattern with descriptive **Given_When_Then** naming conventions.

## Test Classes

### 1. NpcInfoProviderTests
Tests for enemy NPC information and priority calculations.

**Coverage:**
- Priority calculation for 15+ different enemy types
- DPS calculation for individual and multiple enemies
- Orbit beacon detection logic
- Edge cases (unknown enemies, zero DPS, null handling)

**Key Scenarios:**
- High-priority targets (Anchoring, Firewatcher, Renewing)
- DPS-based priority calculation (800 - DPS formula)
- Special handling for Guristas, Drifters, and Extraction nodes
- Aggregate DPS calculation from overview

### 2. PriorityManagerTests
Tests for enemy filtering, sorting, and target selection.

**Coverage:**
- Enemy filtering by range, type, and status
- Priority-based sorting with distance as tiebreaker
- Exclusion of non-combatants (Vila Swarmer, Extraction nodes)
- Complex battle scenarios with multiple filters

**Key Scenarios:**
- Filtering enemies beyond targeting range
- Removing extraction nodes and swarmers
- Priority ordering (Anchoring > Firewatcher > Renewing > DPS-based)
- Distance-based sorting for same-priority targets
- Null and edge case handling

### 3. ShipStateTests
Tests for ship state management and module activation decisions.

**Coverage:**
- Tanking module activation based on shield HP and incoming DPS
- Weapon activation based on target range
- Orbit maneuver decisions
- Always-active module management (hardeners)
- Module state toggling logic

**Key Scenarios:**
- Shield booster activation at low HP
- Capacitor-aware tanking decisions
- Weapon activation within/beyond attack range
- Orbit behavior when beyond optimal range
- Module busy state handling

### 4. ShipFitTests
Tests for ship fitting and module organization.

**Coverage:**
- Module retrieval by type (weapons, hardeners, shield boosters, MWD)
- Module organization in high/mid/low slots
- Always-active module identification
- Module state toggling (active/inactive)

**Key Scenarios:**
- Getting all modules of a specific type
- Finding first module of type
- Handling fits with missing module types
- Module activation/deactivation tasks
- Null state handling for modules

### 5. ActiveTargetsControllerTests
Tests for active target management.

**Coverage:**
- Target list initialization from memory
- Active target identification
- Target count tracking
- Null and empty state handling

**Key Scenarios:**
- Multiple targets with one selected
- No selected targets
- Empty target list
- Multiple selected targets (first wins)
- Null safety

## Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test class
dotnet test --filter "FullyQualifiedName~NpcInfoProviderTests"

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## Code Coverage

The test suite provides comprehensive coverage of core domain logic:

- **NpcInfoProvider**: ~95% coverage (all priority and DPS calculation paths)
- **PriorityManager**: ~90% coverage (all filtering and sorting logic)
- **ShipState**: ~75% coverage (core decision logic; some paths require complex setup)
- **ShipFit**: ~85% coverage (module management and retrieval)
- **ActiveTargetsController**: ~90% coverage (target tracking logic)

**Overall estimated coverage**: ~85% for tested domain components

## Test Patterns

### Mocking with NSubstitute
```csharp
var bot = Substitute.For<Bot>();
var memory = Substitute.For<IMemoryMeasurement>();
memory.Target.Returns(targetArray);
```

### Fluent Assertions
```csharp
result.Should().NotBeNull("explanation");
result.Should().HaveCount(3, "should have 3 targets");
modules.Should().AllSatisfy(m => m.Type.Should().Be(ModuleType.Hardener));
```

### Given-When-Then Naming
```csharp
[Fact]
public void Given_LowShieldAndHighDps_When_GettingTankingTask_Then_ActivatesAllShieldBoosters()
{
    // Arrange (Given)
    // Act (When)
    // Assert (Then)
}
```

## Dependencies

- **xUnit**: Test framework
- **FluentAssertions**: Expressive assertion library
- **NSubstitute**: Mocking framework
- **coverlet.collector**: Code coverage collection

## Future Enhancements

- Integration tests for state machine transitions
- Performance benchmarks for priority calculations
- Property-based testing for edge cases
- Tests for remaining bot tasks (LootTask, UndockTask, etc.)
- End-to-end scenario tests
