using FractalDataWorks.Messages;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Messages;

/// <summary>
/// Message indicating that a database connection failed.
/// </summary>
[Message("ConnectionFailed")]
public sealed class ConnectionFailedMessage : DataGatewayMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionFailedMessage"/> class.
    /// </summary>
    public ConnectionFailedMessage() 
        : base(3002, "ConnectionFailed", MessageSeverity.Error, 
               "Failed to connect to the data source", "DATA_CONNECTION_FAILED") { }

    /// <summary>
    /// Initializes a new instance with connection details.
    /// </summary>
    /// <param name="dataSource">The data source that failed to connect.</param>
    public ConnectionFailedMessage(string dataSource) 
        : base(3002, "ConnectionFailed", MessageSeverity.Error, 
               $"Failed to connect to data source: {dataSource}", "DATA_CONNECTION_FAILED") { }
}