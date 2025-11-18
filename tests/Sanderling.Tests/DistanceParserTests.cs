using Sanderling.Parse;

namespace Sanderling.Tests;

/// <summary>
/// Tests for distance parsing functionality.
/// EVE Online displays distances in meters (m), kilometers (km), and astronomical units (AU).
/// </summary>
public class DistanceParserTests
{
    #region Meter Distance Tests

    [Theory]
    [InlineData("0 m", 0L, 1L)]
    [InlineData("1 m", 1L, 2L)]
    [InlineData("10 m", 10L, 11L)]
    [InlineData("100 m", 100L, 101L)]
    [InlineData("1,000 m", 1000L, 1001L)]
    [InlineData("10,000 m", 10000L, 10001L)]
    public void DistanceParse_ShouldParseMeters(string input, long expectedMin, long expectedMax)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().Be(expectedMin);
        max.Should().Be(expectedMax);
    }

    [Theory]
    [InlineData("500 m")]
    [InlineData("999 m")]
    [InlineData("5,500 m")]
    public void DistanceParseMin_ShouldParseMeters(string input)
    {
        // Act
        var result = Distance.DistanceParseMin(input);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Theory]
    [InlineData("500 m")]
    [InlineData("999 m")]
    public void DistanceParseMax_ShouldParseMeters(string input)
    {
        // Act
        var result = Distance.DistanceParseMax(input);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    #endregion

    #region Kilometer Distance Tests

    [Theory]
    [InlineData("1 km", 1000L, 2000L)]
    [InlineData("10 km", 10000L, 11000L)]
    [InlineData("100 km", 100000L, 101000L)]
    [InlineData("1,000 km", 1000000L, 1001000L)]
    public void DistanceParse_ShouldParseKilometers(string input, long expectedMin, long expectedMax)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().Be(expectedMin);
        max.Should().Be(expectedMax);
    }

    [Theory]
    [InlineData("1.5 km", 1500L)]
    [InlineData("2.75 km", 2750L)]
    [InlineData("10.25 km", 10250L)]
    [InlineData("100.5 km", 100500L)]
    public void DistanceParseMin_ShouldParseDecimalKilometers(string input, long expectedMin)
    {
        // Act
        var result = Distance.DistanceParseMin(input);

        // Assert
        result.Should().Be(expectedMin);
    }

    [Theory]
    [InlineData("1,5 km", 1500L)] // European format
    [InlineData("2,75 km", 2750L)]
    [InlineData("10,25 km", 10250L)]
    public void DistanceParseMin_ShouldParseDecimalKilometersWithComma(string input, long expectedMin)
    {
        // Act
        var result = Distance.DistanceParseMin(input);

        // Assert
        result.Should().Be(expectedMin);
    }

    #endregion

    #region Astronomical Unit Distance Tests

    [Theory]
    [InlineData("1 AU", 149597870700L)]
    [InlineData("2 AU", 299195741400L)]
    [InlineData("10 AU", 1495978707000L)]
    public void DistanceParse_ShouldParseAU(string input, long expectedMin)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().Be(expectedMin);
        max.Should().NotBeNull();
        max.Should().BeGreaterThan(min.Value);
    }

    [Theory]
    [InlineData("1.5 AU")]
    [InlineData("2.75 AU")]
    [InlineData("0.5 AU")]
    public void DistanceParseMin_ShouldParseDecimalAU(string input)
    {
        // Act
        var result = Distance.DistanceParseMin(input);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void AstronomicalUnit_ShouldBeCorrectValue()
    {
        // Assert
        Distance.AstronomicalUnit.Should().Be(149597870700L);
    }

    [Theory]
    [InlineData("1.0 AU")]
    [InlineData("1.5 AU")]
    [InlineData("2.0 AU")]
    public void DistanceParse_ShouldHaveSmallerBoundsForAU(string input)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().NotBeNull();
        max.Should().NotBeNull();

        var diff = max.Value - min.Value;
        // For AU, the difference should be AstronomicalUnit / 10
        diff.Should().Be(Distance.AstronomicalUnit / 10);
    }

    #endregion

    #region Whitespace and Format Tests

    [Theory]
    [InlineData("  100 m  ")]
    [InlineData("\t100 m\t")]
    [InlineData("100  m")]
    [InlineData("100\tm")]
    public void DistanceParse_ShouldHandleWhitespace(string input)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().Be(100L);
        max.Should().Be(101L);
    }

    [Theory]
    [InlineData("1,234 km", 1234000L)]
    [InlineData("1.234 km", 1234000L)]
    [InlineData("1'234 km", 1234000L)]
    [InlineData("1 234 km", 1234000L)]
    public void DistanceParseMin_ShouldHandleDifferentGroupSeparators(string input, long expectedMin)
    {
        // Act
        var result = Distance.DistanceParseMin(input);

        // Assert
        result.Should().Be(expectedMin);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void DistanceParse_ShouldHandleNullInput()
    {
        // Act
        Distance.DistanceParse(null, out var min, out var max);

        // Assert
        min.Should().BeNull();
        max.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not a distance")]
    [InlineData("100")]
    [InlineData("distance")]
    public void DistanceParse_ShouldReturnNull_WhenInputIsInvalid(string input)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().BeNull();
        max.Should().BeNull();
    }

    [Theory]
    [InlineData("100 unknown")]
    [InlineData("100 miles")]
    [InlineData("100 feet")]
    public void DistanceParse_ShouldReturnNull_WhenUnitIsUnknown(string input)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().BeNull();
        max.Should().BeNull();
    }

    [Fact]
    public void DistanceParseMin_ShouldReturnNull_WhenInputIsNull()
    {
        // Act
        var result = Distance.DistanceParseMin(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DistanceParseMax_ShouldReturnNull_WhenInputIsNull()
    {
        // Act
        var result = Distance.DistanceParseMax(null);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DistanceParseMinMaxKeyValue Tests

    [Theory]
    [InlineData("100 m", 100L, 101L)]
    [InlineData("1 km", 1000L, 2000L)]
    [InlineData("1.5 km", 1500L, 2500L)]
    public void DistanceParseMinMaxKeyValue_ShouldReturnKeyValuePair(
        string input,
        long expectedMin,
        long expectedMax)
    {
        // Act
        var result = Distance.DistanceParseMinMaxKeyValue(input);

        // Assert
        result.Should().NotBeNull();
        result.Value.Key.Should().Be(expectedMin);
        result.Value.Value.Should().Be(expectedMax);
    }

    [Fact]
    public void DistanceParseMinMaxKeyValue_ShouldReturnNull_WhenInputIsInvalid()
    {
        // Act
        var result = Distance.DistanceParseMinMaxKeyValue("invalid");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void DistanceParseMinMaxKeyValue_ShouldReturnNull_WhenInputIsNull()
    {
        // Act
        var result = Distance.DistanceParseMinMaxKeyValue(null);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Boundary Tests

    [Theory]
    [InlineData("0 m", 0L, 1L)]
    [InlineData("0 km", 0L, 1000L)]
    [InlineData("0 AU", 0L)]
    public void DistanceParse_ShouldHandleZeroDistance(string input, long expectedMin, long? expectedMaxMin = null)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().Be(expectedMin);
        max.Should().NotBeNull();
        if (expectedMaxMin.HasValue)
        {
            max.Should().BeGreaterOrEqualTo(expectedMaxMin.Value);
        }
    }

    [Theory]
    [InlineData("999,999 km")]
    [InlineData("1,000,000 km")]
    [InlineData("10,000,000 km")]
    public void DistanceParse_ShouldHandleVeryLargeDistances(string input)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().NotBeNull();
        max.Should().NotBeNull();
        max.Should().BeGreaterThan(min.Value);
    }

    [Theory]
    [InlineData("0.1 m")]
    [InlineData("0.01 km")]
    [InlineData("0.001 AU")]
    public void DistanceParse_ShouldHandleVerySmallDistances(string input)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().NotBeNull();
        max.Should().NotBeNull();
    }

    #endregion

    #region Rounding Behavior Tests

    [Fact]
    public void DistanceParse_ShouldRoundDown_AsPerEveOnlineClient()
    {
        // EVE Online client always rounds down
        // This test verifies that the min value is the parsed value
        // and max is min + unit difference

        // Arrange
        var input = "123.45 km";

        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().Be(123450L); // Rounded down value
        max.Should().Be(124450L); // min + 1000 (1 km)
    }

    [Theory]
    [InlineData("1.9 km", 1900L, 2900L)]
    [InlineData("9.9 km", 9900L, 10900L)]
    [InlineData("99.9 km", 99900L, 100900L)]
    public void DistanceParse_ShouldConsistentlyRoundDown(
        string input,
        long expectedMin,
        long expectedMax)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().Be(expectedMin);
        max.Should().Be(expectedMax);
    }

    #endregion

    #region Real-World EVE Online Scenarios

    [Theory]
    [InlineData("12 m")] // Very close range
    [InlineData("150 m")] // Activation range for many modules
    [InlineData("2,500 m")] // Common locking range
    [InlineData("5 km")] // Typical engagement range
    [InlineData("50 km")] // Long range engagement
    [InlineData("150 km")] // Very long range
    [InlineData("1.5 AU")] // Gate to gate distance
    [InlineData("14 AU")] // Typical system diameter
    public void DistanceParse_ShouldHandleCommonEveDistances(string input)
    {
        // Act
        Distance.DistanceParse(input, out var min, out var max);

        // Assert
        min.Should().NotBeNull();
        max.Should().NotBeNull();
        max.Should().BeGreaterThan(min.Value);
    }

    #endregion
}
