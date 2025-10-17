using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Authentication.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that a token was null or empty.
/// </summary>
[Message("TokenNullOrEmpty")]
public sealed class TokenNullOrEmptyMessage : AuthenticationMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TokenNullOrEmptyMessage"/> class.
    /// </summary>
    public TokenNullOrEmptyMessage()
        : base(2001, "TokenNullOrEmpty", MessageSeverity.Error,
               "Token cannot be null or empty", "AUTH_TOKEN_NULL") { }
}
