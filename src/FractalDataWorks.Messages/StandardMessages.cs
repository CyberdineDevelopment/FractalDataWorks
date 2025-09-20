using System;
using System.Collections.Generic;

namespace FractalDataWorks.Messages;

/// <summary>
/// Standard error message for argument null exceptions.
/// </summary>
[Message("StandardMessages")]
public sealed class ArgumentNullMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentNullMessage"/> class.
    /// </summary>
    /// <param name="parameterName">The name of the null parameter.</param>
    public ArgumentNullMessage(string parameterName)
        : base(
            id: 1001,
            name: "ArgumentNull",
            severity: MessageSeverity.Error,
            message: $"Parameter '{parameterName}' cannot be null",
            code: "ARG_NULL",
            source: "ArgumentValidation")
    {
        ParameterName = parameterName;
    }

    /// <summary>
    /// Gets the name of the parameter that was null.
    /// </summary>
    public string ParameterName { get; }
}

/// <summary>
/// Standard error message for resource not found conditions.
/// </summary>
[Message("StandardMessages")]
public sealed class NotFoundMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundMessage"/> class.
    /// </summary>
    /// <param name="resourceDescription">Description of the resource that was not found.</param>
    public NotFoundMessage(string resourceDescription)
        : base(
            id: 1002,
            name: "NotFound",
            severity: MessageSeverity.Error,
            message: resourceDescription,
            code: "NOT_FOUND",
            source: "ResourceLookup")
    {
        ResourceDescription = resourceDescription;
    }

    /// <summary>
    /// Gets the description of the resource that was not found.
    /// </summary>
    public string ResourceDescription { get; }
}

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

/// <summary>
/// Standard error message for validation failures.
/// </summary>
[Message("StandardMessages")]
public sealed class ValidationMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationMessage"/> class.
    /// </summary>
    /// <param name="validationError">Description of the validation error.</param>
    public ValidationMessage(string validationError)
        : base(
            id: 1004,
            name: "Validation",
            severity: MessageSeverity.Error,
            message: validationError,
            code: "VALIDATION",
            source: "Validation")
    {
        ValidationError = validationError;
    }

    /// <summary>
    /// Gets the description of the validation error.
    /// </summary>
    public string ValidationError { get; }
}

/// <summary>
/// Standard error message for not implemented features.
/// </summary>
[Message("StandardMessages")]
public sealed class NotImplementedMessage : MessageTemplate<MessageSeverity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotImplementedMessage"/> class.
    /// </summary>
    /// <param name="featureDescription">Description of the feature that is not implemented.</param>
    public NotImplementedMessage(string featureDescription)
        : base(
            id: 1005,
            name: "NotImplemented",
            severity: MessageSeverity.Error,
            message: featureDescription,
            code: "NOT_IMPLEMENTED",
            source: "Implementation")
    {
        FeatureDescription = featureDescription;
    }

    /// <summary>
    /// Gets the description of the feature that is not implemented.
    /// </summary>
    public string FeatureDescription { get; }
}