using Sanderling.Parse;

namespace Sanderling.Tests;

/// <summary>
/// Tests for overview entry parsing functionality.
/// Tests EWar type detection and icon hint parsing.
/// </summary>
public class OverviewParserTests
{
    #region EWar Type Detection Tests

    [Theory]
    [InlineData("jamming me", EWarTypeEnum.ECM)]
    [InlineData("Jamming Me", EWarTypeEnum.ECM)]
    [InlineData("JAMMING ME", EWarTypeEnum.ECM)]
    [InlineData("target jamming me", EWarTypeEnum.ECM)]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldDetectECM(string hintText, EWarTypeEnum expected)
    {
        // Arrange
        var icon = new TestSprite { HintText = hintText };

        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(icon);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("warp disrupt me", EWarTypeEnum.WarpDisrupt)]
    [InlineData("warp disrupting me", EWarTypeEnum.WarpDisrupt)]
    [InlineData("Warp Disrupt Me", EWarTypeEnum.WarpDisrupt)]
    [InlineData("WARP DISRUPT ME", EWarTypeEnum.WarpDisrupt)]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldDetectWarpDisrupt(string hintText, EWarTypeEnum expected)
    {
        // Arrange
        var icon = new TestSprite { HintText = hintText };

        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(icon);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("warp scramble me", EWarTypeEnum.WarpScramble)]
    [InlineData("warp scrambling me", EWarTypeEnum.WarpScramble)]
    [InlineData("Warp Scramble Me", EWarTypeEnum.WarpScramble)]
    [InlineData("WARP SCRAMBLE ME", EWarTypeEnum.WarpScramble)]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldDetectWarpScramble(string hintText, EWarTypeEnum expected)
    {
        // Arrange
        var icon = new TestSprite { HintText = hintText };

        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(icon);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("web me", EWarTypeEnum.Web)]
    [InlineData("webbing me", EWarTypeEnum.Web)]
    [InlineData("Web Me", EWarTypeEnum.Web)]
    [InlineData("WEB ME", EWarTypeEnum.Web)]
    [InlineData("stasis web me", EWarTypeEnum.Web)]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldDetectWeb(string hintText, EWarTypeEnum expected)
    {
        // Arrange
        var icon = new TestSprite { HintText = hintText };

        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(icon);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("unknown effect me")]
    [InlineData("some other effect")]
    [InlineData("tracking disrupting me")]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldDetectOther_WhenTypeIsUnknown(string hintText)
    {
        // Arrange
        var icon = new TestSprite { HintText = hintText };

        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(icon);

        // Assert
        result.Should().Be(EWarTypeEnum.Other);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldReturnNull_WhenIconIsNull()
    {
        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldReturnOther_WhenHintTextIsNull()
    {
        // Arrange
        var icon = new TestSprite { HintText = null };

        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(icon);

        // Assert
        result.Should().Be(EWarTypeEnum.Other);
    }

    [Fact]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldReturnOther_WhenHintTextIsEmpty()
    {
        // Arrange
        var icon = new TestSprite { HintText = "" };

        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(icon);

        // Assert
        result.Should().Be(EWarTypeEnum.Other);
    }

    #endregion

    #region Pattern Matching Tests

    [Theory]
    [InlineData("target jamming me now")]
    [InlineData("is jamming me")]
    [InlineData("jamming me actively")]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldUseRegexMatching(string hintText)
    {
        // Arrange
        var icon = new TestSprite { HintText = hintText };

        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(icon);

        // Assert
        result.Should().Be(EWarTypeEnum.ECM);
    }

    [Theory]
    [InlineData("attempting to warp disrupt me")]
    [InlineData("is warp disrupting me")]
    [InlineData("warp disruption on me")]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldMatchPartialPatterns(string hintText)
    {
        // Arrange
        var icon = new TestSprite { HintText = hintText };

        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(icon);

        // Assert
        result.Should().Be(EWarTypeEnum.WarpDisrupt);
    }

    #endregion

    #region Real-World EVE Online Scenarios

    [Fact]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldHandleMultipleEffects()
    {
        // In reality, icons appear separately for each effect
        // This test ensures individual icons are parsed correctly

        // Arrange
        var ecmIcon = new TestSprite { HintText = "jamming me" };
        var webIcon = new TestSprite { HintText = "web me" };
        var warpDisruptIcon = new TestSprite { HintText = "warp disrupt me" };

        // Act
        var ecmResult = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(ecmIcon);
        var webResult = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(webIcon);
        var warpDisruptResult = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(warpDisruptIcon);

        // Assert
        ecmResult.Should().Be(EWarTypeEnum.ECM);
        webResult.Should().Be(EWarTypeEnum.Web);
        warpDisruptResult.Should().Be(EWarTypeEnum.WarpDisrupt);
    }

    [Theory]
    [InlineData("warp scramble me", "warp disrupt me")]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldDistinguishSimilarEffects(
        string scrambleHint,
        string disruptHint)
    {
        // Arrange
        var scrambleIcon = new TestSprite { HintText = scrambleHint };
        var disruptIcon = new TestSprite { HintText = disruptHint };

        // Act
        var scrambleResult = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(scrambleIcon);
        var disruptResult = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(disruptIcon);

        // Assert
        scrambleResult.Should().Be(EWarTypeEnum.WarpScramble);
        disruptResult.Should().Be(EWarTypeEnum.WarpDisrupt);
        scrambleResult.Should().NotBe(disruptResult);
    }

    #endregion

    #region Whitespace and Formatting Tests

    [Theory]
    [InlineData("  jamming me  ")]
    [InlineData("\tjamming me\t")]
    [InlineData("jamming  me")]
    public void EWarTypeFromOverviewEntryRightIcon_ShouldHandleWhitespace(string hintText)
    {
        // Arrange
        var icon = new TestSprite { HintText = hintText };

        // Act
        var result = OverviewExtension.EWarTypeFromOverviewEntryRightIcon(icon);

        // Assert
        result.Should().Be(EWarTypeEnum.ECM);
    }

    #endregion

    #region Test Helper Classes

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
