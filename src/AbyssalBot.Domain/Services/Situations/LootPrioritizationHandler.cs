using AbyssalBot.Domain.Models;

namespace AbyssalBot.Domain.Services.Situations;

/// <summary>
/// Handles loot prioritization to maximize value collected
/// </summary>
public class LootPrioritizationHandler : ILootPrioritizationHandler
{
    // High-value item types (in priority order)
    private static readonly string[] HighPriorityTypes = new[]
    {
        "Mutaplasmid",
        "Filament",
        "Triglavian Data",
        "Abyssal Module",
        "Skill Book"
    };

    private const double MinimumValuePerM3 = 100000.0; // 100k ISK per m³
    private const double HighValueThreshold = 10000000.0; // 10M ISK

    public IReadOnlyList<LootTarget> PrioritizeLoot(
        IReadOnlyList<LootTarget> available,
        InventorySpace space)
    {
        // If cargo is nearly full and contains valuable items, skip looting
        if (space.IsNearlyFull && space.CurrentValue > 50000000) // 50M ISK
        {
            return Array.Empty<LootTarget>();
        }

        var prioritized = new List<LootTarget>();
        var remainingSpace = space.AvailableVolume;

        // Priority 1: Mutaplasmids (highest priority)
        var mutaplasmids = available
            .Where(l => l.Type.Contains("Mutaplasmid", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(l => l.EstimatedValue)
            .ToList();

        foreach (var item in mutaplasmids)
        {
            if (item.Volume <= remainingSpace)
            {
                prioritized.Add(item);
                remainingSpace -= item.Volume;
            }
        }

        // Priority 2: Filaments
        var filaments = available
            .Where(l => l.Type.Contains("Filament", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(l => l.EstimatedValue)
            .ToList();

        foreach (var item in filaments)
        {
            if (item.Volume <= remainingSpace && !prioritized.Contains(item))
            {
                prioritized.Add(item);
                remainingSpace -= item.Volume;
            }
        }

        // Priority 3: High-value items (> 10M ISK)
        var highValueItems = available
            .Where(l => l.EstimatedValue > HighValueThreshold)
            .Where(l => !prioritized.Contains(l))
            .OrderByDescending(l => l.ValueDensity) // Value per m³
            .ToList();

        foreach (var item in highValueItems)
        {
            if (item.Volume <= remainingSpace)
            {
                prioritized.Add(item);
                remainingSpace -= item.Volume;
            }
        }

        // Priority 4: Good value density items
        var valueDenseItems = available
            .Where(l => l.ValueDensity > MinimumValuePerM3)
            .Where(l => !prioritized.Contains(l))
            .OrderByDescending(l => l.ValueDensity)
            .ToList();

        foreach (var item in valueDenseItems)
        {
            if (item.Volume <= remainingSpace)
            {
                prioritized.Add(item);
                remainingSpace -= item.Volume;
            }
        }

        // Priority 5: Fill remaining space with anything valuable
        if (remainingSpace > 10) // At least 10 m³ remaining
        {
            var remainingItems = available
                .Where(l => !prioritized.Contains(l))
                .Where(l => l.EstimatedValue > 0)
                .OrderByDescending(l => l.ValueDensity)
                .ToList();

            foreach (var item in remainingItems)
            {
                if (item.Volume <= remainingSpace)
                {
                    prioritized.Add(item);
                    remainingSpace -= item.Volume;
                }
            }
        }

        return prioritized;
    }

    public string GetPrioritizationReasoning(
        IReadOnlyList<LootTarget> available,
        InventorySpace space)
    {
        if (!available.Any())
        {
            return "No loot available";
        }

        if (space.IsNearlyFull && space.CurrentValue > 50000000)
        {
            return $"Cargo nearly full ({space.PercentageFull:F0}%) with valuable items ({space.CurrentValue / 1000000:F1}M ISK) - skipping loot";
        }

        var prioritized = PrioritizeLoot(available, space);

        if (!prioritized.Any())
        {
            return $"No valuable loot fits in remaining space ({space.AvailableVolume:F1} m³)";
        }

        var totalValue = prioritized.Sum(l => l.EstimatedValue);
        var totalVolume = prioritized.Sum(l => l.Volume);
        var mutaCount = prioritized.Count(l => l.Type.Contains("Mutaplasmid", StringComparison.OrdinalIgnoreCase));
        var filamentCount = prioritized.Count(l => l.Type.Contains("Filament", StringComparison.OrdinalIgnoreCase));

        var reasoning = new List<string>
        {
            $"Collecting {prioritized.Count} items ({totalVolume:F1} m³, {totalValue / 1000000:F1}M ISK)"
        };

        if (mutaCount > 0)
        {
            reasoning.Add($"{mutaCount} mutaplasmids (highest priority)");
        }

        if (filamentCount > 0)
        {
            reasoning.Add($"{filamentCount} filaments");
        }

        return string.Join(". ", reasoning);
    }
}
