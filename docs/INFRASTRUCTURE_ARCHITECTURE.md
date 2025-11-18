# Infrastructure Architecture

## Overview

The AbyssalBot infrastructure layer implements the **Hexagonal Architecture** pattern (also known as Ports and Adapters), creating a clean separation between domain logic and infrastructure concerns.

## Dependency Flow

```
┌────────────────────────────────────────────────────────────────────┐
│                                                                    │
│                        Application Layer                          │
│                   (Orchestrates use cases)                        │
│                                                                    │
└──────────────────────────┬─────────────────────────────────────────┘
                           │
                           │ depends on
                           ▼
┌────────────────────────────────────────────────────────────────────┐
│                         Domain Layer                               │
│                    (Pure business logic)                          │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │                      Models/                                 │ │
│  │  Target, ShipFitting, CombatDecision, etc.                  │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │                     Services/                                │ │
│  │  CombatStrategyService, TargetPriorityService, etc.         │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │              Interfaces/Infrastructure/                      │ │
│  │  (Ports - what the domain needs from infrastructure)        │ │
│  │                                                              │ │
│  │  Memory Reading:                                            │ │
│  │    IMemoryReader, IGameStateReader, IShipStateReader       │ │
│  │                                                              │ │
│  │  Input/Motor:                                               │ │
│  │    IInputSimulator, IMouseController, IMotionExecutor      │ │
│  │                                                              │ │
│  │  Caching:                                                   │ │
│  │    IGameStateCache, ICacheProvider<T>, ITemporalCache      │ │
│  │                                                              │ │
│  │  Database:                                                  │ │
│  │    IAbyssDataRepository, IConfigurationRepository          │ │
│  │                                                              │ │
│  │  Logging:                                                   │ │
│  │    IBotLogger, ITelemetryCollector, IActionRecorder        │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
└──────────────────────────┬─────────────────────────────────────────┘
                           │
                           │ implemented by
                           ▼
┌────────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                            │
│                 (Adapters - concrete implementations)              │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │                    Memory/                                   │ │
│  │  SanderlingMemoryReader       → IMemoryReader               │ │
│  │  SanderlingGameStateReader    → IGameStateReader            │ │
│  │  SanderlingShipStateReader    → IShipStateReader            │ │
│  │  SanderlingOverviewReader     → IOverviewReader             │ │
│  │  SanderlingInventoryReader    → IInventoryReader            │ │
│  │  SanderlingUIStateReader      → IUIStateReader              │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │                    Input/                                    │ │
│  │  WindowMotorInputSimulator    → IInputSimulator             │ │
│  │  WindowMotorMouseController   → IMouseController            │ │
│  │  WindowMotorKeyboardController → IKeyboardController        │ │
│  │  WindowMotorMotionExecutor    → IMotionExecutor             │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │                   Caching/                                   │ │
│  │  MemoryGameStateCache         → IGameStateCache             │ │
│  │  RedisGameStateCache          → IGameStateCache             │ │
│  │  RedisCacheProvider<T>        → ICacheProvider<T>           │ │
│  │  RedisTemporalCache           → ITemporalCache              │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │                   Database/                                  │ │
│  │  EFAbyssDataRepository        → IAbyssDataRepository        │ │
│  │  InMemoryConfigurationRepo    → IConfigurationRepository    │ │
│  │  EFConfigurationRepository    → IConfigurationRepository    │ │
│  │  EFSessionRepository          → ISessionRepository          │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐ │
│  │                   Logging/                                   │ │
│  │  SerilogBotLogger             → IBotLogger                  │ │
│  │  OpenTelemetryCollector       → ITelemetryCollector         │ │
│  │  FileActionRecorder           → IActionRecorder             │ │
│  └──────────────────────────────────────────────────────────────┘ │
│                                                                    │
└──────────────────────────┬─────────────────────────────────────────┘
                           │
                           │ uses
                           ▼
┌────────────────────────────────────────────────────────────────────┐
│                    External Dependencies                           │
│                                                                    │
│  Sanderling, WindowMotor, Redis, EF Core, Serilog, OpenTelemetry │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

## Data Flow Example: Combat Decision

```
┌──────────────────┐
│  Bot Main Loop   │
└────────┬─────────┘
         │
         │ 1. Request combat decision
         ▼
