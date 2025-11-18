# Situation Handlers Quick Reference

## Handler Summary

| Handler | Priority | Trigger | Primary Action |
|---------|----------|---------|----------------|
| **Critical Damage** | 50 | Hull < 50%, Shield < 20%, Armor < 30% | Overheat all tank modules |
| **Capacitor Emergency** | 40 | Cap < 10% | Disable MWD, kite away, disable weapons if < 5% |
| **EWAR** | 30 | Jammed, Neuted, Webbed, etc. | Counter-tactics based on EWAR type |
| **Multiple Hostiles** | 20 | > 10 enemies | Defensive orbit, overheat if high DPS |
| **Low Capacitor** | 10 | Cap < 30% | Reduce module usage, conserve cap |

## File Locations

### Handlers
```
/src/AbyssalBot.Domain/Services/Situations/
├── ILowCapacitorHandler.cs
├── LowCapacitorHandler.cs
├── ICriticalDamageHandler.cs
├── CriticalDamageHandler.cs
├── IEwarSituationHandler.cs
├── EwarSituationHandler.cs
├── IMultipleHostilesHandler.cs
├── MultipleHostilesHandler.cs
├── IReloadTimingHandler.cs
├── ReloadTimingHandler.cs
├── ILootPrioritizationHandler.cs
├── LootPrioritizationHandler.cs
├── IAbyssRoomStrategyHandler.cs
├── AbyssRoomStrategyHandler.cs
├── IAmmoSelectionHandler.cs
├── AmmoSelectionHandler.cs
└── SituationCoordinator.cs
```

### Models & Enums
```
/src/AbyssalBot.Domain/Enums/
├── SituationPriority.cs
└── EwarType.cs

/src/AbyssalBot.Domain/Models/
└── SituationResponse.cs
```

### Tests
```
/tests/AbyssalBot.Domain.Tests/Situations/
├── LowCapacitorHandlerTests.cs (6 tests)
├── CriticalDamageHandlerTests.cs (7 tests)
├── EwarSituationHandlerTests.cs (8 tests)
├── MultipleHostilesHandlerTests.cs (7 tests)
├── SituationCoordinatorTests.cs (9 tests)
└── SituationIntegrationTests.cs (12 tests)
```

## Priority Tree

```
CriticalDamage (50)      ◄─── Structure/Shield/Armor critical
    ↓
CapacitorEmergency (40)  ◄─── Cap < 10%
    ↓
Ewar (30)                ◄─── Electronic warfare active
    ↓
MultipleHostiles (20)    ◄─── 10+ enemies engaging
    ↓
LowCapacitor (10)        ◄─── Cap < 30%
    ↓
Normal (0)               ◄─── All clear
```

## Quick Usage

```csharp
// 1. Initialize coordinator
var coordinator = new SituationCoordinator(
    new CriticalDamageHandler(),
    new LowCapacitorHandler(),
    new EwarSituationHandler(),
    new MultipleHostilesHandler()
);

// 2. Get situation response
var response = coordinator.HandleSituation(context, ewarContext, beacon);

// 3. Execute decisions
foreach (var decision in response.Decisions)
{
    ExecuteDecision(decision);
}

// 4. Check emergency
if (coordinator.IsEmergencySituation(context))
{
    // Alert player, emergency procedures
}
```

## Room Strategies

| Room Type | Strategy | Priority Targets |
|-----------|----------|------------------|
| **Triglavian** | Kill logi first | Renewing Logi → Damavik → Vedmak |
| **Drone** | AOE/closest | Closest drones first |
| **Sleeper** | DPS reduction | Close frigates → Rest |
| **EWAR** | Disable EWAR | Neutralizers → Jammers → Rest |
| **Cache** | Clear & loot | Defenders → Loot cache → Gate |

## Loot Priority

1. **Mutaplasmids** (highest value)
2. **Filaments** (site access)
3. **High-value** (> 10M ISK)
4. **Value-dense** (> 100k ISK/m³)
5. **Fill remaining**

Skip if cargo > 80% full with > 50M ISK value

## Test Coverage

- **Total Tests**: 49 test cases
- **Unit Tests**: 37 tests
- **Integration Tests**: 12 scenarios
- **Coverage**: All handlers, coordinator, complex multi-situation scenarios

## Integration Points

1. **CombatStrategyService**: Call coordinator.HandleSituation() in main loop
2. **Bot State Machine**: Use IsEmergencySituation() for state transitions
3. **Telemetry**: Log response.Situation and response.Reasoning
4. **Decision Execution**: Execute response.Decisions via CombatExecutor
5. **Room Navigation**: Use AbyssRoomStrategyHandler for room tactics
6. **Looting**: Use LootPrioritizationHandler during loot phase

## Performance Notes

- All handlers are stateless (except ReloadTimingHandler)
- O(n) complexity where n = targets/modules
- No async operations required
- Minimal memory allocations
- Thread-safe (no shared mutable state)
