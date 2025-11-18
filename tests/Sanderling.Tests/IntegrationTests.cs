using System.Globalization;
using Sanderling.Parse;
using Sanderling.Tests.Fixtures;

namespace Sanderling.Tests;

/// <summary>
/// Integration tests that verify parser components work together correctly
/// with realistic EVE Online data scenarios.
/// </summary>
public class IntegrationTests
{
    #region Number Parser Integration Tests

    [Theory]
    [MemberData(nameof(GetAllNumberFormats))]
    public void NumberParser_ShouldHandleAllSupportedLocales(string numberString)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(numberString);

        // Assert
        result.Should().NotBeNull($"Failed to parse '{numberString}'");
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    public static IEnumerable<object[]> GetAllNumberFormats()
    {
        return SampleData.Numbers.USFormat
            .Concat(SampleData.Numbers.GermanFormat)
            .Concat(SampleData.Numbers.FrenchFormat)
            .Concat(SampleData.Numbers.SwissFormat)
            .Where(s => s != null)
            .Select(s => new object[] { s });
    }

    [Theory]
    [MemberData(nameof(GetInvalidNumbers))]
    public void NumberParser_ShouldGracefullyHandleInvalidInput(string numberString)
    {
        // Act
        var result = Number.NumberParseDecimalMilli(numberString);

        // Assert
        result.Should().BeNull($"Expected null for invalid input '{numberString}'");
    }

    public static IEnumerable<object[]> GetInvalidNumbers()
    {
        return SampleData.Numbers.Invalid
            .Select(s => new object[] { s });
    }

    #endregion

    #region Distance Parser Integration Tests

    [Theory]
    [MemberData(nameof(GetAllValidDistances))]
    public void DistanceParser_ShouldHandleAllDistanceFormats(string distanceString)
    {
        // Act
        var min = Distance.DistanceParseMin(distanceString);
        var max = Distance.DistanceParseMax(distanceString);

        // Assert
        min.Should().NotBeNull($"Failed to parse min distance from '{distanceString}'");
        max.Should().NotBeNull($"Failed to parse max distance from '{distanceString}'");
        max.Should().BeGreaterThan(min.Value, "Max distance should be greater than min");
    }

    public static IEnumerable<object[]> GetAllValidDistances()
    {
        return SampleData.Distances.Meters
            .Concat(SampleData.Distances.Kilometers)
            .Concat(SampleData.Distances.DecimalKilometers)
            .Concat(SampleData.Distances.AstronomicalUnits)
            .Concat(SampleData.Distances.EngagementRanges)
            .Select(s => new object[] { s });
    }

    [Theory]
    [MemberData(nameof(GetInvalidDistances))]
    public void DistanceParser_ShouldHandleInvalidDistances(string distanceString)
    {
        // Act
        var result = Distance.DistanceParseMin(distanceString);

        // Assert
        result.Should().BeNull($"Expected null for invalid distance '{distanceString}'");
    }

    public static IEnumerable<object[]> GetInvalidDistances()
    {
        return SampleData.Distances.Invalid
            .Select(s => new object[] { s });
    }

    #endregion

    #region Inventory Parser Integration Tests

    [Theory]
    [MemberData(nameof(GetAllInventoryCapacities))]
    public void InventoryParser_ShouldHandleAllCapacityFormats(string capacityString)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(capacityString);

        // Assert
        result.Should().NotBeNull($"Failed to parse capacity '{capacityString}'");
        result.Used.Should().NotBeNull();
        result.Used.Should().BeGreaterThanOrEqualTo(0);

        if (result.Max.HasValue)
        {
            result.Max.Should().BeGreaterThanOrEqualTo(0);
            result.Used.Should().BeLessThanOrEqualTo(result.Max.Value, "Used should not exceed max");
        }

