using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Represents a SQL Server table container.
/// </summary>
public sealed class TableContainer : ContainerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableContainer"/> class.
    /// </summary>
    /// <param name="name">The container name.</param>
    /// <param name="path">The database path to the table.</param>
    /// <param name="schema">The container schema with field definitions.</param>
    public TableContainer(string name, DatabasePath path, IContainerSchema schema)
        : base(1, name)
    {
        Path = path;
        Schema = schema;
        TranslatorTypeNames = new[] { "TSqlQuery" };
        FormatTypeNames = new[] { "Tabular", "Json" };
    }

    /// <summary>
    /// Gets the path to this table.
    /// </summary>
    public override IPath Path { get; }

    /// <summary>
    /// Gets the schema definition for this table.
    /// </summary>
    public override IContainerSchema Schema { get; }
}
