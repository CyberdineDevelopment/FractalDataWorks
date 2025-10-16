using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Execution.Abstractions.Messages;

/// <summary>
/// Collection definition to generate ExecutionMessages static class.
/// </summary>
[MessageCollection("ExecutionMessages", ReturnType = typeof(IServiceMessage))]
public abstract class ExecutionMessageCollectionBase : MessageCollectionBase<ExecutionMessage>
{
}