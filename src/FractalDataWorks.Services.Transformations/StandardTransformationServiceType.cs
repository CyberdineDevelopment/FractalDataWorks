using System;
using FractalDataWorks.DataContainers.Abstractions;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Transformations.Abstractions;

namespace FractalDataWorks.Services.Transformations;

/// <summary>
/// Service type definition for the standard transformation service implementation.
/// </summary>
public sealed class StandardTransformationServiceType :
    TransformationTypeBase<ITransformationProvider, ITransformationsConfiguration, IServiceFactory<ITransformationProvider, ITransformationsConfiguration>>,
    IEnumOption<StandardTransformationServiceType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StandardTransformationServiceType"/> class.
    /// </summary>
    public StandardTransformationServiceType()
        : base(id: 1, name: "StandardTransformation")
    {
    }

    /// <inheritdoc/>
    public override Type InputType => typeof(object);

    /// <inheritdoc/>
    public override Type OutputType => typeof(object);

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;

    /// <inheritdoc/>
    public override IDataContainerType[] SupportedContainers => []; // TODO: Add actual container types

    /// <inheritdoc/>
    public override Type GetFactoryImplementationType()
    {
        // Use the generic factory for now
        // Can be overridden later to return a custom StandardTransformationServiceFactory if needed
        return typeof(GenericServiceFactory<ITransformationProvider, ITransformationsConfiguration>);
    }
}