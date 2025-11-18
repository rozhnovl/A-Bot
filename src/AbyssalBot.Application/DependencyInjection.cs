using Microsoft.Extensions.DependencyInjection;
using Sanderling.ABot.Bot;
using Sanderling.ABot.Bot.Strategies;

namespace AbyssalBot.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers domain services (strategies, states, providers)
    /// </summary>
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Register strategies
        services.AddScoped<IStrategy, AbyssalRunner>();

        // Register factories
        services.AddScoped<IStateFactory, StateFactory>();
        services.AddScoped<IProviderFactory, ProviderFactory>();

        // Register providers
        services.AddSingleton<INpcInfoProvider, NpcInfoProvider>();

        // States are created via StateFactory, so no need to register them individually
        // The factory pattern allows us to control state creation with configuration parameters

        return services;
    }

    /// <summary>
    /// Registers application services (bot orchestration)
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register the main Bot as scoped (one per request/session)
        services.AddScoped<Bot>();

        return services;
    }

    /// <summary>
    /// Registers infrastructure services (memory readers, external dependencies)
    /// </summary>
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Infrastructure services would be registered here
        // For now, this is a placeholder for future infrastructure dependencies
        // e.g., memory readers, external APIs, file systems, etc.

        return services;
    }
}
