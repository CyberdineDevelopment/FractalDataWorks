using FractalDataWorks.DataStores.Abstractions;

namespace FractalDataWorks.DataStores.SqlServer;

/// <summary>
/// DataStore type definition for SQL Server storage backend.
/// </summary>
public sealed class SqlServerDataStoreType : DataStoreTypeBase
{
    /// <summary>
    /// Gets the singleton instance of the SQL Server data store type.
    /// </summary>
    public static SqlServerDataStoreType Instance { get; } = new();

    private SqlServerDataStoreType() : base(
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
