using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.DataContainers.Abstractions;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations;

/// <summary>
/// Service type definition for the standard transformation service implementation.
/// </summary>
[ServiceTypeOption(typeof(TransformationTypes), "StandardTransformation")]
public sealed class StandardTransformationServiceType :
    TransformationTypeBase<ITransformationProvider, ITransformationsConfiguration, IServiceFactory<ITransformationProvider, ITransformationsConfiguration>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StandardTransformationServiceType"/> class.
    /// </summary>
    public StandardTransformationServiceType()
        : base(
            id: 1,
            name: "StandardTransformation",
            inputType: typeof(object),
            outputType: typeof(object),
            supportsStreaming: true,
            supportedContainers: []) // TODO: Add actual container types
    {
    }

    /// <summary>
    /// Registers services required by this service type with the DI container.
    /// </summary>
    public override void Register(IServiceCollection services)
    {
        // Register the generic factory
        services.AddScoped<IServiceFactory<ITransformationProvider, ITransformationsConfiguration>, GenericServiceFactory<ITransformationProvider, ITransformationsConfiguration>>();

        // Register the transformation provider
        services.AddScoped<ITransformationProvider, StandardTransformationProvider>();
    }

    /// <summary>
    /// Configures the service type using the provided configuration.
    /// </summary>
    public override void Configure(IConfiguration configuration)
    {
        // Configuration validation and setup can be added here
        // For now, this is a no-op as the standard transformation doesn't require special configuration
    }
}