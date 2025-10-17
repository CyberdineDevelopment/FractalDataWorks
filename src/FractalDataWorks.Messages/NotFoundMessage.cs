using System;
using System.Collections.Generic;

namespace FractalDataWorks.Messages;

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