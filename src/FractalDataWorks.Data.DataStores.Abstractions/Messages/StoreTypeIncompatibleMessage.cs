using FractalDataWorks.Messages;

namespace FractalDataWorks.Data.DataStores.Abstractions.Messages;

/// <summary>
/// Message indicating that a store type is incompatible with a connection type.
/// </summary>
public sealed class StoreTypeIncompatibleMessage : DataStoreMessage
{
    /// <summary>
    /// Gets the singleton instance of this message.
    /// </summary>
    public static StoreTypeIncompatibleMessage Instance { get; } = new();

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
