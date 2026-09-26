using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sanderling.ABot.Bot.Configuration
{
	/// <summary>Static EVE inventory metadata for a fitted module type.</summary>
	public sealed class ModuleTypeInfo
	{
		[JsonPropertyName("n")] public string Name { get; set; } = "";
		[JsonPropertyName("g")] public int GroupId { get; set; }
		[JsonPropertyName("p")] public bool Published { get; set; }
	}

	/// <summary>
	/// Offline type-ID lookup over the bundled module subset of the EVE reference data.
	/// The numeric type ID remains authoritative; names are only added for readable logs,
	/// diagnostics and configuration. Unknown/new IDs degrade to a stable numeric label.
	/// </summary>
	public static class ModuleTypes
	{
		private static readonly Lazy<Dictionary<int, ModuleTypeInfo>> db = new(Load);

		public static IReadOnlyDictionary<int, ModuleTypeInfo> All => db.Value;
		public static int Count => db.Value.Count;

		public static ModuleTypeInfo? Lookup(int typeId) =>
			db.Value.TryGetValue(typeId, out var module) ? module : null;

		public static string NameOrId(int typeId) => Lookup(typeId)?.Name is { Length: > 0 } name
			? $"{name} (type {typeId})"
			: $"module type {typeId}";

		private static Dictionary<int, ModuleTypeInfo> Load()
		{
			try
			{
				var assembly = Assembly.GetExecutingAssembly();
				var resource = assembly.GetManifestResourceNames().FirstOrDefault(name =>
					name.EndsWith("module-types.json", StringComparison.OrdinalIgnoreCase));
				if (resource == null) return new();

				using var stream = assembly.GetManifestResourceStream(resource)!;
				return JsonSerializer.Deserialize<Dictionary<int, ModuleTypeInfo>>(stream) ?? new();
			}
			catch
			{
				return new();
			}
		}
	}
}
