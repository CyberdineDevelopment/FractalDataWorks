using FractalDataWorks.Data.DataStores.Abstractions;

namespace FractalDataWorks.Data.DataStores.Rest;

/// <summary>
/// DataStore type definition for REST API storage backend.
/// </summary>
public sealed class RestDataStoreType : DataStoreTypeBase
{
    /// <summary>
    /// Gets the singleton instance of the REST data store type.
    /// </summary>
    public static RestDataStoreType Instance { get; } = new();

    private RestDataStoreType() : base(
        id: 2,
        name: "Rest",
        displayName: "REST API",
        description: "REST API data store supporting JSON, XML, and custom formats",
        supportsRead: true,
        supportsWrite: true,
        supportsTransactions: false,
        category: "Web")
    {
    }
}
