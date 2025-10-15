using FractalDataWorks.Messages;
using FractalDataWorks.Messages.Attributes;

namespace FractalDataWorks.Data.DataStores.Abstractions.Messages;

/// <summary>
/// Message indicating that a store type is incompatible with a connection type.
/// </summary>
[Message("StoreTypeIncompatible")]
public sealed class StoreTypeIncompatibleMessage : DataStoreMessage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoreTypeIncompatibleMessage"/> class.
    /// </summary>
    public StoreTypeIncompatibleMessage()
        : base(
            id: 1005,
            name: "StoreTypeIncompatible",
            severity: MessageSeverity.Error,
            message: "Store type '{0}' is not compatible with connection type '{1}'. Compatible types: {2}",
            code: "DS_TYPE_INCOMPATIBLE")
    {
    }
}
