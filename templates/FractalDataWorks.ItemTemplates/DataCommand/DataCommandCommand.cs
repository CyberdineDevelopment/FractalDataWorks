using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Commands.Data.Abstractions;

namespace FractalDataWorks.Commands.Data;

/// <summary>
/// DataCommand command for [DESCRIBE OPERATION].
/// </summary>
/// <typeparam name="T">The type of entity.</typeparam>
#if (HasInputData)
[TypeOption(typeof(DataCommands), "DataCommand")]
public sealed class DataCommandCommand<T> : DataCommandBase<TResult, TInput>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandCommand{T}"/> class.
    /// </summary>
    /// <param name="containerName">The name of the container.</param>
    /// <param name="data">The input data.</param>
    public DataCommandCommand(string containerName, TInput data)
        : base(
            id: 99,
            name: "DataCommand",
            containerName,
            DataCommandCategory.Category,
            data)
    {
    }
#else
[TypeOption(typeof(DataCommands), "DataCommand")]
public sealed class DataCommandCommand<T> : DataCommandBase<TResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataCommandCommand{T}"/> class.
    /// </summary>
    /// <param name="containerName">The name of the container.</param>
    public DataCommandCommand(string containerName)
        : base(
            id: 99,
            name: "DataCommand",
            containerName,
            DataCommandCategory.Category)
    {
    }
#endif

#if (HasFilter)
    /// <summary>
    /// Gets or sets the filter expression (WHERE clause).
    /// </summary>
    public IFilterExpression? Filter { get; init; }

#endif
#if (HasProjection)
    /// <summary>
    /// Gets or sets the projection expression (SELECT clause).
    /// </summary>
    public IProjectionExpression? Projection { get; init; }

#endif
#if (HasOrdering)
    /// <summary>
    /// Gets or sets the ordering expression (ORDER BY clause).
    /// </summary>
    public IOrderingExpression? Ordering { get; init; }

#endif
#if (HasPaging)
    /// <summary>
    /// Gets or sets the paging expression (SKIP/TAKE).
    /// </summary>
    public IPagingExpression? Paging { get; init; }

#endif
}
