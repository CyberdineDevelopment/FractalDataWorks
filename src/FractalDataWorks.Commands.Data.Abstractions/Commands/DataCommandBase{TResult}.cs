namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Abstract base class for data commands with typed result.
/// </summary>
/// <typeparam name="TResult">The type of result this command returns.</typeparam>
/// <remarks>
/// Use this base class for commands that return a specific type but don't require input data.
/// Examples: QueryCommand&lt;T&gt;, DeleteCommand (returns int).
/// </remarks>
public abstract class DataCommandBase<TResult> : DataCommandBase, IDataCommand<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase{TResult}"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for this command type.</param>
    /// <param name="name">Name of the command type (must match TypeOption attribute).</param>
    /// <param name="containerName">Name of the data container this command operates on.</param>
    /// <param name="category">The command category.</param>
    protected DataCommandBase(int id, string name, string containerName, FractalDataWorks.Commands.Abstractions.IGenericCommandCategory category)
        : base(id, name, containerName, category)
    {
    }
}
