using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Samples.MsSql;

/// <summary>
/// Sample SQL Server table container type demonstrating the TypeCollection pattern.
/// </summary>
/// <remarks>
/// In production, this would include actual SQL Server table introspection logic
/// and schema discovery capabilities.
/// </remarks>
[TypeOption(typeof(ContainerTypes), "SqlTable")]
public sealed class SqlTableContainerType : ContainerTypeBase
{
    public SqlTableContainerType()
        : base(
            id: 1,
            name: "SqlTable",
            displayName: "SQL Server Table",
            description: "Represents a SQL Server database table with schema discovery",
            supportsSchemaDiscovery: true,
            category: "Table")
    {
    }
}
