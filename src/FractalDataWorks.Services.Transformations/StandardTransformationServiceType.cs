using System;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations;

/// <summary>
/// Service type definition for the standard transformation service implementation.
/// </summary>
public sealed class StandardTransformationServiceType : 
    TransformationServiceType<StandardTransformationServiceType, ITransformationProvider, ITransformationsConfiguration, IServiceFactory<ITransformationProvider, ITransformationsConfiguration>>,
    IEnumOption<StandardTransformationServiceType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StandardTransformationServiceType"/> class.
    /// </summary>
    public StandardTransformationServiceType() 
        : base(
            id: 1, 
            name: "StandardTransformation", 
            description: "Standard data transformation service with mapping, filtering, and pipeline capabilities",
            supportedInputTypes: new[] { "JSON", "XML", "CSV", "Object", "Dictionary", "Stream" },
            supportedOutputTypes: new[] { "JSON", "XML", "CSV", "Object", "Dictionary", "Stream" },
            supportedCategories: new[] { "Mapping", "Filtering", "Validation", "Aggregation", "Formatting", "Conversion" },
            supportsParallelExecution: true,
            supportsTransformationCaching: true,
            supportsPipelineMode: true,
            maxInputSizeBytes: 104857600L, // 100MB
            priority: 75)
    {
    }

    /// <inheritdoc/>
    public override Type GetFactoryImplementationType()
    {
        // Use the generic factory for now
        // Can be overridden later to return a custom StandardTransformationServiceFactory if needed
        return typeof(GenericServiceFactory<ITransformationProvider, ITransformationsConfiguration>);
    }
}