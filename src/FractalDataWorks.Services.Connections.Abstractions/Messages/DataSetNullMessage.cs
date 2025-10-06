using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.Connections.Abstractions.Messages;

/// <summary>
/// Message indicating that the DataSet was null.
/// </summary>
[Message("DataSetNull")]
public sealed class DataSetNullMessage : ConnectionMessage, IServiceMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataSetNullMessage"/> class.
    /// </summary>
    public DataSetNullMessage()
        : base(1005, "DataSetNull", MessageSeverity.Error,
               "DataSet cannot be null", "CONN_DATASET_NULL") { }
}
