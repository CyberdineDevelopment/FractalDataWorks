using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Messages;

/// <summary>
/// Collection definition to generate FactoryMessages static class.
/// </summary>
[MessageCollection("FactoryMessages", ReturnType = typeof(IServiceMessage))]
public abstract class FactoryMessageCollectionBase : MessageCollectionBase<FactoryMessage>
{

}