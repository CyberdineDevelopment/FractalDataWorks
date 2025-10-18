using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Path type for SQL Server database paths (Database.Schema.Object format).
/// </summary>
[TypeOption(typeof(PathTypes), "DatabasePath")]
public sealed class DatabasePathType : PathTypeBase
{
    /// <summary>
    /// Singleton instance of DatabasePathType.
    /// </summary>
    public static readonly DatabasePathType Instance = new();

    private DatabasePathType()
        : base(
            id: 1,
            name: "DatabasePath",
            displayName: "Database Path",
            description: "Navigates to SQL Server database objects using Database.Schema.Object format",
            domain: "Sql")
    {
    }
}
