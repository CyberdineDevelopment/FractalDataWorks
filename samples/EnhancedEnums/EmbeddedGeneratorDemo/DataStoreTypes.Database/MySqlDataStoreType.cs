using DataStore.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace DataStore.Database;

/// <summary>
/// MySQL DataStore type implementation
/// </summary>
[TypeOption(typeof(DataStoreTypes), "MySql")]
public sealed class MySqlDataStoreType : DataStoreTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MySqlDataStoreType"/> class.
    /// </summary>
    public MySqlDataStoreType() : base(3, "MySql", "Database")
    {
    }
}