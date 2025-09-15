using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions.Messages;

/// <summary>
/// Collection definition to generate ConnectionMessages static class.

/// </summary>
[MessageCollection("ConnectionMessages", ReturnType = typeof(IServiceMessage))]
public abstract class ConnectionMessageCollectionBase : MessageCollectionBase<ConnectionMessage>
{

}