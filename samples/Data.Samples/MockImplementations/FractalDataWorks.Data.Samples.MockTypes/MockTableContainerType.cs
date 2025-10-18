using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MockTypes;

/// <summary>
/// Demonstrates the ContainerType TypeCollection pattern for SQL tables.
/// </summary>
/// <remarks>
/// <para><strong>What This Demonstrates:</strong></para>
/// <list type="bullet">
///   <item><description>
///     <strong>Schema Discovery Capability:</strong> The SupportsSchemaDiscovery flag indicates that
///     SQL tables can introspect their schema (columns, data types, constraints) at runtime.
///   </description></item>
///   <item><description>
///     <strong>Container Abstraction:</strong> Containers hold structured data (tables, views, collections,
///     API responses) - this is the metadata describing what "table container" means.
///   </description></item>
///   <item><description>
///     <strong>Separate TypeCollection:</strong> ContainerTypes is a different TypeCollection than PathTypes,
///     showing how the architecture segregates concerns (where vs. what vs. how).
///   </description></item>
///   <item><description>
///     <strong>Type-Safe Metadata:</strong> Properties like SupportsSchemaDiscovery are compile-time safe,
///     not runtime string parsing or reflection.
///   </description></item>
/// </list>
/// <para><strong>In Production Use:</strong></para>
/// <para>
/// Configuration would reference this as:
/// <code>
/// "DataSource": {
///   "ContainerType": "SqlTable",  // Resolved to this type at runtime
///   "Schema": "dbo",
///   "Table": "Customers"
/// }
/// </code>
/// </para>
/// </remarks>
[TypeOption(typeof(ContainerTypes), "SqlTable")]
public sealed class MockTableContainerType : ContainerTypeBase
{
    public MockTableContainerType()
        : base(
            id: 1,
            name: "SqlTable",
            displayName: "SQL Server Table",
            description: "Represents a SQL Server database table",
            supportsSchemaDiscovery: true)
    {
    }
}
