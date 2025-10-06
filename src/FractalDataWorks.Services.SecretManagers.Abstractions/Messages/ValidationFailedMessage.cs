using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.SecretManagers.Abstractions.Messages;

/// <summary>
/// Message indicating that validation failed.
/// </summary>
[Message("ValidationFailed")]
public sealed class ValidationFailedMessage : SecretManagerMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationFailedMessage"/> class.
    /// </summary>
    /// <param name="errorMessage">The validation error message.</param>
    public ValidationFailedMessage(string errorMessage)
        : base(1004, "ValidationFailed", MessageSeverity.Error,
               errorMessage, "SM_VALIDATION_FAILED") { }
}
