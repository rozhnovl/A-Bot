using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services;

/// <summary>
/// Provides information about NPCs including DPS calculations and priority
/// </summary>
public class NpcInformationService
{
    private static readonly IReadOnlyDictionary<string, double> DpsLookup = new Dictionary<string, double>
    {
        { "Sparkneedle Tessella", 25 },
        { "Emberneedle Tessella", 25 },
        { "Strikeneedle Tessella", 25 },
        { "Blastneedle Tessella", 25 },
        { "Snarecaster Tessella", 10 },
        { "Spotlighter Tessella", 10 },
        { "Fogcaster Tessella", 10 },
        { "Gazedimmer Tessella", 10 },
        { "Sparklance Tessella", 50 },
        { "Emberlance Tessella", 50 },
        { "Strikelance Tessella", 50 },
        { "Blastlance Tessella", 50 },
        { "Fieldweaver Tessella", 0 },
        { "Plateforger Tessella", 0 },
        { "Sparkgrip Tessera", 191 },
        { "Embergrip Tessera", 191 },
        { "Strikegrip Tessera", 191 },
        { "Blastgrip Tessera", 191 },
        { "Photic Abyssal Overmind", 108.6419753 },
        { "Twilit Abyssal Overmind", 264 },
        { "Bathyic Abyssal Overmind", 375.3084112 },
        { "Hadal Abyssal Overmind", 457.5575221 },
        { "Benthic Abyssal Overmind", 594.861461 },
        { "Drifter Foothold Battleship", 100 },
        { "Drifter Rearguard Battleship", 200 },
        { "Drifter Frontline Battleship", 300 },
        { "Drifter Vanguard Battleship", 400 },
        { "Drifter Assault Battleship", 500 },
        { "Drifter Entanglement Cruiser", 40 },
        { "Drifter Nullwarp Cruiser", 40 },
        { "Drifter Nullcharge Cruiser", 40 },
        { "Ghosting Damavik", 36 },
        { "Tangling Damavik", 36 },
        { "Anchoring Damavik", 36 },
        { "Starving Damavik", 36 },
        { "Striking Damavik", 36 },
        { "Striking Vila Damavik", 49 },
        { "Tangling Vila Damavik", 49 },
        { "Anchoring Vila Damavik", 49 },
        { "Shining Vila Damavik", 49 },
        { "Blinding Vila Damavik", 49 },
        { "Ghosting Vila Damavik", 49 },
        { "Starving Vedmak", 237.6 },
        { "Harrowing Vedmak", 237.6 },
        { "Harrowing Vila Vedmak", 158.8 },
        { "Striking Leshak", 147.84 },
        { "Renewing Leshak", 147.84 },
        { "Tangling Leshak", 147.84 },
        { "Starving Leshak", 147.84 },
        { "Warding Leshak", 147.84 },
        { "Blinding Leshak", 147.84 },
        { "Lucid Escort", 24 },
        { "Lucid Warden", 20 },
        { "Lucid Aegis", 36 },
        { "Lucid Firewatcher", 30 },
        { "Lucid Preserver", 0 },
        { "Lucid Watchman", 48 },
        { "Lucid Upholder", 40 },
        { "Lucid Sentinel", 40 },
        { "Lucid Deepwatcher", 160 },
        { "Ephialtes Lancer", 30 },
        { "Ephialtes Entangler", 20 },
        { "Ephialtes Spearfisher", 20 },
        { "Ephialtes Illuminator", 20 },
        { "Ephialtes Dissipator", 20 },
        { "Ephialtes Obfuscator", 20 },
        { "Ephialtes Confuser", 20 },
        { "Vila Swarmer", 0 },
        { "Triglavian Bioadaptive Cache", 0 },
        { "Triglavian Extraction Node", 0 },
        { "Triglavian Extraction SubNode", 0 },
        { "Guristas Despoiler", 29 },
        { "Triglavian Biocombinative Cache", 0 },
        { "Devoted Knight", 100 }
    };

    /// <summary>
    /// Gets the DPS for a specific NPC type
    /// </summary>
    public double GetNpcDps(string npcType)
    {
        var trimmedType = npcType.Trim();
        return DpsLookup.TryGetValue(trimmedType, out var dps)
            ? dps
            : throw new KeyNotFoundException($"Unknown NPC type: {npcType}");
    }

    /// <summary>
    /// Checks if the NPC type exists in the database
    /// </summary>
    public bool IsKnownNpc(string npcType)
    {
        return DpsLookup.ContainsKey(npcType.Trim());
    }

    /// <summary>
    /// Calculates the total DPS from a collection of NPCs
    /// </summary>
    public double CalculateTotalDps(IEnumerable<string> npcTypes)
    {
        return npcTypes.Sum(GetNpcDps);
    }

    /// <summary>
    /// Determines if an NPC should be used as an orbit beacon
    /// High-threat targets that require orbiting
    /// </summary>
    public bool IsOrbitBeacon(string npcName)
    {
        return npcName.Contains("Leshak")
               || npcName.Contains("Overmind")
               || npcName.Contains("Battleship");
    }

    /// <summary>
    /// Calculates target priority for an NPC
    /// Lower values = higher priority
    /// </summary>
    public int CalculateTargetPriority(string npcName, string npcType)
    {
        // Faction spawns (highest priority)
        if (npcName.Contains("Guristas"))
            return 20;

        // Critical threat NPCs
        if (npcName.Contains("Anchoring"))
            return 1;

        if (npcName.Contains("Firewatcher"))
            return 2;

        // Support/healing NPCs
        if (npcName.Contains("Renewing")
            || npcName.Contains("Plateforger")
            || npcName.Contains("Fieldweaver"))
            return 3;

        // Tackle/EWAR NPCs
        if (npcName.Contains("Entangler")
            || npcName.Contains("Snarecaster"))
            return 6;

        // Named bosses
        if (npcName.Contains("Scylla") || npcName.Contains("Tyrannos"))
            return 8;

        // Extraction nodes (low priority)
        if (npcName.Contains("Extraction"))
            return 10;

        // Environmental objects (very low priority)
        if (npcName.Contains("Bioadaptive"))
            return 1000;

        // Drifter Battleships (special case - very low priority)
        if (npcType.Contains("Battleship") && npcType.Contains("Drifter"))
            return 9000;

        // Default: prioritize by DPS (higher DPS = higher priority)
        // 800 - DPS means higher DPS gets lower number (higher priority)
        var dps = IsKnownNpc(npcType) ? GetNpcDps(npcType) : 0;
        return (int)(800 - dps);
    }

    /// <summary>
    /// Gets all known NPC types
    /// </summary>
    public IEnumerable<string> GetAllKnownNpcTypes()
    {
        return DpsLookup.Keys;
    }
}
