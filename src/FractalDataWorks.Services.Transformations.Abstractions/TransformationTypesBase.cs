using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.DataContainers.Abstractions;

namespace FractalDataWorks.Services.Transformations.Abstractions;

/// <summary>
/// Concrete collection of all transformation service types in the system.
/// This partial class will be extended by the source generator to include
/// all discovered transformation types with high-performance lookup capabilities.
/// </summary>
[ServiceTypeCollection("TransformationTypeBase", "TransformationTypes")]
public partial class TransformationTypesBase : 
    TransformationTypeCollectionBase<
        TransformationTypeBase<ITransformationProvider, ITransformationsConfiguration, IServiceFactory<ITransformationProvider, ITransformationsConfiguration>>,
        TransformationTypeBase<ITransformationProvider, ITransformationsConfiguration, IServiceFactory<ITransformationProvider, ITransformationsConfiguration>>,
        ITransformationProvider,
        ITransformationsConfiguration,
        IServiceFactory<ITransformationProvider, ITransformationsConfiguration>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformationTypesBase"/> class.
    /// The source generator will populate all discovered transformation types.
    /// </summary>
    public TransformationTypesBase()
    {
        // Source generator will add:
        // - Static fields for each transformation type (e.g., DataCleaning, FormatConversion, etc.)
        // - FrozenDictionary for O(1) lookups by Id/Name
        // - Factory methods for each constructor overload
        // - Empty() method returning default instance
        // - All() method returning all transformation types
        // - Lookup methods by InputType, OutputType, Categories, etc.
    }
}