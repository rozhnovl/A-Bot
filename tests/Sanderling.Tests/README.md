# Sanderling.Tests

Comprehensive test suite for Sanderling parsing and memory reading components.

## Overview

This test project provides extensive coverage for the EVE Online memory reading and parsing functionality in Sanderling. The tests are designed to handle various locales, edge cases, and real-world EVE Online scenarios.

## Test Structure

### Unit Tests

#### NumberParserTests.cs
Tests for number parsing functionality across different locales and formats.

**Coverage:**
- Basic number parsing (integers, decimals)
- Negative and positive signed numbers
- Locale-specific formats (US, German, French, Swiss)
- Different group separators (comma, period, apostrophe, space)
- Mixed separators (e.g., `1.234.567,89`)
- Large numbers and precision handling
- Roman numeral parsing
- Edge cases (null, empty, whitespace, invalid input)
- Multi-culture integration tests

**Key Features:**
- Tests all supported EVE Online number formats
- Validates parsing consistency across different Windows locale settings
- Covers decimal precision and rounding behavior
- Tests Roman numerals used in system names (e.g., "Jita IV")

#### DistanceParserTests.cs
Tests for distance parsing in meters (m), kilometers (km), and astronomical units (AU).

**Coverage:**
- Meter distance parsing
- Kilometer distance parsing (whole and decimal)
- Astronomical unit parsing
- Different number format locales
- Whitespace handling
- Boundary conditions (zero, very large, very small)
- EVE Online rounding behavior (always rounds down)
- Real-world engagement ranges

**Key Features:**
- Tests all distance units used in EVE Online
- Validates min/max range calculation
- Covers typical combat and navigation distances
- Tests astronomical unit precision

#### InventoryParserTests.cs
Tests for inventory capacity gauge and ship cargo space parsing.

**Coverage:**
- Capacity gauge parsing (used, max, selected)
- Different locale number formats
- Cyrillic unit support (м³)
- Ship name and type extraction
- Cargo space type identification
- Empty, partial, and full cargo scenarios
- Large capacity values (freighters)
- Specialized holds (drone bay, ore hold, etc.)

**Key Features:**
- Tests capacity gauge formats: `100 / 5,000 m³` and `(50) 100 / 5,000 m³`
- Validates ship label parsing: `Ship Name (Ship Type)`
- Covers all ship size categories
- Tests specialized cargo holds

#### OverviewParserTests.cs
Tests for overview entry parsing and EWar effect detection.

**Coverage:**
- EWar type detection (ECM, Web, Warp Scramble, Warp Disrupt)
- Pattern matching in hint texts
- Case-insensitive matching
- Multiple simultaneous effects
- Edge cases (null, empty hints)

**Key Features:**
- Tests all EVE Online electronic warfare types
- Validates hint text pattern matching
- Distinguishes similar effects (scramble vs. disrupt)

#### ListEntryParserTests.cs
Tests for generic list entry parsing (used by Overview, Probe Scanner, etc.).

**Coverage:**
- Column value extraction
- Distance parsing from columns
- Name and Type extraction
- "No Item" detection
- Case-insensitive column matching
- Real-world list entry scenarios

**Key Features:**
- Tests overview entries
- Tests probe scan results
- Validates column-based data extraction

### Integration Tests

#### IntegrationTests.cs
End-to-end tests using realistic EVE Online data scenarios.

**Coverage:**
- Cross-parser validation
- Complete inventory scenarios
- Complete combat scenarios
- Real-world data from all parsers
- Multi-component interactions

**Key Features:**
- Simulates complete game state parsing
- Tests parser interactions
- Validates data consistency across parsers

### Test Fixtures

#### Fixtures/SampleData.cs
Provides realistic sample data for testing.

**Includes:**
- Number formats in various locales
- Distance strings (m, km, AU)
- Inventory capacity gauges
- Ship names and types
- Overview entry data
- EWar effect hints
- Roman numerals
- Cargo space type labels

## Running Tests

### Command Line
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~NumberParserTests"

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

### Visual Studio
1. Open Test Explorer (Test > Test Explorer)
2. Click "Run All" to run all tests
3. Use filters to run specific test categories

## Test Categories

### Edge Cases Covered
- **Null Values**: All parsers handle null input gracefully
- **Empty Strings**: Proper handling of empty and whitespace-only strings
- **Invalid Data**: Returns null or appropriate defaults for malformed input
- **Different Number Formats**: Supports US, German, French, Swiss, and other locale formats
- **Whitespace**: Handles leading, trailing, and embedded whitespace
- **Large Numbers**: Tests with values up to freighter cargo capacity (1.2M m³)
- **Precision**: Validates decimal precision and rounding behavior

### Real-World EVE Online Scenarios
- **Combat**: Parsing targets at various ranges with EWar effects
- **Inventory**: Ship cargo holds, specialized bays, selection states
- **Navigation**: Distances in m, km, and AU
- **Localization**: Multiple language/locale number formats

## Test Framework

- **Framework**: xUnit 2.9.2
- **Assertions**: FluentAssertions 7.0.0
- **Target**: .NET 9.0
- **Coverage**: coverlet.collector 6.0.2

## Parser Components Tested

1. **Number Parser** (`Parse/Number.cs`)
   - Decimal number parsing with locale support
   - Roman numeral parsing
   - Pattern-based number extraction

2. **Distance Parser** (`Parse/Distance.cs`)
   - Multi-unit distance parsing (m, km, AU)
   - Min/max range calculation
   - Unit conversion

3. **Inventory Parser** (`Parse/Inventory.cs`)
   - Capacity gauge parsing
   - Ship identification
   - Cargo space type detection

4. **Overview Parser** (`Parse/Overview.cs`)
   - EWar effect detection
   - Icon hint parsing

5. **List Entry Parser** (`Parse/ListEntry.cs`)
   - Column-based data extraction
   - Generic list entry parsing

## Edge Cases and Validation

### Locale Support
All parsers support number formats from:
- English (US/UK)
- German
- French
- Spanish
- Russian
- Swiss
- Pashto (special decimal separator ،)

### Null Safety
- All parsers return `null` for invalid input
- No exceptions thrown for malformed data
- Graceful degradation

### Data Validation
- Range checks (min < max)
- Capacity constraints (used ≤ max)
- Type safety with nullable return types

## Contributing

When adding new tests:
1. Follow existing naming conventions
2. Use descriptive test names (e.g., `ShouldParseKilometers_WhenInputIsValid`)
3. Add sample data to `Fixtures/SampleData.cs`
4. Include edge cases and real-world scenarios
5. Use FluentAssertions for readable assertions

## Known Limitations

1. Arabic locale with trailing sign not fully supported
2. Some edge cases for multi-digit Roman numerals may not be covered
3. Limited testing of concurrent parser usage

## Future Enhancements

- [ ] Performance benchmarks
- [ ] Memory leak detection
- [ ] Concurrent parsing tests
- [ ] More comprehensive UI element identification tests
- [ ] Additional locale testing
