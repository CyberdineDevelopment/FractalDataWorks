namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Abstract base class for data commands with typed input and typed result.
/// </summary>
/// <typeparam name="TResult">The type of result this command returns.</typeparam>
/// <typeparam name="TInput">The type of input data this command requires.</typeparam>
/// <remarks>
/// Use this base class for commands that require input data and return a specific type.
/// Examples: InsertCommand&lt;T&gt;, UpdateCommand&lt;T&gt;, BulkInsertCommand&lt;T&gt;.
/// </remarks>
public abstract class DataCommandBase<TResult, TInput> : DataCommandBase<TResult>, IDataCommand<TResult, TInput>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandBase{TResult, TInput}"/> class.
    /// </summary>
    /// <param name="id">Unique identifier for this command type.</param>
    /// <param name="name">Name of the command type (must match TypeOption attribute).</param>
    /// <param name="containerName">Name of the data container this command operates on.</param>
    /// <param name="category">The command category.</param>
    /// <param name="data">The input data for this command.</param>
    protected DataCommandBase(int id, string name, string containerName, FractalDataWorks.Commands.Abstractions.IGenericCommandCategory category, TInput data)
        : base(id, name, containerName, category)
    {
        Data = data;
    }

    /// <summary>
    /// Gets the input data for this command.
    /// </summary>
    public TInput Data { get; }
}
