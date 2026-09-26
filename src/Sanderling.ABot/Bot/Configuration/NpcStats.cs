using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Sanderling.ABot.Bot.Configuration
{
	/// <summary>
	/// Combat stats for one NPC type, precomputed from the EVE Static Data Export
	/// (via EVE Ref reference-data) and bundled as <c>npc-stats.json</c>. Keyed by the exact
	/// in-game type name, which is what the overview shows — so an overview enemy resolves 1:1.
	/// EHP is precomputed against a few damage profiles; DPS is the base (unramped) weapon output.
	/// </summary>
	public sealed class NpcStat
	{
		[JsonPropertyName("id")] public int Id { get; set; }
		[JsonPropertyName("g")] public int Group { get; set; }
		[JsonPropertyName("cat")] public int Cat { get; set; }
		/// <summary>Base weapon DPS (unramped). Null/0 for structures, loot, drones-that-don't-shoot.</summary>
		[JsonPropertyName("dps")] public double? Dps { get; set; }
		/// <summary>Dominant damage type dealt: em/th/kin/exp.</summary>
		[JsonPropertyName("dmg")] public string? Dmg { get; set; }
		/// <summary>Triglavian-style ramping disintegrator (DPS climbs the longer it fires one target).</summary>
		[JsonPropertyName("ramp")] public bool Ramp { get; set; }
		[JsonPropertyName("ehpKin")] public long? EhpKin { get; set; }
		[JsonPropertyName("ehpTh")] public long? EhpTh { get; set; }
		[JsonPropertyName("ehpOmni")] public long? EhpOmni { get; set; }
		/// <summary>Raw shield/armor/hull HP, keyed by s/a/h.</summary>
		[JsonPropertyName("hp")] public Dictionary<string, double>? Hitpoints { get; set; }
		/// <summary>
		/// Damage resonance per layer and damage type. Applied damage is raw damage multiplied by
		/// resonance, so 0.4 means 60% resistance. Outer keys are s/a/h; inner keys are em/th/kin/exp.
		/// </summary>
		[JsonPropertyName("res")] public Dictionary<string, Dictionary<string, double>>? Resonances { get; set; }
		[JsonPropertyName("vmax")] public int? Vmax { get; set; }
		[JsonPropertyName("sig")] public int? Sig { get; set; }
		/// <summary>EWAR the NPC applies, from its dogma: web/scram/neut/rep/damp/td/paint/ecm.</summary>
		[JsonPropertyName("ewar")] public string[]? Ewar { get; set; }

		public bool HasEwar(string kind) => Ewar != null && Array.IndexOf(Ewar, kind) >= 0;
		public bool Webs => HasEwar("web");
		public bool Scrams => HasEwar("scram");
		public bool Neuts => HasEwar("neut");
		public bool RemoteReps => HasEwar("rep");
		public bool Damps => HasEwar("damp");
		public bool Jams => HasEwar("ecm");

		public double HitpointsFor(string layer) =>
			Hitpoints != null && Hitpoints.TryGetValue(layer, out var value) ? value : 0;

		public double ResonanceFor(string layer, string damageType) =>
			Resonances != null && Resonances.TryGetValue(layer, out var byDamage) &&
			byDamage.TryGetValue(damageType, out var value) ? value : 1;

		/// <summary>
		/// EHP against ONE damage type (em/th/kin/exp): shield + armor + hull, each divided by that
		/// layer's resonance to the type. This is what makes a Nova-vs-Scourge choice a number —
		/// Triglavian armor sits at 0.64 to explosive but 0.47 to kinetic. Falls back to the
		/// precomputed kin/th/omni summaries when the per-layer data is missing; null when unknown.
		/// </summary>
		public double? EhpFor(string? damageType)
		{
			var type = string.IsNullOrWhiteSpace(damageType) ? "kin" : damageType.Trim().ToLowerInvariant();
			if (Hitpoints != null && Hitpoints.Count > 0 && Resonances != null && Resonances.Count > 0)
			{
				var total = 0.0;
				foreach (var (layer, hp) in Hitpoints)
				{
					var resonance = ResonanceFor(layer, type);
					total += hp / (resonance > 0 ? resonance : 1);
				}
				return total;
			}
			return type switch
			{
				"kin" => EhpKin,
				"th" => EhpTh,
				_ => EhpOmni,
			};
		}
	}

	/// <summary>
	/// Lazy-loaded lookup over the bundled NPC stat database. Resolves an overview enemy name
	/// (markup stripped, trimmed) to its <see cref="NpcStat"/>. Everything is best-effort: a name
	/// the DB doesn't know returns null and callers fall back to heuristics — nothing throws.
	/// </summary>
	public static class NpcStats
	{
		private static readonly Lazy<Dictionary<string, NpcStat>> db = new(Load);

		public static IReadOnlyDictionary<string, NpcStat> All => db.Value;
		public static int Count => db.Value.Count;

		public static NpcStat? Lookup(string? name)
		{
			if (string.IsNullOrWhiteSpace(name)) return null;
			return db.Value.TryGetValue(Clean(name), out var s) ? s : null;
		}

		/// <summary>Base weapon DPS for a type, or 0 when unknown/non-combat.</summary>
		public static double Dps(string? name) => Lookup(name)?.Dps ?? 0;

		/// <summary>EHP versus kinetic (our missile-boat default), or null when unknown.</summary>
		public static long? EhpKinetic(string? name) => Lookup(name)?.EhpKin;

		/// <summary>EHP versus one damage type (em/th/kin/exp), or null when the type is unknown.</summary>
		public static double? EhpVersus(string? name, string? damageType) => Lookup(name)?.EhpFor(damageType);

		private static string Clean(string name) => Regex.Replace(name, "<.*?>", "").Trim();

		private static Dictionary<string, NpcStat> Load()
		{
			try
			{
				var asm = Assembly.GetExecutingAssembly();
				var res = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("npc-stats.json", StringComparison.OrdinalIgnoreCase));
				if (res == null) return new();
				using var stream = asm.GetManifestResourceStream(res)!;
				var parsed = JsonSerializer.Deserialize<Dictionary<string, NpcStat>>(stream);
				return parsed ?? new();
			}
			catch { return new(); }
		}
	}
}
