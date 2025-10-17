using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FractalDataWorks.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Abstractions;
using static FractalDataWorks.Services.Abstractions.ServiceLifetimes;

namespace FractalDataWorks.Services.Extensions;

/// <summary>
/// Extension methods for registering service factories with the dependency injection container.
/// </summary>
/// <remarks>
/// These extensions provide a fluent API for registering service factories and configuring
/// the ServiceFactoryProvider. They handle the integration between the factory provider
/// and the DI container.
/// </remarks>
public static class ServiceFactoryRegistrationExtensions
{
    /// <summary>
    /// Adds the ServiceFactoryProvider to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method registers the ServiceFactoryProvider as a singleton, ensuring that
    /// factory registrations are shared across the entire application.
    /// </remarks>
    public static IServiceCollection AddServiceFactoryProvider(this IServiceCollection services)
    {
        services.AddSingleton<IServiceFactoryProvider, ServiceFactoryProvider>();
        return services;
    }

    /// <summary>
    /// Registers connection factories with both the DI container and the factory provider.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="configure">Action to configure the factory registrations.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method provides a two-phase registration:
    /// 1. Registers factory types with the DI container for dependency injection
    /// 2. Configures the factory provider to map type names to factory instances
    /// 
    /// The configure action receives a factory registration builder that provides
    /// a fluent API for registering connection factories.
    /// </remarks>
    public static IServiceCollection RegisterConnectionFactories(
        this IServiceCollection services,
        Action<IConnectionFactoryRegistrationBuilder> configure)
    {
        if (configure == null)
            throw new ArgumentNullException(nameof(configure));

        // Add the factory provider if not already added
        services.AddServiceFactoryProvider();

        // Create the registration builder
        var builder = new ConnectionFactoryRegistrationBuilder(services);

        // Let the caller configure the registrations
        configure(builder);

        return services;
    }

    /// <summary>
    /// Registers connection factories automatically from configuration.
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="configuration">The configuration containing connection factory settings.</param>
    /// <param name="sectionName">The configuration section name (defaults to "ConnectionFactories").</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method reads connection factory registrations from configuration and
    /// registers them automatically. This is useful for scenarios where the
    /// factory registrations are defined in configuration files.
    /// 
    /// Expected configuration format:
    /// {
    ///   "ConnectionFactories": {
    ///     "MsSql": {
    ///       "FactoryType": "FractalDataWorks.Services.Connections.MsSql.MsSqlConnectionFactory",
    ///       "Lifetime": "Scoped"
    ///     },
    ///     "Rest": {
    ///       "FactoryType": "FractalDataWorks.Services.Connections.Rest.RestConnectionFactory", 
    ///       "Lifetime": "Transient"
    ///     }
    ///   }
    /// }
    /// </remarks>
    public static IServiceCollection RegisterConnectionFactoriesFromConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "ConnectionFactories")
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        var factoriesSection = configuration.GetSection(sectionName);
        if (!factoriesSection.Exists())
        {
            return services; // No configuration section found, skip registration
        }

        services.AddServiceFactoryProvider();

        services.AddSingleton<IServiceProvider>(provider =>
        {
            var factoryProvider = provider.GetRequiredService<IServiceFactoryProvider>();

            foreach (var factorySection in factoriesSection.GetChildren())
            {
                var typeName = factorySection.Key;
                var factoryTypeName = factorySection["FactoryType"];
                var lifetimeName = factorySection["Lifetime"] ?? "Scoped";

                if (string.IsNullOrWhiteSpace(factoryTypeName))
                {
                    continue; // Skip if no factory type specified
                }

                var factoryType = Type.GetType(factoryTypeName);
                if (factoryType == null || !typeof(IServiceFactory).IsAssignableFrom(factoryType))
                {
                    continue; // Skip if type not found or doesn't implement IServiceFactory
                }

                // Register the factory type with appropriate lifetime
                var lifetime = ByName(lifetimeName) ?? Scoped;
                
                RegisterFactoryWithLifetime(services, factoryType, lifetime);

                // Register with factory provider using the new lifetime-aware method
                var factory = provider.GetService(factoryType) as IServiceFactory;
                if (factory != null)
                {
                    factoryProvider.RegisterFactory(typeName, factory, lifetime);
                }
            }

            return provider;
        });

        return services;
    }

    /// <summary>
    /// Registers a factory type with the DI container using the specified lifetime.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="factoryType">The factory type to register.</param>
    /// <param name="lifetime">The service lifetime.</param>
    internal static void RegisterFactoryWithLifetime(IServiceCollection services, Type factoryType, IServiceLifetime lifetime)
    {
        // Use the Microsoft ServiceLifetime enum directly from our lifetime object
        services.Add(new ServiceDescriptor(factoryType, factoryType, lifetime.EnumValue));
    }
}