┌─────────────────────────────────┐
│  CombatStrategyService          │  ◄─── Domain Layer
│  (Domain Logic)                 │
└────────┬────────────────────────┘
         │
         │ 2. Read ship state
         ▼
┌─────────────────────────────────┐
│  IShipStateReader               │  ◄─── Interface (Port)
│  (Interface)                    │
└────────┬────────────────────────┘
         │
         │ 3. Implementation
         ▼
┌─────────────────────────────────┐
│  SanderlingShipStateReader      │  ◄─── Infrastructure (Adapter)
│  (Concrete Implementation)      │
└────────┬────────────────────────┘
         │
         │ 4. Memory read
         ▼
┌─────────────────────────────────┐
│  Sanderling Library             │  ◄─── External Dependency
│  (Memory Reading)               │
└────────┬────────────────────────┘
         │
         │ 5. Raw memory data
         ▼
┌─────────────────────────────────┐
│  SanderlingShipStateReader      │
│  (Parses and maps to domain)    │
└────────┬────────────────────────┘
         │
         │ 6. Domain model
         ▼
┌─────────────────────────────────┐
│  ShipHitpointsAndEnergy         │  ◄─── Domain Model
│  (Clean domain object)          │
└────────┬────────────────────────┘
         │
         │ 7. Return to service
         ▼
┌─────────────────────────────────┐
│  CombatStrategyService          │
│  (Makes decision using          │
│   pure domain logic)            │
└────────┬────────────────────────┘
         │
         │ 8. Return decision
         ▼
┌──────────────────┐
│  Bot Main Loop   │
└──────────────────┘
```

## Interface Inventory

### Created Interfaces (19 total)

#### Memory Reading Layer (6 interfaces)
1. **IMemoryReader** - Raw memory access to EVE client
2. **IGameStateReader** - Parsed game state (location, time, readiness)
3. **IShipStateReader** - Ship status, fitting, movement
4. **IOverviewReader** - Overview entries and targets
5. **IInventoryReader** - Cargo, drone bay, inventory
6. **IUIStateReader** - UI window states and notifications

#### Input/Motor Layer (4 interfaces)
7. **IInputSimulator** - High-level input abstraction
8. **IMouseController** - Mouse operations (click, drag, scroll)
9. **IKeyboardController** - Keyboard operations (keys, hotkeys)
10. **IMotionExecutor** - Motion command execution

#### Caching Layer (3 interfaces)
11. **IGameStateCache** - Game state caching
12. **ICacheProvider<T>** - Generic cache operations
13. **ITemporalCache** - Time-based caching with expiration

#### Database Layer (3 interfaces)
14. **IAbyssDataRepository** - Abyss spawn data and statistics
15. **IConfigurationRepository** - Settings and configuration
16. **ISessionRepository** - Bot session tracking

#### Logging & Telemetry (3 interfaces)
17. **IBotLogger** - Structured logging
18. **ITelemetryCollector** - Metrics, traces, events
19. **IActionRecorder** - Bot action recording and analysis

### Created Implementations (7 implementations)

#### Memory Implementations
1. **SanderlingMemoryReader** → IMemoryReader
2. **SanderlingGameStateReader** → IGameStateReader

#### Input Implementations
3. **WindowMotorInputSimulator** → IInputSimulator
4. **WindowMotorMotionExecutor** → IMotionExecutor

#### Caching Implementations
5. **MemoryGameStateCache** → IGameStateCache

#### Database Implementations
6. **InMemoryConfigurationRepository** → IConfigurationRepository

#### Logging Implementations
7. **SerilogBotLogger** → IBotLogger

## Key Benefits

### 1. Testability
```csharp
// Easy to test domain logic with mocks
var mockShipStateReader = new Mock<IShipStateReader>();
mockShipStateReader
    .Setup(x => x.ReadShipStatusAsync(It.IsAny<CancellationToken>()))
    .ReturnsAsync(testShipStatus);

