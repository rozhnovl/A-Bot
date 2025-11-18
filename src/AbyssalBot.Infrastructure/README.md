# AbyssalBot.Infrastructure

This project contains all infrastructure implementations for the AbyssalBot, following the **Hexagonal Architecture** (Ports and Adapters) pattern. The infrastructure layer is responsible for integrating with external systems and libraries while keeping the domain layer pure and dependency-free.

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                         Application Layer                       │
│                    (AbyssalBot.Application)                     │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ Uses
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Domain Layer                            │
│                     (AbyssalBot.Domain)                         │
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │            Interfaces/Infrastructure/                    │  │
│  │  - IMemoryReader, IGameStateReader, IShipStateReader    │  │
│  │  - IInputSimulator, IMouseController, IMotionExecutor   │  │
│  │  - IGameStateCache, ICacheProvider, ITemporalCache      │  │
│  │  - IAbyssDataRepository, IConfigurationRepository       │  │
│  │  - IBotLogger, ITelemetryCollector, IActionRecorder     │  │
│  └──────────────────────────────────────────────────────────┘  │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ Implemented by
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                         │
│                  (AbyssalBot.Infrastructure)                    │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐         │
│  │   Memory/    │  │    Input/    │  │   Caching/   │         │
│  │  Sanderling  │  │ WindowMotor  │  │ Redis/Memory │         │
│  └──────────────┘  └──────────────┘  └──────────────┘         │
│                                                                 │
│  ┌──────────────┐  ┌──────────────┐                           │
│  │  Database/   │  │   Logging/   │                           │
│  │   EF Core    │  │   Serilog    │                           │
│  └──────────────┘  └──────────────┘                           │
└────────────────────────┬────────────────────────────────────────┘
                         │
                         │ Depends on
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│                    External Libraries                           │
│  Sanderling, WindowMotor, Redis, EF Core, Serilog, etc.       │
└─────────────────────────────────────────────────────────────────┘
```

## Project Structure

```
AbyssalBot.Infrastructure/
├── Memory/                    # Memory reading implementations
│   ├── SanderlingMemoryReader.cs
│   ├── SanderlingGameStateReader.cs
│   ├── SanderlingShipStateReader.cs
│   ├── SanderlingOverviewReader.cs
│   ├── SanderlingInventoryReader.cs
│   └── SanderlingUIStateReader.cs
│
├── Input/                     # Input simulation implementations
│   ├── WindowMotorInputSimulator.cs
│   ├── WindowMotorMouseController.cs
│   ├── WindowMotorKeyboardController.cs
│   └── WindowMotorMotionExecutor.cs
│
├── Caching/                   # Caching implementations
│   ├── MemoryGameStateCache.cs
│   ├── RedisGameStateCache.cs
│   ├── RedisCacheProvider.cs
│   └── RedisTemporalCache.cs
│
├── Database/                  # Database implementations
│   ├── EFAbyssDataRepository.cs
│   ├── InMemoryConfigurationRepository.cs
│   ├── EFConfigurationRepository.cs
│   └── EFSessionRepository.cs
│
├── Logging/                   # Logging implementations
│   ├── SerilogBotLogger.cs
│   ├── OpenTelemetryCollector.cs
│   └── FileActionRecorder.cs
│
└── README.md
```

## Key Design Principles

### 1. Dependency Inversion
- **Domain layer defines interfaces** (what it needs)
- **Infrastructure layer implements interfaces** (how to provide it)
- Domain never depends on Infrastructure

### 2. Interface Segregation
- Interfaces are small and focused
- Clients depend only on the interfaces they use
- Example: `IMemoryReader` vs `IGameStateReader` vs `IShipStateReader`

### 3. Single Responsibility
- Each implementation has one reason to change
- `SanderlingMemoryReader`: only changes when Sanderling API changes
- `WindowMotorInputSimulator`: only changes when WindowMotor API changes

### 4. Testability
- Domain logic can be tested with mock implementations
- Infrastructure can be tested independently
- No concrete dependencies in domain

## Interface Categories

### Memory Reading Layer
| Interface | Purpose | Implementation |
|-----------|---------|----------------|
| `IMemoryReader` | Raw memory access | `SanderlingMemoryReader` |
| `IGameStateReader` | Game state parsing | `SanderlingGameStateReader` |
| `IShipStateReader` | Ship status/fitting | `SanderlingShipStateReader` |
| `IOverviewReader` | Overview/targets | `SanderlingOverviewReader` |
| `IInventoryReader` | Cargo/inventory | `SanderlingInventoryReader` |
| `IUIStateReader` | UI window states | `SanderlingUIStateReader` |

### Input/Motor Layer
| Interface | Purpose | Implementation |
|-----------|---------|----------------|
| `IInputSimulator` | High-level input | `WindowMotorInputSimulator` |
| `IMouseController` | Mouse operations | `WindowMotorMouseController` |
| `IKeyboardController` | Keyboard operations | `WindowMotorKeyboardController` |
| `IMotionExecutor` | Motion execution | `WindowMotorMotionExecutor` |

### Caching Layer
| Interface | Purpose | Implementation |
|-----------|---------|----------------|
| `IGameStateCache` | Game state cache | `MemoryGameStateCache`, `RedisGameStateCache` |
| `ICacheProvider<T>` | Generic caching | `RedisCacheProvider<T>` |
| `ITemporalCache` | Time-based cache | `RedisTemporalCache` |

### Database Layer
| Interface | Purpose | Implementation |
|-----------|---------|----------------|
| `IAbyssDataRepository` | Abyss data/stats | `EFAbyssDataRepository` |
| `IConfigurationRepository` | Settings | `InMemoryConfigurationRepository`, `EFConfigurationRepository` |
| `ISessionRepository` | Session tracking | `EFSessionRepository` |

### Logging & Telemetry
| Interface | Purpose | Implementation |
|-----------|---------|----------------|
| `IBotLogger` | Structured logging | `SerilogBotLogger` |
| `ITelemetryCollector` | Metrics/traces | `OpenTelemetryCollector` |
| `IActionRecorder` | Action history | `FileActionRecorder` |

## Usage Examples

### Dependency Injection Setup

```csharp
// Program.cs or Startup.cs
services.AddSingleton<IMemoryReader, SanderlingMemoryReader>();
services.AddSingleton<IGameStateReader, SanderlingGameStateReader>();
services.AddSingleton<IShipStateReader, SanderlingShipStateReader>();

