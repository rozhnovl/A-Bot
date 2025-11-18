# Infrastructure Quick Reference

## Interface → Implementation Mapping

### Memory Reading Layer

| Interface | Implementation | Status | Location |
|-----------|----------------|--------|----------|
| `IMemoryReader` | `SanderlingMemoryReader` | ✅ Created | `/src/AbyssalBot.Infrastructure/Memory/` |
| `IGameStateReader` | `SanderlingGameStateReader` | ✅ Created | `/src/AbyssalBot.Infrastructure/Memory/` |
| `IShipStateReader` | `SanderlingShipStateReader` | ⏳ Pending | - |
| `IOverviewReader` | `SanderlingOverviewReader` | ⏳ Pending | - |
| `IInventoryReader` | `SanderlingInventoryReader` | ⏳ Pending | - |
| `IUIStateReader` | `SanderlingUIStateReader` | ⏳ Pending | - |

### Input/Motor Layer

| Interface | Implementation | Status | Location |
|-----------|----------------|--------|----------|
| `IInputSimulator` | `WindowMotorInputSimulator` | ✅ Created | `/src/AbyssalBot.Infrastructure/Input/` |
| `IMouseController` | `WindowMotorMouseController` | ⏳ Pending | - |
| `IKeyboardController` | `WindowMotorKeyboardController` | ⏳ Pending | - |
| `IMotionExecutor` | `WindowMotorMotionExecutor` | ✅ Created | `/src/AbyssalBot.Infrastructure/Input/` |

### Caching Layer

| Interface | Implementation | Status | Location |
|-----------|----------------|--------|----------|
| `IGameStateCache` | `MemoryGameStateCache` | ✅ Created | `/src/AbyssalBot.Infrastructure/Caching/` |
| `IGameStateCache` | `RedisGameStateCache` | ⏳ Pending | - |
| `ICacheProvider<T>` | `RedisCacheProvider<T>` | ⏳ Pending | - |
| `ITemporalCache` | `RedisTemporalCache` | ⏳ Pending | - |

### Database Layer

| Interface | Implementation | Status | Location |
|-----------|----------------|--------|----------|
| `IAbyssDataRepository` | `EFAbyssDataRepository` | ⏳ Pending | - |
| `IConfigurationRepository` | `InMemoryConfigurationRepository` | ✅ Created | `/src/AbyssalBot.Infrastructure/Database/` |
| `IConfigurationRepository` | `EFConfigurationRepository` | ⏳ Pending | - |
| `ISessionRepository` | `EFSessionRepository` | ⏳ Pending | - |

### Logging & Telemetry Layer

| Interface | Implementation | Status | Location |
|-----------|----------------|--------|----------|
| `IBotLogger` | `SerilogBotLogger` | ✅ Created | `/src/AbyssalBot.Infrastructure/Logging/` |
| `ITelemetryCollector` | `OpenTelemetryCollector` | ⏳ Pending | - |
| `IActionRecorder` | `FileActionRecorder` | ⏳ Pending | - |

## Common Usage Patterns

### Reading Memory

```csharp
// Inject the reader
public class MyService
{
    private readonly IShipStateReader _shipStateReader;

    public MyService(IShipStateReader shipStateReader)
    {
        _shipStateReader = shipStateReader;
    }

    public async Task DoWorkAsync(CancellationToken ct)
    {
        var shipStatus = await _shipStateReader.ReadShipStatusAsync(ct);
        // Use shipStatus...
    }
}
```

### Executing Input

```csharp
// Inject the input simulator
public class MyBot
{
    private readonly IInputSimulator _inputSimulator;

    public MyBot(IInputSimulator inputSimulator)
    {
        _inputSimulator = inputSimulator;
    }

    public async Task ClickTargetAsync(int x, int y, CancellationToken ct)
    {
        var result = await _inputSimulator.ClickAsync(x, y, MouseButton.Left, ct);
        if (!result.Success)
        {
            // Handle error
        }
    }
}
```

### Using Cache

```csharp
// Inject the cache
public class GameStateService
{
    private readonly IGameStateCache _cache;
    private readonly IGameStateReader _reader;

    public async Task<GameState> GetGameStateAsync(CancellationToken ct)
    {
        // Try cache first
        var cached = await _cache.GetGameStateAsync(ct);
        if (cached != null)
        {
            return cached;
        }

        // Read from memory
        var state = await _reader.ReadCurrentStateAsync(ct);

        // Cache it
        await _cache.SetGameStateAsync(state, expirationSeconds: 5, ct);

        return state;
    }
}
```

### Logging

```csharp
// Inject the logger
public class CombatService
{
    private readonly IBotLogger _logger;

    public CombatService(IBotLogger logger)
    {
        _logger = logger;
    }

    public void ProcessCombat()
    {
        using var scope = _logger.BeginScope("CombatProcessing");

        _logger.LogInformation("Starting combat processing");

        try
        {
            // Do work...
            _logger.LogMetric("TargetsEngaged", 5, "count");
        }
        catch (Exception ex)
        {
            _logger.LogError("Combat processing failed", ex);
        }
    }
}
```

### Storing Configuration

