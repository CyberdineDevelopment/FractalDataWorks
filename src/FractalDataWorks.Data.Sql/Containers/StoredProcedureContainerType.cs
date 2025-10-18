using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Container type for SQL Server stored procedures.
/// </summary>
[TypeOption(typeof(ContainerTypes), "StoredProcedure")]
public sealed class StoredProcedureContainerType : ContainerTypeBase
{
    /// <summary>
    /// Singleton instance of StoredProcedureContainerType.
    /// </summary>
    public static readonly StoredProcedureContainerType Instance = new();

    private StoredProcedureContainerType()
        : base(
            id: 3,
            name: "StoredProcedure",
            displayName: "SQL Stored Procedure",
            description: "SQL Server stored procedure container with parameter discovery",
            supportsSchemaDiscovery: true)
    {
    }
}
