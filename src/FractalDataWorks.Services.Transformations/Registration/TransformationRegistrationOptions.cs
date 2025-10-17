using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations;

/// <summary>
/// Configuration options for transformation registration.
/// </summary>
public class TransformationRegistrationOptions
{
    private readonly Dictionary<string, Action<IServiceCollection, ITransformationType>> _customConfigurations = new(StringComparer.Ordinal);

    /// <summary>
    /// Custom configurations for specific transformation types.
    /// </summary>
    public IDictionary<string, Action<IServiceCollection, ITransformationType>> CustomConfigurations => _customConfigurations;

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