using FractalDataWorks.Messages;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.Authentication.Abstractions.Messages;

/// <summary>
/// Message indicating that a refresh token is invalid.
/// </summary>
[Message("RefreshTokenInvalid")]
public sealed class RefreshTokenInvalidMessage : AuthenticationMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenInvalidMessage"/> class.
    /// </summary>
    public RefreshTokenInvalidMessage() 
        : base(2004, "RefreshTokenInvalid", MessageSeverity.Error, 
               "The refresh token is invalid or has been revoked", "AUTH_REFRESH_TOKEN_INVALID") { }
}