services.AddSingleton<IInputSimulator, WindowMotorInputSimulator>();
services.AddSingleton<IMotionExecutor, WindowMotorMotionExecutor>();

services.AddSingleton<IGameStateCache, MemoryGameStateCache>();
services.AddSingleton<IConfigurationRepository, InMemoryConfigurationRepository>();

services.AddSingleton<IBotLogger, SerilogBotLogger>();
services.AddSingleton<ITelemetryCollector, OpenTelemetryCollector>();
```

### Using Infrastructure in Domain Services

```csharp
// Domain service uses interfaces, not implementations
public class CombatStrategyService
{
    private readonly IShipStateReader _shipStateReader;
    private readonly IOverviewReader _overviewReader;
    private readonly IBotLogger _logger;

    public CombatStrategyService(
        IShipStateReader shipStateReader,
        IOverviewReader overviewReader,
        IBotLogger logger)
    {
        _shipStateReader = shipStateReader;
        _overviewReader = overviewReader;
        _logger = logger;
    }

    public async Task<CombatDecision> DecideAsync(CancellationToken ct)
    {
        // Read ship state through abstraction
        var shipStatus = await _shipStateReader.ReadShipStatusAsync(ct);
        var targets = await _overviewReader.ReadTargetsAsync(ct);

        // Make decisions based on domain logic
        var decision = /* ... */;

        // Log through abstraction
        _logger.LogInformation("Combat decision made",
            LogContext.FromCurrentState(/* ... */));

        return decision;
    }
}
```

### Switching Implementations

```csharp
// Easy to switch from in-memory to Redis cache
// Just change the DI registration:

// Development
services.AddSingleton<IGameStateCache, MemoryGameStateCache>();

// Production
services.AddSingleton<IGameStateCache, RedisGameStateCache>();

// Domain code doesn't change at all!
```

## Benefits

### 1. Clean Separation of Concerns
- Domain logic is pure and testable
- Infrastructure complexity is isolated
- Easy to understand and maintain

### 2. Technology Independence
- Can swap Sanderling for another memory reader
- Can swap Redis for another cache
- Can swap Serilog for another logger

### 3. Testability
```csharp
// Easy to mock for tests
var mockShipStateReader = new Mock<IShipStateReader>();
mockShipStateReader
    .Setup(x => x.ReadShipStatusAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(new ShipHitpointsAndEnergy(/* ... */));

var service = new CombatStrategyService(
    mockShipStateReader.Object,
    /* ... */
);
```

### 4. Parallel Development
- Domain team works on interfaces and logic
- Infrastructure team implements adapters
- Teams work independently

### 5. Easy Configuration
- Swap implementations via DI
- Use different implementations for different environments
- No code changes required

## Migration Path

To migrate existing code to use these abstractions:

1. **Identify dependencies** in domain services
2. **Extract interfaces** to Domain/Interfaces/Infrastructure
3. **Create implementations** in Infrastructure project
4. **Update DI configuration** to wire up implementations
5. **Test** with both real and mock implementations

## Future Enhancements

- [ ] Add `RedisGameStateCache` implementation
- [ ] Add `EFAbyssDataRepository` implementation
- [ ] Add `OpenTelemetryCollector` implementation
- [ ] Add health checks for infrastructure services
- [ ] Add circuit breakers for external dependencies
- [ ] Add retry policies for transient failures
- [ ] Add connection pooling configurations
- [ ] Add infrastructure monitoring dashboards

## Contributing

When adding new infrastructure interfaces:

1. **Define interface in Domain project** first
2. **Add XML documentation** explaining purpose
3. **Use async/await** throughout
4. **Add CancellationToken support**
5. **Create at least one implementation** in Infrastructure
6. **Add usage examples** to this README
7. **Update dependency diagram**

## References

- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
