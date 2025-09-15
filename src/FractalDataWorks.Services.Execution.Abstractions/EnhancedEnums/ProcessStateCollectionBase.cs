using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

/// <summary>
/// Collection of all process states.
/// </summary>
[StaticEnumCollection(CollectionName = "ProcessStates", DefaultGenericReturnType = typeof(IProcessState),UseSingletonInstances = true)]
public abstract class ProcessStateCollectionBase : EnumCollectionBase<ProcessStateBase>
{
}