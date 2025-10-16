using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.SecretManagers.Abstractions.Messages;

/// <summary>
/// Collection definition to generate SecretManagerMessages static class.
/// </summary>
[MessageCollection("SecretManagerMessages", ReturnType = typeof(IServiceMessage))]
public abstract class SecretManagerMessageCollectionBase : MessageCollectionBase<SecretManagerMessage>
{
}
