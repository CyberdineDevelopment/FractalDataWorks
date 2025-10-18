using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Container type for SQL Server views.
/// </summary>
[TypeOption(typeof(ContainerTypes), "View")]
public sealed class ViewContainerType : ContainerTypeBase
{
    /// <summary>
    /// Singleton instance of ViewContainerType.
    /// </summary>
    public static readonly ViewContainerType Instance = new();

    private ViewContainerType()
        : base(
            id: 2,
            name: "View",
            displayName: "SQL View",
            description: "SQL Server view container with schema discovery support",
            supportsSchemaDiscovery: true)
    {
    }
}
