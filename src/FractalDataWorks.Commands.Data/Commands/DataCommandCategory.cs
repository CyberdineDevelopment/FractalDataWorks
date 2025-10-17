using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// Simple category for data commands.
/// This is a temporary implementation until full category system is implemented.
/// </summary>
internal sealed class DataCommandCategory : IGenericCommandCategory
{
    /// <summary>
    /// Gets the singleton Query category.
    /// </summary>
    public static readonly DataCommandCategory Query = new(1, "Query", requiresTransaction: false, isMutation: false, isCacheable: true);

    /// <summary>
    /// Gets the singleton Insert category.
    /// </summary>
    public static readonly DataCommandCategory Insert = new(2, "Insert", requiresTransaction: true, isMutation: true, isCacheable: false);

    /// <summary>
    /// Gets the singleton Update category.
    /// </summary>
    public static readonly DataCommandCategory Update = new(3, "Update", requiresTransaction: true, isMutation: true, isCacheable: false);

    /// <summary>
    /// Gets the singleton Delete category.
    /// </summary>
    public static readonly DataCommandCategory Delete = new(4, "Delete", requiresTransaction: true, isMutation: true, isCacheable: false);

    private DataCommandCategory(int id, string name, bool requiresTransaction, bool isMutation, bool isCacheable)
    {
        Id = id;
        Name = name;
        RequiresTransaction = requiresTransaction;
        IsMutation = isMutation;
        IsCacheable = isCacheable;
        SupportsStreaming = !isMutation;
        ExecutionPriority = 50;
    }

    /// <inheritdoc/>
    public int Id { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool RequiresTransaction { get; }

    /// <inheritdoc/>
    public bool SupportsStreaming { get; }

    /// <inheritdoc/>
    public bool IsCacheable { get; }

    /// <inheritdoc/>
    public bool IsMutation { get; }

    /// <inheritdoc/>
    public int ExecutionPriority { get; }

    /// <inheritdoc/>
    public string Category => string.Empty;
}
