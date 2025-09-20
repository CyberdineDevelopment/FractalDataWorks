using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions;

/// <summary>
/// Concrete collection of all scheduler service types in the system.
/// This partial class will be extended by the source generator to include
/// all discovered scheduler types with high-performance lookup capabilities.
/// </summary>
[ServiceTypeCollection("SchedulerTypeBase", "SchedulerTypes")]
public partial class SchedulerTypesBase : 
    SchedulerTypeCollectionBase<
        SchedulerTypeBase<IFdwSchedulingService, ISchedulingConfiguration, IServiceFactory<IFdwSchedulingService, ISchedulingConfiguration>>,
        SchedulerTypeBase<IFdwSchedulingService, ISchedulingConfiguration, IServiceFactory<IFdwSchedulingService, ISchedulingConfiguration>>,
        IFdwSchedulingService,
        ISchedulingConfiguration,
        IServiceFactory<IFdwSchedulingService, ISchedulingConfiguration>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SchedulerTypesBase"/> class.
    /// The source generator will populate all discovered scheduler types.
    /// </summary>
    public SchedulerTypesBase()
    {
        // Source generator will add:
        // - Static fields for each scheduler type (e.g., QuartzNet, Hangfire, FluentScheduler, etc.)
        // - FrozenDictionary for O(1) lookups by Id/Name
        // - Factory methods for each constructor overload
        // - Empty() method returning default instance
        // - All() method returning all scheduler types
        // - Lookup methods by SchedulingEngine, capabilities, etc.
    }
}