```csharp
// Inject the repository
public class ConfigurationService
{
    private readonly IConfigurationRepository _configRepo;

    public async Task<string> GetSettingAsync(string key)
    {
        return await _configRepo.GetConfigurationAsync(key);
    }

    public async Task SaveFittingAsync(string name, BotFittingConfiguration fitting)
    {
        await _configRepo.SaveShipFittingAsync(name, fitting);
    }
}
```

## DI Registration Examples

### ASP.NET Core

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Memory Reading
builder.Services.AddSingleton<IMemoryReader, SanderlingMemoryReader>();
builder.Services.AddSingleton<IGameStateReader, SanderlingGameStateReader>();
builder.Services.AddSingleton<IShipStateReader, SanderlingShipStateReader>();

// Input/Motor
builder.Services.AddSingleton<IInputSimulator, WindowMotorInputSimulator>();
builder.Services.AddSingleton<IMotionExecutor, WindowMotorMotionExecutor>();

// Caching
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IGameStateCache, MemoryGameStateCache>();

// Database
builder.Services.AddSingleton<IConfigurationRepository, InMemoryConfigurationRepository>();

// Logging
builder.Services.AddSingleton<IBotLogger, SerilogBotLogger>();

var app = builder.Build();
```

### Console Application

```csharp
// Program.cs
var services = new ServiceCollection();

// Memory Reading
services.AddSingleton<IMemoryReader, SanderlingMemoryReader>();
services.AddSingleton<IGameStateReader, SanderlingGameStateReader>();

// Input/Motor
services.AddSingleton<IInputSimulator, WindowMotorInputSimulator>();

// Caching
services.AddMemoryCache();
services.AddSingleton<IGameStateCache, MemoryGameStateCache>();

// Logging
services.AddSingleton<IBotLogger>(sp =>
{
    var logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.File("logs/bot.log", rollingInterval: RollingInterval.Day)
        .CreateLogger();
    return new SerilogBotLogger(logger);
});

var provider = services.BuildServiceProvider();
```

## File Locations Reference

### Interfaces (Domain Project)
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

### Implementations (Infrastructure Project)
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
└── AbyssalBot.Infrastructure.csproj
```

### Documentation
```
/home/user/A-Bot/docs/
├── INFRASTRUCTURE_ARCHITECTURE.md  (Detailed architecture)
├── INFRASTRUCTURE_SUMMARY.md       (Complete inventory)
└── INFRASTRUCTURE_QUICK_REFERENCE.md (This file)

/home/user/A-Bot/src/AbyssalBot.Infrastructure/
└── README.md                       (Usage guide)
```

## Testing Examples

### Unit Testing with Mocks

```csharp
using Moq;
using Xunit;

public class CombatStrategyServiceTests
{
    [Fact]
    public async Task Should_Make_Decision_Based_On_Ship_Status()
    {
        // Arrange
        var mockShipReader = new Mock<IShipStateReader>();
        mockShipReader
            .Setup(x => x.ReadShipStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShipHitpointsAndEnergy(
                ShieldHitpoints: 1000,
                ArmorHitpoints: 500,
                HullHitpoints: 300,
                CapacitorCurrent: 800,
                CapacitorMax: 1000
            ));

        var service = new CombatStrategyService(mockShipReader.Object);

        // Act
        var decision = await service.MakeDecisionAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(decision);
        // No Sanderling required!
    }
}
```

## Common Patterns

### Repository Pattern
```csharp
// Define in Domain
public interface IMyRepository
{
    Task<MyEntity> GetByIdAsync(Guid id, CancellationToken ct);
    Task SaveAsync(MyEntity entity, CancellationToken ct);
}

// Implement in Infrastructure
public class EFMyRepository : IMyRepository
{
    private readonly DbContext _context;

    public async Task<MyEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.MyEntities
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task SaveAsync(MyEntity entity, CancellationToken ct)
    {
        _context.MyEntities.Add(entity);
        await _context.SaveChangesAsync(ct);
    }
}
```

### Cache-Aside Pattern
```csharp
public async Task<T> GetOrCreateAsync<T>(
    string key,
    Func<Task<T>> factory,
    IGameStateCache cache,
    CancellationToken ct)
{
    // Try cache
    var cached = await cache.GetShipStateAsync<T>(key, ct);
    if (cached != null)
    {
        return cached;
    }

    // Create/fetch
    var value = await factory();

    // Cache it
    await cache.SetShipStateAsync(key, value, expirationSeconds: 5, ct);

    return value;
}
```

## Status Summary

- **Interfaces Defined**: 19/19 (100%)
- **Core Implementations**: 7/19 (37%)
- **Documentation**: 4 documents
- **Ready for Integration**: ✅ Yes

## Next Priority Implementations

1. **High Priority** (Core functionality)
   - [ ] `SanderlingShipStateReader`
   - [ ] `SanderlingOverviewReader`
   - [ ] `WindowMotorMouseController`
   - [ ] `WindowMotorKeyboardController`

2. **Medium Priority** (Enhanced functionality)
   - [ ] `SanderlingInventoryReader`
   - [ ] `SanderlingUIStateReader`
   - [ ] `RedisGameStateCache`
   - [ ] `EFAbyssDataRepository`

3. **Lower Priority** (Optional features)
   - [ ] `OpenTelemetryCollector`
   - [ ] `FileActionRecorder`
   - [ ] `EFSessionRepository`
