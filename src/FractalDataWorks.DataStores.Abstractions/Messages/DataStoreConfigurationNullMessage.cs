using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.DataStores.Abstractions.Messages;

[Message("DataStoreConfigurationNull")]
public sealed class DataStoreConfigurationNullMessage : DataStoreMessage
{
    public DataStoreConfigurationNullMessage()
        : base(1002, "DataStoreConfigurationNull", MessageSeverity.Error,
               "Configuration cannot be null", "DS_CONFIG_NULL") { }
}
