using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Authentication.Abstractions.Messages;

/// <summary>
/// Message indicating that no factory is registered for the authentication type.
/// </summary>
[Message("NoFactoryRegistered")]
public sealed class NoFactoryRegisteredMessage : AuthenticationMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NoFactoryRegisteredMessage"/> class.
    /// </summary>
    /// <param name="authenticationType">The authentication type that has no factory registered.</param>
    public NoFactoryRegisteredMessage(string authenticationType)
        : base(1003, "NoFactoryRegistered", MessageSeverity.Error,
               $"No factory registered for authentication type: {authenticationType}", "AUTH_NO_FACTORY") { }
}
