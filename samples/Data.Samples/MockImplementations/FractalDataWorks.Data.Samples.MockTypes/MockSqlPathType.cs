using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MockTypes;

/// <summary>
/// Demonstrates the PathType TypeCollection pattern for SQL database paths.
/// </summary>
/// <remarks>
/// <para><strong>What This Demonstrates:</strong></para>
/// <list type="bullet">
///   <item><description>
///     <strong>TypeOption Pattern:</strong> The [TypeOption] attribute marks this type for automatic
///     discovery by the TypeCollectionGenerator source generator.
///   </description></item>
///   <item><description>
///     <strong>Configuration-First Design:</strong> The "SqlDatabase" name is used in appsettings.json
///     to reference this path type without coupling to the implementation class.
///   </description></item>
///   <item><description>
///     <strong>Metadata Layer:</strong> This is pure metadata - describes what "SQL database path" means,
///     not the actual path implementation (which would be in a PathBase-derived class).
///   </description></item>
///   <item><description>
///     <strong>Domain Classification:</strong> The "Sql" domain groups this with other SQL-related types
///     for architectural organization.
///   </description></item>
/// </list>
/// <para><strong>In Production Use:</strong></para>
/// <para>
/// Configuration would reference this as:
/// <code>
/// "DataConnection": {
///   "PathType": "SqlDatabase",  // Resolved to this type at runtime
///   "Path": "MyDatabase.dbo.Customers"
/// }
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(PathTypes), "SqlDatabase")]
public sealed class MockSqlPathType : PathTypeBase
{
    public MockSqlPathType()
        : base(
            id: 1,
            name: "SqlDatabase",
            displayName: "SQL Server Database Path",
            description: "Path to SQL Server database objects (tables, views, stored procedures)",
            domain: "Sql")
    {
    }
}