        if (result.Selected.HasValue)
        {
            result.Selected.Should().BeGreaterThanOrEqualTo(0);
            result.Selected.Should().BeLessThanOrEqualTo(result.Used.Value, "Selected should not exceed used");
        }
    }

    public static IEnumerable<object[]> GetAllInventoryCapacities()
    {
        return SampleData.InventoryCapacity.Empty
            .Concat(SampleData.InventoryCapacity.PartiallyFilled)
            .Concat(SampleData.InventoryCapacity.Full)
            .Concat(SampleData.InventoryCapacity.WithSelection)
            .Concat(SampleData.InventoryCapacity.SpecializedHolds)
            .Concat(SampleData.InventoryCapacity.DifferentLocales)
            .Select(s => new object[] { s });
    }

    [Theory]
    [MemberData(nameof(GetInvalidInventoryCapacities))]
    public void InventoryParser_ShouldHandleInvalidCapacities(string capacityString)
    {
        // Act
        var result = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(capacityString);

        // Assert
        result.Should().BeNull($"Expected null for invalid capacity '{capacityString}'");
    }

    public static IEnumerable<object[]> GetInvalidInventoryCapacities()
    {
        return SampleData.InventoryCapacity.Invalid
            .Select(s => new object[] { s });
    }

    #endregion

    #region Ship Label Integration Tests

    [Theory]
    [MemberData(nameof(GetValidShipLabels))]
    public void ShipLabelParser_ShouldParseValidLabels(string label)
    {
        // Act
        var result = InventoryExtension.ParseTreeEntryLabelShipNameAndType(label);

        // Assert
        result.Should().NotBeNull($"Failed to parse ship label '{label}'");
        result.Value.Key.Should().NotBeNullOrEmpty("Ship name should not be empty");
        result.Value.Value.Should().NotBeNullOrEmpty("Ship type should not be empty");
    }

    public static IEnumerable<object[]> GetValidShipLabels()
    {
        return SampleData.ShipLabels.ValidLabels
            .Select(s => new object[] { s });
    }

    [Theory]
    [MemberData(nameof(GetInvalidShipLabels))]
    public void ShipLabelParser_ShouldHandleInvalidLabels(string label)
    {
        // Act
        var result = InventoryExtension.ParseTreeEntryLabelShipNameAndType(label);

        // Assert
        result.Should().BeNull($"Expected null for invalid ship label '{label}'");
    }

    public static IEnumerable<object[]> GetInvalidShipLabels()
    {
        return SampleData.ShipLabels.InvalidLabels
            .Select(s => new object[] { s });
    }

    #endregion

    #region EWar Detection Integration Tests

    [Theory]
    [InlineData("ECM")]
    [InlineData("WarpDisrupt")]
    [InlineData("WarpScramble")]
    [InlineData("Web")]
    public void EWarDetection_ShouldIdentifyAllEWarTypes(string ewarType)
    {
        // Arrange
        var hints = ewarType switch
        {
            "ECM" => SampleData.EWarHints.ECM,
            "WarpDisrupt" => SampleData.EWarHints.WarpDisrupt,
            "WarpScramble" => SampleData.EWarHints.WarpScramble,
            "Web" => SampleData.EWarHints.Web,
            _ => throw new ArgumentException($"Unknown EWar type: {ewarType}")
        };

        foreach (var hint in hints.Where(h => !string.IsNullOrEmpty(h)))
        {
            var sprite = new TestSprite { HintText = hint };

            // Act
            var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(sprite);

            // Assert
            result.Should().NotBeNull($"Failed to detect EWar type from hint '{hint}'");
            result.Should().NotBe(EWarTypeEnum.None, $"Hint '{hint}' should map to a specific EWar type");
        }
    }

    #endregion

    #region Overview Entry Parsing Integration Tests

    [Fact]
    public void OverviewEntry_ShouldParseCompleteRatEntry()
    {
        // Arrange
        var entry = SampleData.OverviewEntries.RatEntries[0];
        var listEntry = CreateTestListEntry(entry.Name, entry.Type, entry.Distance);

        // Act
        var parsed = ListEntryExtension.ParseAsListEntry(listEntry);

        // Assert
        parsed.Should().NotBeNull();
        parsed.Name.Should().Be(entry.Name);
        parsed.Type.Should().Be(entry.Type);
        parsed.DistanceMin.Should().NotBeNull();
        parsed.DistanceMax.Should().NotBeNull();
        parsed.DistanceMax.Should().BeGreaterThan(parsed.DistanceMin.Value);
    }

    [Fact]
    public void OverviewEntry_ShouldParseAllPlayerEntries()
    {
        // Arrange & Act & Assert
        foreach (var entry in SampleData.OverviewEntries.PlayerEntries)
        {
            var listEntry = CreateTestListEntry(entry.Name, entry.Type, entry.Distance);
            var parsed = ListEntryExtension.ParseAsListEntry(listEntry);

            parsed.Should().NotBeNull($"Failed to parse player entry: {entry}");
            parsed.Name.Should().Be(entry.Name);
            parsed.Type.Should().Be(entry.Type);
            parsed.DistanceMin.Should().NotBeNull();
        }
    }

    [Fact]
    public void OverviewEntry_ShouldParseStructureEntriesWithAU()
    {
        // Arrange & Act & Assert
        foreach (var entry in SampleData.OverviewEntries.StructureEntries)
        {
            var listEntry = CreateTestListEntry(entry.Name, entry.Type, entry.Distance);
            var parsed = ListEntryExtension.ParseAsListEntry(listEntry);

            parsed.Should().NotBeNull($"Failed to parse structure entry: {entry}");
            parsed.Name.Should().Be(entry.Name);
            parsed.Type.Should().Be(entry.Type);
            parsed.DistanceMin.Should().NotBeNull();

            // Structure distances are typically in AU, which should be very large
            if (entry.Distance.Contains("AU"))
            {
                parsed.DistanceMin.Should().BeGreaterThan(1_000_000L, "AU distances should be very large");
            }
        }
    }

    #endregion

    #region Cross-Parser Integration Tests

    [Fact]
    public void IntegrationTest_CompleteInventoryScenario()
    {
        // This test simulates parsing a complete inventory window

        // Arrange - Ship in inventory
        var shipLabel = "Hyperion (Hyperion)";
        var cargoCapacity = "(500) 2,500 / 5,000 m³";
        var droneBayCapacity = "25 / 50 m³";

        // Act - Parse ship name/type
        var shipInfo = InventoryExtension.ParseTreeEntryLabelShipNameAndType(shipLabel);

        // Parse cargo capacity
        var cargo = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(cargoCapacity);

        // Parse drone bay
        var droneBay = InventoryExtension.ParseAsInventoryCapacityGaugeMilli(droneBayCapacity);

        // Assert
        shipInfo.Should().NotBeNull();
        shipInfo.Value.Key.Should().Be("Hyperion");
        shipInfo.Value.Value.Should().Be("Hyperion");

        cargo.Should().NotBeNull();
        cargo.Used.Should().Be(2_500_000L);
        cargo.Max.Should().Be(5_000_000L);
        cargo.Selected.Should().Be(500_000L);

        droneBay.Should().NotBeNull();
        droneBay.Used.Should().Be(25_000L);
        droneBay.Max.Should().Be(50_000L);
    }

    [Fact]
    public void IntegrationTest_CompleteCombatScenario()
    {
        // This test simulates parsing a complete combat overview

        // Arrange - Multiple targets at different ranges with different effects
        var target1 = ("Guristas Eliminator", "Frigate", "12.5 km");
        var target2 = ("Guristas Phalanx", "Cruiser", "25 km");
        var ewarEffect = "warp scramble me";

        // Act - Parse targets
        var entry1 = ListEntryExtension.ParseAsListEntry(CreateTestListEntry(target1.Item1, target1.Item2, target1.Item3));
        var entry2 = ListEntryExtension.ParseAsListEntry(CreateTestListEntry(target2.Item1, target2.Item2, target2.Item3));

        // Parse EWar effect
        var sprite = new TestSprite { HintText = ewarEffect };
        var ewar = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(sprite);

        // Assert
        entry1.Should().NotBeNull();
        entry1.Name.Should().Be(target1.Item1);
        entry1.DistanceMin.Should().Be(12_500L);

        entry2.Should().NotBeNull();
        entry2.Name.Should().Be(target2.Item1);
        entry2.DistanceMin.Should().Be(25_000L);

        ewar.Should().Be(EWarTypeEnum.WarpScramble);

        // Verify target ordering by distance
        entry1.DistanceMin.Should().BeLessThan(entry2.DistanceMin.Value, "Closer target should have smaller distance");
    }

    #endregion

    #region Helper Methods

    private static TestListEntry CreateTestListEntry(string name, string type, string distance)
    {
        return new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Name" }, name),
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Type" }, type),
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Distance" }, distance)
            }
        };
    }

    #endregion

    #region Test Helper Classes

    private class TestListEntry : Sanderling.Interface.MemoryStruct.IListEntry
    {
        public KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>[] ListColumnCellLabel { get; set; }
        public IEnumerable<Sanderling.Interface.MemoryStruct.IUIElementText> LabelText { get; set; }
        public IEnumerable<Sanderling.Interface.MemoryStruct.IUIElementText> ButtonText { get; set; }
        public int? ChildLastInTreeIndex { get; set; }
        public int? ContentBoundLeft { get; set; }
        public Sanderling.Interface.MemoryStruct.IUIElement GroupExpander { get; set; }
        public long Id { get; set; }
        public IEnumerable<Sanderling.Interface.MemoryStruct.IUIElementInputText> InputText { get; set; }
        public int? InTreeIndex { get; set; }
        public bool? IsExpanded { get; set; }
        public bool? IsGroup { get; set; }
        public bool? IsSelected { get; set; }
        public BotEngine.Interface.ColorORGB[] ListBackgroundColor { get; set; }
        public Bib3.Geometrik.RectInt? Region { get; set; }
        public Sanderling.Interface.MemoryStruct.IUIElement RegionInteraction { get; set; }
        public Sanderling.Interface.MemoryStruct.ISprite[] SetSprite { get; set; }
        public IEnumerable<Sanderling.Interface.MemoryStruct.ISprite> Sprite { get; set; }
    }

    private class TestColumnHeader : Sanderling.Interface.MemoryStruct.IColumnHeader
    {
        public string Text { get; set; }
        public Bib3.Geometrik.RectInt? Region { get; set; }
        public int? InTreeIndex { get; set; }
        public Sanderling.Interface.MemoryStruct.IUIElement RegionInteraction { get; set; }
        public long Id { get; set; }
        public int? ChildLastInTreeIndex { get; set; }
    }

    private class TestSprite : Sanderling.Interface.MemoryStruct.ISprite
    {
        public string HintText { get; set; }
        public string Name { get; set; }
        public Bib3.Geometrik.RectInt? Region { get; set; }
        public int? InTreeIndex { get; set; }
        public Sanderling.Interface.MemoryStruct.IUIElement RegionInteraction { get; set; }
        public long Id { get; set; }
        public int? ChildLastInTreeIndex { get; set; }
        public BotEngine.Interface.ColorORGB? Color { get; set; }
        public string TextureId { get; set; }
    }

    #endregion
}
