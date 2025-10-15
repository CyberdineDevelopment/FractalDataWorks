using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Connections.Abstractions.Messages;

namespace FractalDataWorks.Services.Connections.Http.Abstractions.Messages;

/// <summary>
/// Collection definition to generate HttpMessages static class.
/// </summary>
[MessageCollection("HttpMessages", ReturnType = typeof(IGenericMessage))]
public abstract class HttpMessageCollectionBase : MessageCollectionBase<ConnectionMessage>
{
}