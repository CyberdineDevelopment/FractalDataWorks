using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions.Messages;

/// <summary>
/// CurrentMessage indicating that the DataReader is closed and cannot be read.
/// </summary>
[Message("DataReaderClosed")]
public sealed class DataReaderClosedMessage : ConnectionMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataReaderClosedMessage"/> class.
    /// </summary>
    public DataReaderClosedMessage()
        : base(1006, "DataReaderClosed", MessageSeverity.Error,
               "SqlDataReader is closed and cannot be read", "CONN_READER_CLOSED") { }
}
