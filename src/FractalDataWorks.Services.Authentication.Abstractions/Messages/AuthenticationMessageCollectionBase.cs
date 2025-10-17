using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Authentication.Abstractions.Messages;

/// <summary>
/// Collection definition to generate AuthenticationMessages static class.
/// </summary>
[MessageCollection("AuthenticationMessages", ReturnType = typeof(IServiceMessage))]
public abstract class AuthenticationMessageCollectionBase : MessageCollectionBase<AuthenticationMessage>
{

}