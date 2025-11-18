using Sanderling.Parse;

namespace Sanderling.Tests;

/// <summary>
/// Tests for inventory parsing functionality.
/// Tests capacity gauge parsing and ship cargo space type identification.
/// </summary>
public class InventoryParserTests
{
    #region Capacity Gauge Parsing Tests

    [Theory]
    [InlineData("0 m³", 0L, null, null)]
    [InlineData("100 m³", 100000L, null, null)]
    [InlineData("1,000 m³", 1000000L, null, null)]
    [InlineData("5,000 m³", 5000000L, null, null)]
    public void ParseAsInventoryCapacityGaugeMilli_ShouldParseUsedOnly(
        string input,
        long expectedUsed,
        long? expectedMax,
        long? expectedSelected)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().Be(expectedUsed);
        result.Max.Should().Be(expectedMax);
        result.Selected.Should().Be(expectedSelected);
    }

    [Theory]
    [InlineData("100 / 5,000 m³", 100000L, 5000000L)]
    [InlineData("1,000 / 10,000 m³", 1000000L, 10000000L)]
    [InlineData("0 / 1,000 m³", 0L, 1000000L)]
    [InlineData("500 / 500 m³", 500000L, 500000L)]
    public void ParseAsInventoryCapacityGaugeMilli_ShouldParseUsedAndMax(
        string input,
        long expectedUsed,
        long expectedMax)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().Be(expectedUsed);
        result.Max.Should().Be(expectedMax);
    }

    [Theory]
    [InlineData("(50) 100 / 5,000 m³", 50000L, 100000L, 5000000L)]
    [InlineData("(100) 500 / 1,000 m³", 100000L, 500000L, 1000000L)]
    [InlineData("(0) 0 / 100 m³", 0L, 0L, 100000L)]
    public void ParseAsInventoryCapacityGaugeMilli_ShouldParseWithSelected(
        string input,
        long expectedSelected,
        long expectedUsed,
        long expectedMax)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Selected.Should().Be(expectedSelected);
        result.Used.Should().Be(expectedUsed);
        result.Max.Should().Be(expectedMax);
    }

    [Theory]
    [InlineData("100 м³", 100000L)] // Cyrillic 'м'
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandleCyrillicUnit(
        string input,
        long expectedUsed)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().Be(expectedUsed);
    }

    #endregion

    #region Different Number Formats

    [Theory]
    [InlineData("1.234,56 / 10.000 m³", 1234560L, 10000000L)] // German format
    [InlineData("1,234.56 / 10,000 m³", 1234560L, 10000000L)] // US format
    [InlineData("1'234.56 / 10'000 m³", 1234560L, 10000000L)] // Swiss format
    [InlineData("1 234,56 / 10 000 m³", 1234560L, 10000000L)] // French format
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandleDifferentLocales(
        string input,
        long expectedUsed,
        long expectedMax)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().Be(expectedUsed);
        result.Max.Should().Be(expectedMax);
    }

    [Theory]
    [InlineData("1.234،56 / 10.000 m³")] // Pashto decimal separator
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandlePashtoSeparators(string input)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().NotBeNull();
    }

    #endregion

    #region Whitespace Handling

    [Theory]
    [InlineData("  100 / 5,000 m³  ")]
    [InlineData("100  /  5,000  m³")]
    [InlineData("\t100 / 5,000 m³\t")]
    [InlineData("( 50 ) 100 / 5,000 m³")]
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandleWhitespace(string input)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().Be(100000L);
        result.Max.Should().Be(5000000L);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void ParseAsInventoryCapacityGaugeMilli_ShouldReturnNull_WhenInputIsNull()
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(null);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a gauge")]
    [InlineData("100")]
    [InlineData("100 liters")]
    public void ParseAsInventoryCapacityGaugeMilli_ShouldReturnNull_WhenInputIsInvalid(string input)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Capacity Gauge Equality Tests

    [Fact]
    public void InventoryCapacityGauge_ShouldBeEqual_WhenValuesAreTheSame()
    {
        // Arrange
        var gauge1 = new InventoryCapacityGauge { Used = 100, Max = 500, Selected = 50 };
        var gauge2 = new InventoryCapacityGauge { Used = 100, Max = 500, Selected = 50 };

        // Assert
        gauge1.Equals(gauge2).Should().BeTrue();
        gauge1.GetHashCode().Should().Be(gauge2.GetHashCode());
    }

    [Fact]
    public void InventoryCapacityGauge_ShouldNotBeEqual_WhenValuesAreDifferent()
    {
        // Arrange
        var gauge1 = new InventoryCapacityGauge { Used = 100, Max = 500, Selected = 50 };
        var gauge2 = new InventoryCapacityGauge { Used = 200, Max = 500, Selected = 50 };

        // Assert
        gauge1.Equals(gauge2).Should().BeFalse();
    }

    #endregion

    #region Ship Cargo Space Type Tests

    [Theory]
    [InlineData("Drone Bay", ShipCargoSpaceTypeEnum.DroneBay)]
    [InlineData("Ore Hold", ShipCargoSpaceTypeEnum.OreHold)]
    [InlineData("drone bay", ShipCargoSpaceTypeEnum.DroneBay)]
    [InlineData("ore hold", ShipCargoSpaceTypeEnum.OreHold)]
    public void FromIventoryLabelParseShipCargoSpaceType_ShouldParseKnownTypes(
        string label,
        ShipCargoSpaceTypeEnum expected)
    {
        // Act
        var result = InventoryExtension.FromIventoryLabelParseShipCargoSpaceType(label);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Unknown Bay")]
    [InlineData("Cargo Hold")]
    [InlineData("")]
    [InlineData(null)]
    public void FromIventoryLabelParseShipCargoSpaceType_ShouldReturnNull_WhenTypeIsUnknown(string label)
    {
        // Act
        var result = InventoryExtension.FromIventoryLabelParseShipCargoSpaceType(label);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Ship Name and Type Parsing Tests

    [Theory]
    [InlineData("Noctis (Noctis)", "Noctis", "Noctis")]
    [InlineData("My Drake (Drake)", "My Drake", "Drake")]
    [InlineData("Brutix Navy Issue (Brutix Navy Issue)", "Brutix Navy Issue", "Brutix Navy Issue")]
    [InlineData("Ship Name (Ship Type)", "Ship Name", "Ship Type")]
    public void ParseTreeEntryLabelShipNameAndType_ShouldParseValidFormat(
        string label,
        string expectedName,
        string expectedType)
    {
        // Act
        var result = InventoryExtension.ParseTreeEntryLabelShipNameAndType(label);

        // Assert
        result.Should().NotBeNull();
        result.Value.Key.Should().Be(expectedName);
        result.Value.Value.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("  Noctis (Noctis)  ", "Noctis", "Noctis")]
    [InlineData("My Drake  ( Drake )", "My Drake", "Drake")]
    public void ParseTreeEntryLabelShipNameAndType_ShouldTrimWhitespace(
        string label,
        string expectedName,
        string expectedType)
    {
        // Act
        var result = InventoryExtension.ParseTreeEntryLabelShipNameAndType(label);

        // Assert
        result.Should().NotBeNull();
        result.Value.Key.Should().Be(expectedName);
        result.Value.Value.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("Just a label")]
    [InlineData("Missing (closing paren")]
    [InlineData("Missing opening) paren")]
    [InlineData("")]
    [InlineData(null)]
    public void ParseTreeEntryLabelShipNameAndType_ShouldReturnNull_WhenFormatIsInvalid(string label)
    {
        // Act
        var result = InventoryExtension.ParseTreeEntryLabelShipNameAndType(label);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Real-World EVE Online Scenarios

    [Theory]
    [InlineData("0 / 5,000 m³")] // Empty cargo
    [InlineData("5,000 / 5,000 m³")] // Full cargo
    [InlineData("2,500 / 5,000 m³")] // Half full cargo
    [InlineData("(100) 2,600 / 5,000 m³")] // Items selected
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandleCommonScenarios(string input)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().NotBeNull();
        result.Max.Should().NotBeNull();
    }

    [Theory]
    [InlineData("0 / 50 m³")] // Small ship
    [InlineData("0 / 400 m³")] // Frigate
    [InlineData("0 / 5,000 m³")] // Battleship
    [InlineData("0 / 62,500 m³")] // Industrial ship
    [InlineData("0 / 1,200,000 m³")] // Freighter
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandleDifferentShipSizes(string input)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().Be(0);
        result.Max.Should().NotBeNull();
        result.Max.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("0 / 50 m³")] // Drone bay
    [InlineData("0 / 150,000 m³")] // Large ore hold
    [InlineData("0 / 28,000 m³")] // Ship maintenance bay
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandleSpecializedHolds(string input)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().Be(0);
        result.Max.Should().NotBeNull();
    }

    #endregion

    #region Precision and Large Number Tests

    [Theory]
    [InlineData("123,456.78 / 1,000,000 m³", 123456780L, 1000000000L)]
    [InlineData("999,999.99 / 1,000,000 m³", 999999990L, 1000000000L)]
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandleDecimalPrecision(
        string input,
        long expectedUsed,
        long expectedMax)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().Be(expectedUsed);
        result.Max.Should().Be(expectedMax);
    }

    [Theory]
    [InlineData("0 / 10,000,000 m³", 0L, 10000000000L)]
    [InlineData("5,000,000 / 10,000,000 m³", 5000000000L, 10000000000L)]
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandleVeryLargeCapacities(
        string input,
        long expectedUsed,
        long expectedMax)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().Be(expectedUsed);
        result.Max.Should().Be(expectedMax);
    }

    #endregion

    #region Boundary Conditions

    [Theory]
    [InlineData("0 / 0 m³", 0L, 0L)]
    [InlineData("0.0 / 0.0 m³", 0L, 0L)]
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandleZeroCapacity(
        string input,
        long expectedUsed,
        long expectedMax)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Used.Should().Be(expectedUsed);
        result.Max.Should().Be(expectedMax);
    }

    [Theory]
    [InlineData("(0) 100 / 1,000 m³", 0L)]
    [InlineData("(1,000) 1,000 / 1,000 m³", 1000000L)]
    public void ParseAsInventoryCapacityGaugeMilli_ShouldHandleBoundarySelections(
        string input,
        long expectedSelected)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(input);

        // Assert
        result.Should().NotBeNull();
        result.Selected.Should().Be(expectedSelected);
    }

    #endregion
}
