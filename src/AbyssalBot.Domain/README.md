# AbyssalBot.Domain

Pure domain logic for Abyssal combat bot. Contains no infrastructure dependencies (no EF, Redis, memory reading, etc.).

## Architecture

This domain layer follows Domain-Driven Design (DDD) principles:
- **Pure business logic** - no infrastructure concerns
- **Rich domain models** - using C# records and modern features
- **Clear interfaces** - for infrastructure dependencies
- **Testable** - all logic can be unit tested without infrastructure

## Structure

### Models (`/Models`)
Pure domain entities and value objects:
- **ShipFitting** - Ship configuration with modules
- **ShipModule** - Individual module characteristics
- **Target** / **PrioritizedTarget** - Combat targets
- **CombatDecision** - Decision types (activate module, lock target, etc.)
- **DroneState** - Drone bay state
- **ShipHitpointsAndEnergy** - Ship health and capacitor
- **CombatContext** - Complete combat situation

### Services (`/Services`)
Domain services implementing business logic:
- **NpcInformationService** - NPC stats, DPS calculations
- **TargetPriorityService** - Target prioritization logic
- **ManeuverDecisionService** - Movement and positioning decisions
- **DroneControlService** - Drone control decisions
- **TankingDecisionService** - Shield/armor management decisions
- **CombatStrategyService** - Main combat coordination

### Interfaces (`/Interfaces`)
Abstractions for infrastructure:
- **IShipStateProvider** - Ship state from memory/sensors
- **ITargetProvider** - Target data from overview
- **ICombatExecutor** - Execute decisions (click modules, etc.)
- **IInventoryProvider** - Inventory/cargo access

### Enums (`/Enums`)
Domain enumerations:
- **ModuleType** - Types of ship modules
- **ShipManeuverType** - Movement types
- **DroneStatus** - Drone states
- **CombatDecisionType** - Decision categories

## Key Domain Concepts

### Combat Decision Making
The domain uses a **decision-based architecture**:
1. Services analyze the current `CombatContext`
2. Generate `CombatDecision` objects
3. Infrastructure layer executes decisions
4. Clear separation: thinking vs. doing

### NPC Knowledge
NPC information is encapsulated in `NpcInformationService`:
- DPS values for all Abyssal NPCs
- Target priority calculation
- Orbit beacon identification
- Threat assessment

### Target Priority
Prioritization logic (`TargetPriorityService`):
1. Critical threats (Anchoring, Firewatcher)
2. Support ships (Renewing, Fieldweaver)
3. EWAR ships (Entangler, Snarecaster)
4. DPS-based priority for others
5. Distance tiebreaker

### Maneuver Strategy
Movement decisions (`ManeuverDecisionService`):
- **High DPS (>200)** → Orbit for defense
- **Low DPS** → Keep at range for damage application
- **MWD usage** → Based on distance and threat level

### Tanking Strategy
Shield management (`TankingDecisionService`):
- **Critical (<150)** → All boosters + overload
- **Low (<600) + High DPS** → All boosters
- **Low (<600) + Low DPS** → Single booster
- **Safe (>800) or Low Cap** → Turn off boosters
- **Hardeners** → Always active

### Drone Control
Drone decisions (`DroneControlService`):
1. Launch if space available
2. Engage on active target
3. Return when no enemies

## Usage Example

```csharp
// Setup services
var npcService = new NpcInformationService();
var priorityService = new TargetPriorityService(npcService);
var maneuverService = new ManeuverDecisionService();
var droneService = new DroneControlService();
var tankingService = new TankingDecisionService();

var combatStrategy = new CombatStrategyService(
    npcService,
    priorityService,
    maneuverService,
    droneService,
    tankingService
);

// Generate decisions (infrastructure provides state)
var decisions = combatStrategy.GenerateCombatDecisions(
    shipStateProvider,
    targetProvider
);

// Execute decisions (infrastructure layer)
foreach (var decision in decisions)
{
    await combatExecutor.ExecuteDecisionAsync(decision);
}
```

## Design Principles

1. **No Infrastructure Dependencies**
   - No memory reading
   - No UI automation
   - No database access
   - Pure logic only

2. **Immutable Models**
   - Using C# `record` types
   - State changes create new instances
   - Thread-safe by design

3. **Modern C# Features**
   - Pattern matching
   - Records
   - Nullable reference types
   - Collection expressions
   - Switch expressions

4. **Testability**
   - All logic is deterministic
   - Mock infrastructure interfaces
   - No hidden dependencies

5. **Single Responsibility**
   - Each service has one clear purpose
   - Models represent concepts, not data structures
   - Clear separation of concerns

## Integration with Infrastructure

Infrastructure layer responsibilities:
1. **Read** game state via memory reading
2. **Map** to domain models
3. **Call** domain services for decisions
4. **Execute** decisions via UI automation
5. **Handle** timing, errors, retries

Domain layer responsibilities:
1. **Know** combat rules and strategies
2. **Calculate** priorities and threats
3. **Decide** what actions to take
4. **Validate** decisions make sense

## Future Extensions

Potential domain expansions:
- Loot value calculation
- Filament selection strategy
- Fleet coordination logic
- Market pricing for filaments
- Fit optimization recommendations
- Learning from combat results