var service = new CombatStrategyService(mockShipStateReader.Object);
var decision = await service.DecideAsync(CancellationToken.None);

// Test domain logic without touching Sanderling!
```

### 2. Flexibility
```csharp
// Switch implementations easily via DI
// Development
services.AddSingleton<IGameStateCache, MemoryGameStateCache>();

// Production
services.AddSingleton<IGameStateCache, RedisGameStateCache>();

// Domain code unchanged!
```

### 3. Maintainability
- Each component has a single responsibility
- Changes to Sanderling API only affect one place
- Easy to locate and fix infrastructure issues

### 4. Technology Independence
- Can replace Sanderling with another memory reader
- Can replace WindowMotor with another input system
- Domain logic remains pure

## Design Patterns Used

1. **Dependency Inversion Principle** - Domain defines needs, infrastructure provides
2. **Repository Pattern** - Data access abstraction
3. **Adapter Pattern** - Converting external APIs to domain interfaces
4. **Strategy Pattern** - Swappable implementations
5. **Factory Pattern** - Creating infrastructure instances

## Next Steps

### Recommended Implementation Order

1. ✅ **Phase 1: Interfaces Created** (Complete)
   - All 19 interfaces defined
   - Domain layer has zero infrastructure dependencies

2. **Phase 2: Core Implementations** (In Progress)
   - ✅ Memory: SanderlingMemoryReader, SanderlingGameStateReader
   - ✅ Input: WindowMotorInputSimulator, WindowMotorMotionExecutor
   - ✅ Caching: MemoryGameStateCache
   - ✅ Database: InMemoryConfigurationRepository
   - ✅ Logging: SerilogBotLogger

3. **Phase 3: Complete Implementations** (Next)
   - Memory: SanderlingShipStateReader, SanderlingOverviewReader
   - Input: WindowMotorMouseController, WindowMotorKeyboardController
   - Caching: RedisGameStateCache, RedisCacheProvider, RedisTemporalCache
   - Database: EFAbyssDataRepository, EFSessionRepository
   - Logging: OpenTelemetryCollector, FileActionRecorder

4. **Phase 4: Integration**
   - Wire up DI container
   - Migrate existing code to use interfaces
   - Add integration tests

5. **Phase 5: Advanced Features**
   - Health checks
   - Circuit breakers
   - Retry policies
   - Monitoring dashboards

## File Locations

### Domain Interfaces
```
/home/user/A-Bot/src/AbyssalBot.Domain/Interfaces/Infrastructure/
├── IMemoryReader.cs
├── IGameStateReader.cs
├── IShipStateReader.cs
├── IOverviewReader.cs
├── IInventoryReader.cs
├── IUIStateReader.cs
├── IInputSimulator.cs
├── IMouseController.cs
├── IKeyboardController.cs
├── IMotionExecutor.cs
├── IGameStateCache.cs
├── ICacheProvider.cs
├── ITemporalCache.cs
├── IAbyssDataRepository.cs
├── IConfigurationRepository.cs
├── ISessionRepository.cs
├── IBotLogger.cs
├── ITelemetryCollector.cs
└── IActionRecorder.cs
```

### Infrastructure Implementations
```
/home/user/A-Bot/src/AbyssalBot.Infrastructure/
├── Memory/
│   ├── SanderlingMemoryReader.cs
│   └── SanderlingGameStateReader.cs
├── Input/
│   ├── WindowMotorInputSimulator.cs
│   └── WindowMotorMotionExecutor.cs
├── Caching/
│   └── MemoryGameStateCache.cs
├── Database/
│   └── InMemoryConfigurationRepository.cs
├── Logging/
│   └── SerilogBotLogger.cs
└── README.md
```

## Success Metrics

- ✅ Zero infrastructure dependencies in Domain project
- ✅ All interfaces have XML documentation
- ✅ All interfaces support async/await
- ✅ All interfaces support CancellationToken
- ✅ Infrastructure project created with proper structure
- ✅ Sample implementations demonstrate pattern
- ✅ Comprehensive documentation created

## References

- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Dependency Inversion Principle](https://en.wikipedia.org/wiki/Dependency_inversion_principle)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
