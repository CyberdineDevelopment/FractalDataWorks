using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

/// <summary>
/// Global collection of all process types across all assemblies.
/// This uses the GlobalEnumCollection pattern to automatically discover
/// all ProcessTypeBase implementations at runtime.
/// </summary>
[GlobalStaticEnumCollection(CollectionName = "ProcessTypes", DefaultGenericReturnType = typeof(IProcessType))]
public abstract class ProcessTypeCollectionBase : EnumCollectionBase<ProcessTypeBase>
{
}