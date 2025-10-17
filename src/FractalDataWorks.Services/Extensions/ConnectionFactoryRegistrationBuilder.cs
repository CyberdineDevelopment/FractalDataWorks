using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using FractalDataWorks.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Abstractions;
using static FractalDataWorks.Services.Abstractions.ServiceLifetimes;

namespace FractalDataWorks.Services.Extensions;

/// <summary>
/// Default implementation of IConnectionFactoryRegistrationBuilder.
/// </summary>
internal sealed class ConnectionFactoryRegistrationBuilder : IConnectionFactoryRegistrationBuilder
{
    private readonly IServiceCollection _services;

    public ConnectionFactoryRegistrationBuilder(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IConnectionFactoryRegistrationBuilder RegisterFactory<TFactory>(
        string typeName,
        IServiceLifetime? lifetime = null)
        where TFactory : class, IServiceFactory
    {
        if (string.IsNullOrWhiteSpace(typeName))
            throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));

        // Use default Scoped lifetime if none provided
        var effectiveLifetime = lifetime ?? Scoped;

        // Register the factory type with DI container using the new helper
        ServiceFactoryRegistrationExtensions.RegisterFactoryWithLifetime(_services, typeof(TFactory), effectiveLifetime);

        // Register with factory provider using lifetime-aware method
        _services.AddSingleton<IServiceProvider>(provider =>
        {
            var factoryProvider = provider.GetRequiredService<IServiceFactoryProvider>();
            var factory = provider.GetRequiredService<TFactory>();

            factoryProvider.RegisterFactory(typeName, factory, effectiveLifetime);
            
            return provider;
        });

        return this;
    }

    public IConnectionFactoryRegistrationBuilder RegisterFactory(string typeName, IServiceFactory factory)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            throw new ArgumentException("Type name cannot be null or empty", nameof(typeName));

        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        // Register the factory instance as singleton
        _services.AddSingleton(factory);

        // Register with factory provider
        _services.AddSingleton<IServiceProvider>(provider =>
        {
            var factoryProvider = provider.GetRequiredService<IServiceFactoryProvider>();
            factoryProvider.RegisterFactory(typeName, factory);
            return provider;
        });

        return this;
    }
}