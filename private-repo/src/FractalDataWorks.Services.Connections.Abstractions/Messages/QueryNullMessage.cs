using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that the query was null.
/// </summary>
[Message("QueryNull")]
public sealed class QueryNullMessage : ConnectionMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryNullMessage"/> class.
    /// </summary>
    public QueryNullMessage()
        : base(1004, "QueryNull", MessageSeverity.Error,
               "Query cannot be null", "CONN_QUERY_NULL") { }
}
