using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Represents a SQL Server stored procedure container.
/// </summary>
public sealed class StoredProcedureContainer : ContainerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StoredProcedureContainer"/> class.
    /// </summary>
    /// <param name="name">The container name.</param>
    /// <param name="path">The database path to the stored procedure.</param>
    /// <param name="schema">The container schema with parameter and result set definitions.</param>
    public StoredProcedureContainer(string name, DatabasePath path, IContainerSchema schema)
        : base(3, name)
    {
        Path = path;
        Schema = schema;
        TranslatorTypeNames = new[] { "TSqlSproc" };
        FormatTypeNames = new[] { "Tabular", "Json" };
    }

    /// <summary>
    /// Gets the path to this stored procedure.
    /// </summary>
    public override IPath Path { get; }

    /// <summary>
    /// Gets the schema definition for this stored procedure.
    /// </summary>
    public override IContainerSchema Schema { get; }
}
