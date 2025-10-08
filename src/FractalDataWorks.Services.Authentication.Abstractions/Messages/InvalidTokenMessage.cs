using FractalDataWorks.Messages;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.Authentication.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that a token is invalid or malformed.
/// </summary>
[Message("InvalidToken")]
public sealed class InvalidTokenMessage : AuthenticationMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTokenMessage"/> class.
    /// </summary>
    public InvalidTokenMessage() 
        : base(2001, "InvalidToken", MessageSeverity.Error, 
               "The provided token is invalid", "AUTH_INVALID_TOKEN") { }

    /// <summary>
    /// Initializes a new instance with specific error details.
    /// </summary>
    /// <param name="reason">The reason why the token is invalid.</param>
    public InvalidTokenMessage(string reason) 
        : base(2001, "InvalidToken", MessageSeverity.Error, 
               $"The provided token is invalid: {reason}", "AUTH_INVALID_TOKEN") { }
}