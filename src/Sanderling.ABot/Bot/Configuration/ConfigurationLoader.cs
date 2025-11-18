using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Sanderling.ABot.Bot.Configuration;

/// <summary>
/// Loads and validates configuration from JSON files
/// </summary>
public static class ConfigurationLoader
{
	/// <summary>
	/// Loads all configuration files and registers them with the service collection
	/// </summary>
	/// <param name="services">The service collection to register configurations with</param>
	/// <param name="configPath">Path to the configuration directory (defaults to ./config)</param>
	public static void LoadConfigurations(IServiceCollection services, string? configPath = null)
	{
		configPath ??= Path.Combine(AppContext.BaseDirectory, "config");

		if (!Directory.Exists(configPath))
		{
			throw new DirectoryNotFoundException($"Configuration directory not found: {configPath}");
		}

		// Build configuration
		var configuration = new ConfigurationBuilder()
			.SetBasePath(configPath)
			.AddJsonFile("combat.json", optional: false, reloadOnChange: true)
			.AddJsonFile("shipfits.json", optional: false, reloadOnChange: true)
			.AddJsonFile("abyss.json", optional: false, reloadOnChange: true)
			.AddJsonFile("bot-settings.json", optional: false, reloadOnChange: true)
			.Build();

		// Register configurations
		services.Configure<CombatConfiguration>(configuration);
		services.Configure<ShipFitConfiguration>(configuration);
		services.Configure<AbyssConfiguration>(configuration);
		services.Configure<BotConfiguration>(configuration);

		// Add validation
		services.AddSingleton<IValidateOptions<CombatConfiguration>, CombatConfigurationValidator>();
		services.AddSingleton<IValidateOptions<ShipFitConfiguration>, ShipFitConfigurationValidator>();
		services.AddSingleton<IValidateOptions<AbyssConfiguration>, AbyssConfigurationValidator>();
		services.AddSingleton<IValidateOptions<BotConfiguration>, BotConfigurationValidator>();
	}

	/// <summary>
	/// Loads configuration from JSON files without DI container
	/// </summary>
	/// <param name="configPath">Path to the configuration directory (defaults to ./config)</param>
	public static ConfigurationSet LoadConfigurationsStandalone(string? configPath = null)
	{
		configPath ??= Path.Combine(AppContext.BaseDirectory, "config");

		if (!Directory.Exists(configPath))
		{
			throw new DirectoryNotFoundException($"Configuration directory not found: {configPath}");
		}

		var configuration = new ConfigurationBuilder()
			.SetBasePath(configPath)
			.AddJsonFile("combat.json", optional: false, reloadOnChange: false)
			.AddJsonFile("shipfits.json", optional: false, reloadOnChange: false)
			.AddJsonFile("abyss.json", optional: false, reloadOnChange: false)
			.AddJsonFile("bot-settings.json", optional: false, reloadOnChange: false)
			.Build();

		var combatConfig = configuration.Get<CombatConfiguration>() ?? new CombatConfiguration();
		var shipFitConfig = configuration.Get<ShipFitConfiguration>() ?? new ShipFitConfiguration();
		var abyssConfig = configuration.Get<AbyssConfiguration>() ?? new AbyssConfiguration();
		var botConfig = configuration.Get<BotConfiguration>() ?? new BotConfiguration();

		// Validate
		new CombatConfigurationValidator().Validate(null, combatConfig);
		new ShipFitConfigurationValidator().Validate(null, shipFitConfig);
		new AbyssConfigurationValidator().Validate(null, abyssConfig);
		new BotConfigurationValidator().Validate(null, botConfig);

		return new ConfigurationSet(combatConfig, shipFitConfig, abyssConfig, botConfig);
	}
}

/// <summary>
/// Container for all configuration objects
/// </summary>
public record ConfigurationSet(
	CombatConfiguration Combat,
	ShipFitConfiguration ShipFits,
	AbyssConfiguration Abyss,
	BotConfiguration Bot
);

// Validators
public class CombatConfigurationValidator : IValidateOptions<CombatConfiguration>
{
	public ValidateOptionsResult Validate(string? name, CombatConfiguration options)
	{
		if (options.NpcDpsValues == null || options.NpcDpsValues.Count == 0)
		{
			return ValidateOptionsResult.Fail("NpcDpsValues cannot be null or empty");
		}

		if (options.Engagement.MaxTargetDistance <= 0)
		{
			return ValidateOptionsResult.Fail("MaxTargetDistance must be greater than 0");
		}

		if (options.Engagement.DefensiveManeuverDpsThreshold < 0)
		{
			return ValidateOptionsResult.Fail("DefensiveManeuverDpsThreshold must be non-negative");
		}

		return ValidateOptionsResult.Success;
	}
}

public class ShipFitConfigurationValidator : IValidateOptions<ShipFitConfiguration>
{
	public ValidateOptionsResult Validate(string? name, ShipFitConfiguration options)
	{
		if (options.Ships == null || options.Ships.Count == 0)
		{
			return ValidateOptionsResult.Fail("Ships configuration cannot be null or empty");
		}

		foreach (var ship in options.Ships.Values)
		{
			if (string.IsNullOrWhiteSpace(ship.ShipName))
			{
				return ValidateOptionsResult.Fail("Ship name cannot be null or empty");
			}

			if (ship.MaxTargetingRange < 0)
			{
				return ValidateOptionsResult.Fail($"MaxTargetingRange for {ship.ShipName} must be non-negative");
			}

			if (ship.MaxTargets < 0)
			{
				return ValidateOptionsResult.Fail($"MaxTargets for {ship.ShipName} must be non-negative");
			}

			if (ship.MaxDronesInSpace < 0)
			{
				return ValidateOptionsResult.Fail($"MaxDronesInSpace for {ship.ShipName} must be non-negative");
			}
		}

		return ValidateOptionsResult.Success;
	}
}

public class AbyssConfigurationValidator : IValidateOptions<AbyssConfiguration>
{
	public ValidateOptionsResult Validate(string? name, AbyssConfiguration options)
	{
		if (string.IsNullOrWhiteSpace(options.FilamentType))
		{
			return ValidateOptionsResult.Fail("FilamentType cannot be null or empty");
		}

		if (options.InvulnerabilityWaitTimeSeconds <= 0)
		{
			return ValidateOptionsResult.Fail("InvulnerabilityWaitTimeSeconds must be greater than 0");
		}

		if (options.TractorDeploymentDistance <= 0)
		{
			return ValidateOptionsResult.Fail("TractorDeploymentDistance must be greater than 0");
		}

		if (options.LootingDistance <= 0)
		{
			return ValidateOptionsResult.Fail("LootingDistance must be greater than 0");
		}

		return ValidateOptionsResult.Success;
	}
}

public class BotConfigurationValidator : IValidateOptions<BotConfiguration>
{
	public ValidateOptionsResult Validate(string? name, BotConfiguration options)
	{
		if (string.IsNullOrWhiteSpace(options.DefaultShipFit))
		{
			return ValidateOptionsResult.Fail("DefaultShipFit cannot be null or empty");
		}

		if (options.DefaultActionDelayMs < 0)
		{
			return ValidateOptionsResult.Fail("DefaultActionDelayMs must be non-negative");
		}

		return ValidateOptionsResult.Success;
	}
}
