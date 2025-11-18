using System.Globalization;
using Sanderling.Parse;

namespace Sanderling.Tests;

/// <summary>
/// Tests for number parsing functionality across different locales.
/// Number formatting in EVE Online depends on Windows number format configuration.
/// </summary>
public class NumberParserTests
{
    #region Basic Parsing Tests

    [Theory]
    [InlineData("0", 0)]
    [InlineData("1", 1000)]
    [InlineData("10", 10000)]
    [InlineData("100", 100000)]
    [InlineData("1000", 1000000)]
    [InlineData("1,000", 1000000)]
    [InlineData("1.000", 1000000)]
    public void NumberParseDecimalMilli_ShouldParseBasicNumbers(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1.5", 1500)]
    [InlineData("1,5", 1500)]
    [InlineData("10.5", 10500)]
    [InlineData("10,5", 10500)]
    [InlineData("100.25", 100250)]
    [InlineData("100,25", 100250)]
    public void NumberParseDecimalMilli_ShouldParseDecimalNumbers(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Negative Number Tests

    [Theory]
    [InlineData("-1", -1000)]
    [InlineData("-10", -10000)]
    [InlineData("-100", -100000)]
    [InlineData("-1.5", -1500)]
    [InlineData("-1,5", -1500)]
    public void NumberParseDecimalMilli_ShouldParseNegativeNumbers(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("+1", 1000)]
    [InlineData("+10", 10000)]
    [InlineData("+1.5", 1500)]
    public void NumberParseDecimalMilli_ShouldParsePositiveSignedNumbers(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Locale-Specific Tests

    [Theory]
    [InlineData("1.234.567", 1234567000)] // German/Spanish format
    [InlineData("1,234,567", 1234567000)] // US/UK format
    [InlineData("1'234'567", 1234567000)] // Swiss format
    [InlineData("1 234 567", 1234567000)] // French format
    public void NumberParseDecimalMilli_ShouldParseNumbersWithDifferentGroupSeparators(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("1.234.567,89", 1234567890)] // German format
    [InlineData("1,234,567.89", 1234567890)] // US format
    [InlineData("1'234'567.89", 1234567890)] // Swiss format
    [InlineData("1 234 567,89", 1234567890)] // French format
    public void NumberParseDecimalMilli_ShouldParseMixedSeparators(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Large Number Tests

    [Theory]
    [InlineData("1000000", 1000000000)]
    [InlineData("1,000,000", 1000000000)]
    [InlineData("1.000.000", 1000000000)]
    [InlineData("10000000", 10000000000)]
    [InlineData("100000000", 100000000000)]
    public void NumberParseDecimalMilli_ShouldParseLargeNumbers(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("13400000", 13400000000)]
    [InlineData("13456700000", 13456700000000)]
    [InlineData("134567000000000", 134567000000000000)]
    public void NumberParseDecimalMilli_ShouldParseVeryLargeNumbers(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Precision Tests

    [Theory]
    [InlineData("1.1", 1100)]
    [InlineData("1.12", 1120)]
    [InlineData("1.1234", 1123)]
    [InlineData("1.12345", 1123)]
    [InlineData("0.001", 1)]
    [InlineData("0.0001", 0)]
    public void NumberParseDecimalMilli_ShouldHandleDecimalPrecision(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void NumberParseDecimalMilli_ShouldReturnNull_WhenInputIsNull()
    {
        // Act
        var result = Number.NumberParseDecimalMilli(null);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("not a number")]
    public void NumberParseDecimalMilli_ShouldReturnNull_WhenInputIsInvalid(string input)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("  123  ", 123000)]
    [InlineData("\t456\t", 456000)]
    [InlineData("  789.5  ", 789500)]
    public void NumberParseDecimalMilli_ShouldHandleWhitespace(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("- 100", -100000)]
    [InlineData("-  100", -100000)]
    [InlineData("+  100", 100000)]
    public void NumberParseDecimalMilli_ShouldHandleSpacesBetweenSignAndValue(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region NumberParseDecimal Tests

    [Theory]
    [InlineData("1000", 1000)]
    [InlineData("1,000", 1000)]
    [InlineData("1.5", 1)]
    [InlineData("1,5", 1)]
    public void NumberParseDecimal_ShouldDivideMilliByThousand(string input, long expected)
    {
        // Act
        var result = Number.NumberParseDecimal(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Roman Numeral Tests

    [Theory]
    [InlineData("I", 1)]
    [InlineData("II", 2)]
    [InlineData("III", 3)]
    [InlineData("IV", 4)]
    [InlineData("V", 5)]
    [InlineData("VI", 6)]
    [InlineData("IX", 9)]
    [InlineData("X", 10)]
    [InlineData("L", 50)]
    [InlineData("C", 100)]
    [InlineData("D", 500)]
    [InlineData("M", 1000)]
    public void IntFromRoman_ShouldParseBasicRomanNumerals(string input, int expected)
    {
        // Act
        var result = Number.IntFromRoman(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("XIV", 14)]
    [InlineData("XIX", 19)]
    [InlineData("XLIV", 44)]
    [InlineData("XCIX", 99)]
    [InlineData("CDXLIV", 444)]
    [InlineData("MCMXCIV", 1994)]
    public void IntFromRoman_ShouldParseComplexRomanNumerals(string input, int expected)
    {
        // Act
        var result = Number.IntFromRoman(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("i", 1)]
    [InlineData("iv", 4)]
    [InlineData("ix", 9)]
    [InlineData("mcmxciv", 1994)]
    public void IntFromRoman_ShouldBeCaseInsensitive(string input, int expected)
    {
        // Act
        var result = Number.IntFromRoman(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("N", 0)]
    public void IntFromRoman_ShouldParseZero(string input, int expected)
    {
        // Act
        var result = Number.IntFromRoman(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IntFromRoman_ShouldReturnNull_WhenInputIsNullOrEmpty(string input)
    {
        // Act
        var result = Number.IntFromRoman(input);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("IIII")] // Four I's in a row
    [InlineData("VV")] // Two V's
    [InlineData("LL")] // Two L's
    [InlineData("DD")] // Two D's
    [InlineData("ABC")] // Invalid characters
    [InlineData("IXI")] // Invalid ordering
    public void IntFromRoman_ShouldReturnNull_WhenInputIsInvalid(string input)
    {
        // Act
        var result = Number.IntFromRoman(input);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("  XIV  ", 14)]
    [InlineData("\tIX\t", 9)]
    public void IntFromRoman_ShouldHandleWhitespace(string input, int expected)
    {
        // Act
        var result = Number.IntFromRoman(input);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region NumberParseDecimalMilliBetween Tests

    [Fact]
    public void NumberParseDecimalMilliBetween_ShouldExtractNumberBetweenPatterns()
    {
        // Arrange
        var input = "Distance: 1,234.5 km";

        // Act
        var result = Number.NumberParseDecimalMilliBetween(input, "Distance:\\s*", "\\s*km");

        // Assert
        result.Should().Be(1234500);
    }

    [Fact]
    public void NumberParseDecimalMilliBetween_ShouldReturnNull_WhenPatternDoesNotMatch()
    {
        // Arrange
        var input = "Distance: unknown";

        // Act
        var result = Number.NumberParseDecimalMilliBetween(input, "Distance:\\s*", "\\s*km");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NumberParseDecimalMilliBetween_ShouldHandleNullInput()
    {
        // Act
        var result = Number.NumberParseDecimalMilliBetween(null, "prefix", "suffix");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Multi-Culture Integration Tests

    [Theory]
    [MemberData(nameof(GetMultiCultureTestData))]
    public void NumberParseDecimalMilli_ShouldParseNumbersInDifferentCultures(
        CultureInfo culture,
        long numberValue,
        string formattedString,
        long expectedMilli)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(formattedString);

        // Assert
        result.Should().Be(expectedMilli,
            $"Failed to parse '{formattedString}' for culture '{culture.Name}'");
    }

    public static IEnumerable<object[]> GetMultiCultureTestData()
    {
        var cultures = new[]
        {
            CultureInfo.GetCultureInfo("en-US"),
            CultureInfo.GetCultureInfo("de-DE"),
            CultureInfo.GetCultureInfo("fr-FR"),
            CultureInfo.GetCultureInfo("es-ES"),
            CultureInfo.GetCultureInfo("ru-RU"),
        };

        var testValues = new[] { 0L, 40L, 440L, 4000L, 4400L, 4440L, 13400000L, 13456700000L };

        foreach (var culture in cultures)
        {
            foreach (var valueMilli in testValues)
            {
                var valueDecimal = valueMilli / 1000.0;
                var formatted = valueDecimal.ToString("N2", culture);
                yield return new object[] { culture, valueMilli, formatted, valueMilli };
            }

            // Test negative values
            foreach (var valueMilli in testValues.Take(3))
            {
                var negativeValueMilli = -valueMilli;
                var valueDecimal = negativeValueMilli / 1000.0;
                var formatted = valueDecimal.ToString("N2", culture);
                yield return new object[] { culture, negativeValueMilli, formatted, negativeValueMilli };
            }
        }
    }

    #endregion
}
