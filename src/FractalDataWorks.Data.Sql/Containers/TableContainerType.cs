using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Container type for SQL Server tables.
/// </summary>
[TypeOption(typeof(ContainerTypes), "Table")]
public sealed class TableContainerType : ContainerTypeBase
{
    /// <summary>
    /// Singleton instance of TableContainerType.
    /// </summary>
    public static readonly TableContainerType Instance = new();

    private TableContainerType()
        : base(
            id: 1,
            name: "Table",
            displayName: "SQL Table",
            description: "SQL Server table container with full schema discovery support",
            supportsSchemaDiscovery: true)
    {
    }
}
