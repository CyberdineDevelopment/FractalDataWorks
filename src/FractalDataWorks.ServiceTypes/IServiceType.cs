using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Results;
using System;
using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services;

/// <summary>
/// Represents a strongly-typed service type definition with generic type parameters for service, configuration, and factory.
/// This interface extends the Enhanced Enums pattern to provide type-safe service registration and discovery.
/// Used for services that require specific factory implementations.
/// </summary>
/// <typeparam name="TService">The service interface type that this service type provides.</typeparam>
/// <typeparam name="TConfiguration">The configuration type required by the service.</typeparam>
/// <typeparam name="TFactory">The factory type used to create instances of the service.</typeparam>
/// <remarks>
/// This interface combines the Enhanced Enum pattern with service type definitions, enabling:
/// <list type="bullet">
/// <item><description>Type-safe service registration and discovery</description></item>
/// <item><description>Compile-time validation of service, configuration, and factory relationships</description></item>
/// <item><description>Enhanced Enum collection generation for service catalogs</description></item>
/// <item><description>Dependency injection integration with proper lifetime management</description></item>
/// </list>
/// </remarks>
public interface IServiceType<TService, TConfiguration, TFactory> : IServiceType<TService,TConfiguration>
    where TService : class, IFdwService
    where TFactory : class, IServiceFactory<TService, TConfiguration>
    where TConfiguration : class, IFdwConfiguration
{
    /// <summary>
    /// Creates a factory instance for this service type.
    /// Follows Railway-Oriented Programming - returns Result instead of throwing.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency resolution.</param>
    /// <returns>A result containing the factory instance or failure information.</returns>
    IFdwResult<Type> Factory();

}

/// <summary>
/// Represents a strongly-typed service type definition with generic type parameters for service and configuration.
/// This interface provides type safety for services that use standard factory patterns.
/// </summary>
/// <typeparam name="TService">The service interface type that this service type provides.</typeparam>
/// <typeparam name="TConfiguration">The configuration type required by the service.</typeparam>
/// <remarks>
/// This interface is used for services that:
/// <list type="bullet">
/// <item><description>Require specific configuration types</description></item>
/// <item><description>Use standard factory implementations</description></item>
/// <item><description>Need compile-time type validation</description></item>
/// <item><description>Integrate with the Enhanced Enum service discovery system</description></item>
/// </list>
/// </remarks>
public interface IServiceType<TService, TConfiguration> : IServiceType<TService>
    where TService : class, IFdwService
    where TConfiguration : IFdwConfiguration
{

}

/// <summary>
/// Represents a strongly-typed service type definition with a generic type parameter for the service.
/// This interface provides basic type safety for service registration and discovery.
/// </summary>
/// <typeparam name="TService">The service interface type that this service type provides.</typeparam>
/// <remarks>
/// This interface is used for services that:
/// <list type="bullet">
/// <item><description>Have simple registration requirements</description></item>
/// <item><description>Use default configuration patterns</description></item>
/// <item><description>Need basic type safety without complex factory relationships</description></item>
/// <item><description>Integrate with the framework's service discovery mechanisms</description></item>
/// </list>
/// </remarks>
public interface IServiceType<TService> : IServiceType
    where TService : class, IFdwService
{

}


/// <summary>
/// Base interface for all service type definitions in the FractalDataWorks framework.
/// Provides the fundamental contract for service registration, discovery, and factory creation.
/// </summary>
/// <remarks>
/// This interface serves as the foundation for the service type system, enabling:
/// <list type="bullet">
/// <item><description>Service registration with dependency injection containers</description></item>
/// <item><description>Service discovery and catalog management</description></item>
/// <item><description>Factory pattern implementation for service creation</description></item>
/// <item><description>Categorization and organization of services</description></item>
/// <item><description>Integration with Enhanced Enums for type-safe service collections</description></item>
/// </list>
/// 
/// Service types are typically implemented as Enhanced Enum options that provide
/// metadata and behavior for specific service implementations.
/// </remarks>
public interface IServiceType  : IEnumOption<IServiceType>
{
    /// <summary>
    /// Gets the service interface type that this service type provides.
    /// </summary>
    /// <value>The Type of the service interface that implementations must conform to.</value>
    /// <remarks>
    /// This property enables runtime type checking and factory creation for the appropriate service type.
    /// It should return the interface type, not the concrete implementation type.
    /// </remarks>
    Type ServiceType { get; }

    /// <summary>
    /// Gets the category of this service type for organizational purposes.
    /// </summary>
    /// <value>A string representing the service category (e.g., "Connection", "DataGateway", "Transformation", "Scheduling").</value>
    /// <remarks>
    /// Categories are used for:
    /// <list type="bullet">
    /// <item><description>Organizing services in management interfaces</description></item>
    /// <item><description>Filtering services by functional area</description></item>
    /// <item><description>Grouping related services in documentation</description></item>
    /// <item><description>Applying category-specific policies or behaviors</description></item>
    /// </list>
    /// </remarks>
    public string Category { get; }

    /// <summary>
    /// Gets the configuration type required by this service type.
    /// </summary>
    /// <value>The Type of the configuration class that this service requires for initialization.</value>
    /// <remarks>
    /// This property enables:
    /// <list type="bullet">
    /// <item><description>Compile-time validation of configuration compatibility</description></item>
    /// <item><description>Runtime configuration type checking and validation</description></item>
    /// <item><description>Dynamic configuration object creation and binding</description></item>
    /// <item><description>Configuration serialization and deserialization</description></item>
    /// </list>
    /// </remarks>
    Type ConfigurationType { get; }

    /// <summary>
    /// Gets the factory type for creating service instances.
    /// </summary>
    /// <value>The Type of the factory used to create service instances.</value>
    Type FactoryType { get; }


    /// <summary>
    /// Gets a detailed description of this service type's purpose and capabilities.
    /// </summary>
    /// <value>A comprehensive description explaining what this service type does and when to use it.</value>
    /// <remarks>
    /// The description should include:
    /// <list type="bullet">
    /// <item><description>The primary purpose and functionality of the service</description></item>
    /// <item><description>Key capabilities and features provided</description></item>
    /// <item><description>Typical use cases and scenarios</description></item>
    /// <item><description>Any important limitations or requirements</description></item>
    /// </list>
    /// </remarks>
    public string Description { get; }

}