using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

/// <summary>
/// Global collection of all process types across all assemblies.
/// This uses the TypeCollection pattern to automatically discover
/// all ProcessTypeBase implementations at runtime.
/// </summary>
[TypeCollection(typeof(ProcessTypeBase), typeof(IProcessType), typeof(ProcessTypes))]
public abstract partial class ProcessTypes
{
}