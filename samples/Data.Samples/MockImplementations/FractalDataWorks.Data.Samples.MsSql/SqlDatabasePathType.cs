using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MsSql;

/// <summary>
/// Sample SQL Server database path type demonstrating the TypeCollection pattern.
/// </summary>
/// <remarks>
/// In production, this would be in a package like FractalDataWorks.Data.SqlServer
/// and would include actual SQL Server path implementation logic.
/// </remarks>
[TypeOption(typeof(PathTypes), "SqlDatabase")]
public sealed class SqlDatabasePathType : PathTypeBase
{
    public SqlDatabasePathType()
        : base(
            id: 1,
            name: "SqlDatabase",
            displayName: "SQL Server Database Path",
            description: "Path to SQL Server database objects (tables, views, stored procedures)",
            domain: "Sql",
            category: "Database")
    {
    }
}
