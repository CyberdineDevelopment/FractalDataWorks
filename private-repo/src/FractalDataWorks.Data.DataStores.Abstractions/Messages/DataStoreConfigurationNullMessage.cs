using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Data.DataStores.Abstractions.Messages;

/// <summary>
/// Error message indicating that a data store configuration is null.
/// </summary>
[Message("DataStoreConfigurationNull")]
public sealed class DataStoreConfigurationNullMessage : DataStoreMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataStoreConfigurationNullMessage"/> class.
    /// </summary>
    public DataStoreConfigurationNullMessage()
        : base(1002, "DataStoreConfigurationNull", MessageSeverity.Error,
               "Configuration cannot be null", "DS_CONFIG_NULL") { }
}
