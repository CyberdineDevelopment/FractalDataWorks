using System;
using System.Collections.Generic;
using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Messages;

/// <summary>
/// Implementation of IFractalMessage for framework messages.
/// </summary>
public class FractalMessage : IFractalMessage
{
    private int _id = 1;

    /// <inheritdoc/>
    public MessageSeverity Severity { get; set; }
    
    /// <inheritdoc/>
    public string Message { get; set; } = string.Empty;
    
    /// <inheritdoc/>
    public string? Code { get; set; }
    
    /// <inheritdoc/>
    public string? Source { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier for this message.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the display name or string representation of this enum value.
    /// </summary>
    public string Name { get; set; } = "FractalMessage";

    /// <summary>
    /// Gets or sets the timestamp when the message was created.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the correlation identifier for related messages.
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Gets or sets additional metadata for the message.
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>(StringComparer.Ordinal);
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FractalMessage"/> class.
    /// </summary>
    public FractalMessage()
    {
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FractalMessage"/> class with a message.
    /// </summary>
    /// <param name="message">The message text.</param>
    public FractalMessage(string message)
    {
        Message = message ?? string.Empty;
        Severity = MessageSeverity.Information;
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FractalMessage"/> class with full details.
    /// </summary>
    /// <param name="severity">The message severity.</param>
    /// <param name="message">The message text.</param>
    /// <param name="code">The message code.</param>
    /// <param name="source">The message source.</param>
    public FractalMessage(MessageSeverity severity, string message, string? code = null, string? source = null)
    {
        Severity = severity;
        Message = message ?? string.Empty;
        Code = code;
        Source = source;
    }

    #region Implementation of IEnumOption

    /// <summary>
    /// Gets the unique identifier for this enum value.
    /// </summary>
    int IEnumOption.Id => _id;

    #endregion
}
