namespace Sanderling.Tests.Fixtures;

/// <summary>
/// Provides sample data for testing parsers.
/// Contains realistic EVE Online data in various formats.
/// </summary>
public static class SampleData
{
    #region Number Format Samples

    /// <summary>
    /// Sample numbers in different locale formats
    /// </summary>
    public static class Numbers
    {
        // US/UK Format (comma as group separator, period as decimal)
        public static readonly string[] USFormat = new[]
        {
            "1,234.56",
            "10,000.00",
            "1,234,567.89",
            "999,999,999.99"
        };

        // German/Spanish Format (period as group separator, comma as decimal)
        public static readonly string[] GermanFormat = new[]
        {
            "1.234,56",
            "10.000,00",
            "1.234.567,89",
            "999.999.999,99"
        };

        // French Format (space as group separator, comma as decimal)
        public static readonly string[] FrenchFormat = new[]
        {
            "1 234,56",
            "10 000,00",
            "1 234 567,89",
            "999 999 999,99"
        };

        // Swiss Format (apostrophe as group separator, period as decimal)
        public static readonly string[] SwissFormat = new[]
        {
            "1'234.56",
            "10'000.00",
            "1'234'567.89",
            "999'999'999.99"
        };

        // Edge cases
        public static readonly string[] EdgeCases = new[]
        {
            "0",
            "0.0",
            "0.00",
            "-1",
            "-1.5",
            "+1",
            "+1.5",
            "  123  ",
            "- 100",
            "+  50"
        };

        // Invalid numbers
        public static readonly string[] Invalid = new[]
        {
            null,
            "",
            "   ",
            "abc",
            "not a number",
            "12.34.56",
            "1,2,3"
        };
    }

    #endregion

    #region Distance Format Samples

    /// <summary>
    /// Sample distances in EVE Online format
    /// </summary>
    public static class Distances
    {
        public static readonly string[] Meters = new[]
        {
            "0 m",
            "1 m",
            "100 m",
            "500 m",
            "1,000 m",
            "9,999 m"
        };

        public static readonly string[] Kilometers = new[]
        {
            "1 km",
            "5 km",
            "10 km",
            "50 km",
            "100 km",
            "1,000 km",
            "10,000 km"
        };

        public static readonly string[] DecimalKilometers = new[]
        {
            "1.5 km",
            "2.75 km",
            "10.25 km",
            "99.9 km",
            "1,234.5 km"
        };

        public static readonly string[] AstronomicalUnits = new[]
        {
            "1 AU",
            "1.5 AU",
            "2 AU",
            "5.2 AU",
            "10 AU",
            "14 AU"
        };

        // Typical engagement ranges
        public static readonly string[] EngagementRanges = new[]
        {
            "12 m",        // Very close
            "150 m",       // Module activation range
            "2,500 m",     // Lock range
            "5 km",        // Close engagement
            "25 km",       // Medium range
            "50 km",       // Long range
            "150 km"       // Extreme range
        };

        public static readonly string[] Invalid = new[]
        {
            null,
            "",
            "100",
            "100 miles",
            "invalid",
            "km 100"
        };
    }

    #endregion

    #region Inventory Capacity Samples

    /// <summary>
    /// Sample inventory capacity gauges
    /// </summary>
    public static class InventoryCapacity
    {
        public static readonly string[] Empty = new[]
        {
            "0 / 50 m³",           // Empty frigate
            "0 / 400 m³",          // Empty cruiser
            "0 / 5,000 m³",        // Empty battleship
            "0 / 62,500 m³",       // Empty industrial
            "0 / 1,200,000 m³"     // Empty freighter
        };

        public static readonly string[] PartiallyFilled = new[]
        {
            "25 / 50 m³",
            "200 / 400 m³",
            "2,500 / 5,000 m³",
            "30,000 / 62,500 m³",
            "500,000 / 1,200,000 m³"
        };

        public static readonly string[] Full = new[]
        {
            "50 / 50 m³",
            "400 / 400 m³",
            "5,000 / 5,000 m³",
            "62,500 / 62,500 m³",
            "1,200,000 / 1,200,000 m³"
        };

        public static readonly string[] WithSelection = new[]
        {
            "(10) 25 / 50 m³",
            "(100) 200 / 400 m³",
            "(500) 2,500 / 5,000 m³",
            "(1,000) 30,000 / 62,500 m³"
        };

        public static readonly string[] SpecializedHolds = new[]
        {
            "0 / 50 m³",           // Drone bay
            "0 / 150,000 m³",      // Large ore hold
            "0 / 28,000 m³",       // Ship maintenance bay
            "0 / 1,000 m³",        // Fleet hangar
            "0 / 5,000 m³"         // Corporate hangar
        };

