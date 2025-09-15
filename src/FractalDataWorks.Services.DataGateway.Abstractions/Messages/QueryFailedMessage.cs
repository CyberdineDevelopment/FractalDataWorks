using FractalDataWorks.Messages;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Messages;

/// <summary>
/// Message indicating that a data query operation failed.
/// </summary>
[Message("QueryFailed")]
public sealed class QueryFailedMessage : DataGatewayMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryFailedMessage"/> class.
    /// </summary>
    public QueryFailedMessage() 
        : base(3001, "QueryFailed", MessageSeverity.Error, 
               "The query operation failed", "DATA_QUERY_FAILED") { }

    /// <summary>
    /// Initializes a new instance with specific error details.
    /// </summary>
    /// <param name="reason">The reason why the query failed.</param>
    public QueryFailedMessage(string reason) 
        : base(3001, "QueryFailed", MessageSeverity.Error, 
               $"The query operation failed: {reason}", "DATA_QUERY_FAILED") { }
}