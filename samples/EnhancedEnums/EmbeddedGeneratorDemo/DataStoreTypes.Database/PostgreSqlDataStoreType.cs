using DataStore.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace DataStore.Database;

/// <summary>
/// PostgreSQL DataStore type implementation
/// </summary>
[TypeOption("PostgreSql")]
public sealed class PostgreSqlDataStoreType : DataStoreTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgreSqlDataStoreType"/> class.
    /// </summary>
    public PostgreSqlDataStoreType() : base(2, "PostgreSql", "Database")
    {
    }
}