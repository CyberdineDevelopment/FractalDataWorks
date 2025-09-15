using FractalDataWorks.Messages;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.Authentication.Abstractions.Messages;

/// <summary>
/// Message indicating that authentication failed.
/// </summary>
[Message("AuthenticationFailed")]
public sealed class AuthenticationFailedMessage : AuthenticationMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationFailedMessage"/> class.
    /// </summary>
    public AuthenticationFailedMessage() 
        : base(2003, "AuthenticationFailed", MessageSeverity.Error, 
               "Authentication failed", "AUTH_FAILED") { }

    /// <summary>
    /// Initializes a new instance with failure reason.
    /// </summary>
    /// <param name="reason">The reason for authentication failure.</param>
    public AuthenticationFailedMessage(string reason) 
        : base(2003, "AuthenticationFailed", MessageSeverity.Error, 
               $"Authentication failed: {reason}", "AUTH_FAILED") { }
}