using DataStore.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace DataStore.Database;

/// <summary>
/// SQL Server DataStore type implementation
/// </summary>
[TypeOption("SqlServer")]
public sealed class SqlServerDataStoreType : DataStoreTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDataStoreType"/> class.
    /// </summary>
    public SqlServerDataStoreType() : base(1, "SqlServer", "Database")
    {
    }
}