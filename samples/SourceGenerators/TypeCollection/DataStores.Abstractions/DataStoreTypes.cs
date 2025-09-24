using FractalDataWorks.Collections.Attributes;

namespace DataStore.Abstractions;

/// <summary>
/// Global collection for all DataStore types across all referenced assemblies.
/// The TypeCollectionGenerator will automatically discover DataStoreType options from all packages
/// that inherit from DataStoreTypeBase and generate a complete collection API.
/// </summary>
[TypeCollection("DataStore.Abstractions.DataStoreTypeBase", "DataStoreTypes")]
public partial class DataStoreTypes
{
}