using FractalDataWorks.Data.DataStores.Abstractions;

namespace FractalDataWorks.Data.DataStores.SqlServer;

/// <summary>
/// DataStore type definition for SQL Server storage backend.
/// </summary>
public sealed class SqlServerDataStoreType : DataStoreTypeBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlServerDataStoreType"/> class.
    /// </summary>
    public SqlServerDataStoreType() : base(
        id: 1,
        name: "SqlServer",
        displayName: "SQL Server",
        description: "Microsoft SQL Server relational database supporting T-SQL, JSON, and XML",
        supportsRead: true,
        supportsWrite: true,
        supportsTransactions: true,
        category: "Database")
    {
    }
}
