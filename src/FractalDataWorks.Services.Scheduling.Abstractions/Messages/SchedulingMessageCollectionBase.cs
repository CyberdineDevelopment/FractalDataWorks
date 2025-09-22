using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Scheduling.Abstractions.Messages;

/// <summary>
/// Collection definition to generate SchedulingMessages static class.
/// </summary>
[MessageCollection("SchedulingMessages", ReturnType = typeof(IServiceMessage))]
public abstract class SchedulingMessageCollectionBase : MessageCollectionBase<SchedulingMessage>
{

}