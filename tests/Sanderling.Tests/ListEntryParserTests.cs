using Sanderling.Parse;

namespace Sanderling.Tests;

/// <summary>
/// Tests for list entry parsing functionality.
/// Tests column value extraction and distance parsing for list entries.
/// </summary>
public class ListEntryParserTests
{
    #region Column Value Extraction Tests

    [Fact]
    public void CellValueFromColumnHeader_ShouldExtractValue_WhenHeaderMatches()
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Name" }, "Target Ship"),
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Type" }, "Cruiser"),
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Distance" }, "15 km")
            }
        };

        // Act
        var result = ListEntryExtension.CellValueFromColumnHeader(listEntry, "Name");

        // Assert
        result.Should().Be("Target Ship");
    }

    [Fact]
    public void CellValueFromColumnHeader_ShouldReturnNull_WhenHeaderDoesNotMatch()
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Name" }, "Target Ship")
            }
        };

        // Act
        var result = ListEntryExtension.CellValueFromColumnHeader(listEntry, "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CellValueFromColumnHeader_ShouldBeCaseInsensitive()
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Name" }, "Target Ship")
            }
        };

        // Act
        var result = ListEntryExtension.CellValueFromColumnHeader(listEntry, "name");

        // Assert
        result.Should().Be("Target Ship");
    }

    #endregion

    #region Specific Column Tests

    [Fact]
    public void ColumnTypeValue_ShouldExtractTypeColumn()
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Type" }, "Battleship")
            }
        };

        // Act
        var result = ListEntryExtension.ColumnTypeValue(listEntry);

        // Assert
        result.Should().Be("Battleship");
    }

    [Fact]
    public void ColumnNameValue_ShouldExtractNameColumn()
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Name" }, "Enemy Target")
            }
        };

        // Act
        var result = ListEntryExtension.ColumnNameValue(listEntry);

        // Assert
        result.Should().Be("Enemy Target");
    }

    [Fact]
    public void ColumnDistanceValue_ShouldExtractDistanceColumn()
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Distance" }, "25 km")
            }
        };

        // Act
        var result = ListEntryExtension.ColumnDistanceValue(listEntry);

        // Assert
        result.Should().Be("25 km");
    }

    #endregion

    #region No Item Detection Tests

    [Theory]
    [InlineData("no item")]
    [InlineData("No Item")]
    [InlineData("NO ITEM")]
    [InlineData("No  Item")]
    public void IsNoItem_ShouldReturnTrue_WhenTextIndicatesNoItem(string text)
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            LabelText = new[] { new TestUIElementText { Text = text } }
        };

        // Act
        var result = ListEntryExtension.IsNoItem(listEntry);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("Target Ship")]
    [InlineData("Wreck")]
    [InlineData("Container")]
    public void IsNoItem_ShouldReturnFalse_WhenTextDoesNotIndicateNoItem(string text)
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            LabelText = new[] { new TestUIElementText { Text = text } }
        };

        // Act
        var result = ListEntryExtension.IsNoItem(listEntry);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsNoItem_ShouldReturnFalse_WhenLabelTextIsNull()
    {
        // Arrange
        var listEntry = new TestListEntry { LabelText = null };

        // Act
        var result = ListEntryExtension.IsNoItem(listEntry);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ParseAsListEntry Tests

    [Fact]
    public void ParseAsListEntry_ShouldReturnNull_WhenInputIsNull()
    {
        // Act
        var result = ListEntryExtension.ParseAsListEntry(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseAsListEntry_ShouldParseDistance()
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Distance" }, "10 km")
            }
        };

        // Act
        var result = ListEntryExtension.ParseAsListEntry(listEntry);

        // Assert
        result.Should().NotBeNull();
        result.DistanceMin.Should().Be(10000L);
        result.DistanceMax.Should().Be(11000L);
    }

    [Fact]
    public void ParseAsListEntry_ShouldParseTypeAndName()
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Name" }, "Hostile Ship"),
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Type" }, "Frigate")
            }
        };

        // Act
        var result = ListEntryExtension.ParseAsListEntry(listEntry);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Hostile Ship");
        result.Type.Should().Be("Frigate");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CellValueFromColumnHeader_ShouldHandleNullColumnHeaders()
    {
        // Arrange
        var listEntry = new TestListEntry { ListColumnCellLabel = null };

        // Act
        var result = ListEntryExtension.CellValueFromColumnHeader(listEntry, "Name");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CellValueFromColumnHeader_ShouldHandleEmptyColumnHeaders()
    {
        // Arrange
        var listEntry = new TestListEntry { ListColumnCellLabel = Array.Empty<KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>>() };

        // Act
        var result = ListEntryExtension.CellValueFromColumnHeader(listEntry, "Name");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ParseAsListEntry_ShouldHandleInvalidDistance()
    {
        // Arrange
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Distance" }, "invalid")
            }
        };

        // Act
        var result = ListEntryExtension.ParseAsListEntry(listEntry);

        // Assert
        result.Should().NotBeNull();
        result.DistanceMin.Should().BeNull();
        result.DistanceMax.Should().BeNull();
    }

    #endregion

    #region Real-World EVE Online Scenarios

    [Fact]
    public void ParseAsListEntry_ShouldHandleOverviewEntry()
    {
        // Arrange - Typical overview entry
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Name" }, "Guristas Eliminator"),
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Type" }, "Frigate"),
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Distance" }, "12.5 km")
            }
        };

        // Act
        var result = ListEntryExtension.ParseAsListEntry(listEntry);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Guristas Eliminator");
        result.Type.Should().Be("Frigate");
        result.DistanceMin.Should().Be(12500L);
    }

    [Fact]
    public void ParseAsListEntry_ShouldHandleProbeResultEntry()
    {
        // Arrange - Typical probe result entry
        var listEntry = new TestListEntry
        {
            ListColumnCellLabel = new[]
            {
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Name" }, "Combat Site"),
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Type" }, "Cosmic Signature"),
                new KeyValuePair<Sanderling.Interface.MemoryStruct.IColumnHeader, string>(
                    new TestColumnHeader { Text = "Distance" }, "5.2 AU")
            }
        };

        // Act
        var result = ListEntryExtension.ParseAsListEntry(listEntry);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Combat Site");
        result.Type.Should().Be("Cosmic Signature");
        result.DistanceMin.Should().NotBeNull();
        result.DistanceMin.Should().BeGreaterThan(0);
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

    private class TestUIElementText : Sanderling.Interface.MemoryStruct.IUIElementText
    {
        public string Text { get; set; }
        public Bib3.Geometrik.RectInt? Region { get; set; }
        public int? InTreeIndex { get; set; }
        public Sanderling.Interface.MemoryStruct.IUIElement RegionInteraction { get; set; }
        public long Id { get; set; }
        public int? ChildLastInTreeIndex { get; set; }
    }

    #endregion
}
