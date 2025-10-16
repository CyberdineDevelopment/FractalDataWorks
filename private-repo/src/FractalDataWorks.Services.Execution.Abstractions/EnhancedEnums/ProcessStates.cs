using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Services.Execution.Abstractions.EnhancedEnums;

/// <summary>
/// Collection of all process states.
/// </summary>
[TypeCollection(typeof(ProcessStateBase), typeof(IProcessState), typeof(ProcessStates))]
public partial class ProcessStates : TypeCollectionBase<ProcessStateBase, IProcessState>
{

}