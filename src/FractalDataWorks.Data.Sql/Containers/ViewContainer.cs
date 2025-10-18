using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Data.Sql;

/// <summary>
/// Represents a SQL Server view container.
/// </summary>
public sealed class ViewContainer : ContainerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ViewContainer"/> class.
    /// </summary>
    /// <param name="name">The container name.</param>
    /// <param name="path">The database path to the view.</param>
    /// <param name="schema">The container schema with field definitions.</param>
    public ViewContainer(string name, DatabasePath path, IContainerSchema schema)
        : base(2, name)
    {
        Path = path;
        Schema = schema;
        TranslatorTypeNames = new[] { "TSqlQuery" };
        FormatTypeNames = new[] { "Tabular", "Json" };
    }

    /// <summary>
    /// Gets the path to this view.
    /// </summary>
    public override IPath Path { get; }

    /// <summary>
    /// Gets the schema definition for this view.
    /// </summary>
    public override IContainerSchema Schema { get; }
}
