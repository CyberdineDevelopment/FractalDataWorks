using System;
using System.Collections.Generic;

namespace FractalDataWorks.Messages;

/// <summary>
/// Standard error message for general errors.
/// </summary>
[Message("StandardMessages")]
public sealed class ErrorMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorMessage"/> class.
    /// </summary>
    /// <param name="errorDescription">Description of the error that occurred.</param>
    public ErrorMessage(string errorDescription)
        : base(
            id: 1003,
            name: "Error",
            severity: MessageSeverity.Error,
            message: errorDescription,
            code: "ERROR",
            source: "General")
    {
        ErrorDescription = errorDescription;
    }

    /// <summary>
    /// Gets the description of the error.
    /// </summary>
    public string ErrorDescription { get; }
}