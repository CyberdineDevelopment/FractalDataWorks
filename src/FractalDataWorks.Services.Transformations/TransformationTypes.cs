using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations;

/// <summary>
/// ServiceType collection for all transformation types.
/// The source generator will discover all TransformationTypeBase implementations.
/// </summary>
[ServiceTypeCollection("ITransformationType", "TransformationTypes")]
public static partial class TransformationTypes
{
    /// <summary>
    /// Registers all discovered transformation types with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void Register(IServiceCollection services)
    {
        // Register each discovered transformation type
        foreach (var transformationType in All)
        {
            transformationType.Register(services);
        }
    }

    /// <summary>
    /// Registers all discovered transformation types with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Custom configuration action for special cases.</param>
    public static void Register(IServiceCollection services, Action<TransformationRegistrationOptions> configure)
    {
        var options = new TransformationRegistrationOptions();
        configure(options);

        // Register each discovered transformation type with custom options
        foreach (var transformationType in All)
        {
            transformationType.Register(services);

            // Apply custom configuration if needed
            if (options.CustomConfigurations.TryGetValue(transformationType.Name, out var customConfig))
            {
                customConfig(services, transformationType);
            }
        }
    }
}

/// <summary>
/// Configuration options for transformation registration.
/// </summary>
public class TransformationRegistrationOptions
{
    /// <summary>
    /// Custom configurations for specific transformation types.
    /// </summary>
    public Dictionary<string, Action<IServiceCollection, ITransformationType>> CustomConfigurations { get; } = new();

    /// <summary>
    /// Configure a specific transformation type.
    /// </summary>
    /// <param name="transformationTypeName">The name of the transformation type (e.g., "Standard").</param>
    /// <param name="configure">Custom configuration action.</param>
    public void Configure(string transformationTypeName, Action<IServiceCollection, ITransformationType> configure)
    {
        CustomConfigurations[transformationTypeName] = configure;
    }
}