        public static readonly string[] DifferentLocales = new[]
        {
            "1.234,56 / 10.000 m³",    // German
            "1,234.56 / 10,000 m³",    // US
            "1 234,56 / 10 000 m³",    // French
            "1'234.56 / 10'000 m³"     // Swiss
        };

        public static readonly string[] Invalid = new[]
        {
            null,
            "",
            "100",
            "100 liters",
            "invalid m³"
        };
    }

    #endregion

    #region Ship Names and Types

    /// <summary>
    /// Sample ship names and types
    /// </summary>
    public static class ShipLabels
    {
        public static readonly string[] ValidLabels = new[]
        {
            "Noctis (Noctis)",
            "My Drake (Drake)",
            "Brutix Navy Issue (Brutix Navy Issue)",
            "Hyperion (Hyperion)",
            "Raven (Raven)",
            "Caracal (Caracal)",
            "Prophecy (Prophecy)",
            "Harbinger Navy Issue (Harbinger Navy Issue)"
        };

        public static readonly string[] InvalidLabels = new[]
        {
            "Just a label",
            "Missing (closing",
            "Missing opening)",
            "",
            null
        };
    }

    #endregion

    #region Overview Entry Data

    /// <summary>
    /// Sample overview entry data
    /// </summary>
    public static class OverviewEntries
    {
        public static readonly (string Name, string Type, string Distance)[] RatEntries = new[]
        {
            ("Guristas Eliminator", "Frigate", "12.5 km"),
            ("Guristas Phalanx", "Cruiser", "25 km"),
            ("Guristas Obliterator", "Battleship", "50 km"),
            ("Pithi Wrecker", "Frigate", "8.2 km"),
            ("Pithum Killer", "Cruiser", "30 km")
        };

        public static readonly (string Name, string Type, string Distance)[] PlayerEntries = new[]
        {
            ("Player1", "Capsule", "150 m"),
            ("Player2", "Rifter", "2,500 m"),
            ("Player3", "Stabber", "15 km"),
            ("Player4", "Tempest", "75 km"),
            ("Player5", "Nyx", "150 km")
        };

        public static readonly (string Name, string Type, string Distance)[] StructureEntries = new[]
        {
            ("Jita 4-4 CNAP", "Station", "1.5 AU"),
            ("Stargate (Perimeter)", "Stargate", "12 AU"),
            ("Customs Office", "Customs Office", "5.2 AU"),
            ("Astrahus", "Citadel", "3 AU")
        };
    }

    #endregion

    #region EWar Effect Hints

    /// <summary>
    /// Sample EWar effect hint texts
    /// </summary>
    public static class EWarHints
    {
        public static readonly string[] ECM = new[]
        {
            "jamming me",
            "Jamming Me",
            "target jamming me",
            "is jamming me now"
        };

        public static readonly string[] WarpDisrupt = new[]
        {
            "warp disrupt me",
            "warp disrupting me",
            "Warp Disrupt Me",
            "attempting to warp disrupt me"
        };

        public static readonly string[] WarpScramble = new[]
        {
            "warp scramble me",
            "warp scrambling me",
            "Warp Scramble Me",
            "is warp scrambling me"
        };

        public static readonly string[] Web = new[]
        {
            "web me",
            "webbing me",
            "Web Me",
            "stasis web me"
        };

        public static readonly string[] Unknown = new[]
        {
            "unknown effect me",
            "some other effect",
            "",
            null
        };
    }

    #endregion

    #region Roman Numerals

    /// <summary>
    /// Sample Roman numerals for testing
    /// </summary>
    public static class RomanNumerals
    {
        public static readonly (string Roman, int Value)[] Valid = new[]
        {
            ("I", 1),
            ("II", 2),
            ("III", 3),
            ("IV", 4),
            ("V", 5),
            ("IX", 9),
            ("X", 10),
            ("XIV", 14),
            ("XIX", 19),
            ("L", 50),
            ("C", 100),
            ("D", 500),
            ("M", 1000),
            ("MCMXCIV", 1994)
        };

        public static readonly string[] Invalid = new[]
        {
            "IIII",
            "VV",
            "LL",
            "DD",
            "ABC",
            "IXI",
            "",
            null
        };
    }

    #endregion

    #region Cargo Space Types

    /// <summary>
    /// Sample cargo space type labels
    /// </summary>
    public static class CargoSpaceTypes
    {
        public static readonly (string Label, ShipCargoSpaceTypeEnum? Type)[] ValidTypes = new[]
        {
            ("Drone Bay", ShipCargoSpaceTypeEnum.DroneBay),
            ("Ore Hold", ShipCargoSpaceTypeEnum.OreHold),
            ("drone bay", ShipCargoSpaceTypeEnum.DroneBay),
            ("ore hold", ShipCargoSpaceTypeEnum.OreHold)
        };

        public static readonly string[] InvalidTypes = new[]
        {
            "Unknown Bay",
            "Cargo Hold",
            "",
            null
        };
    }

    #endregion
}
