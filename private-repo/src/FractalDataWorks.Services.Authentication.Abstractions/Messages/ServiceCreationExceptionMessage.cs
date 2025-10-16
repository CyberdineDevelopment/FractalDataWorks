using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Authentication.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that an exception occurred during service creation.
/// </summary>
[Message("ServiceCreationException")]
public sealed class ServiceCreationExceptionMessage : AuthenticationMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceCreationExceptionMessage"/> class.
    /// </summary>
    /// <param name="exceptionMessage">The exception message.</param>
    public ServiceCreationExceptionMessage(string exceptionMessage)
        : base(1008, "ServiceCreationException", MessageSeverity.Error,
               exceptionMessage, "AUTH_SERVICE_CREATION_FAILED") { }
}
