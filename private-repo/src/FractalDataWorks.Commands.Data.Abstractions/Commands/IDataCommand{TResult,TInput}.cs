namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Data command with typed input and typed result.
/// Use this interface for commands that require input data and return a specific result type.
/// </summary>
/// <typeparam name="TResult">The type of result this command returns.</typeparam>
/// <typeparam name="TInput">The type of input data this command requires.</typeparam>
/// <remarks>
/// <para>
/// This interface provides compile-time type safety for both input and result, eliminating all casting.
/// </para>
/// <para>
/// Examples:
/// <list type="bullet">
/// <item>InsertCommand&lt;Customer&gt; accepts Customer, returns int (identity)</item>
/// <item>UpdateCommand&lt;Customer&gt; accepts Customer, returns int (affected rows)</item>
/// <item>BulkInsertCommand&lt;Customer&gt; accepts IEnumerable&lt;Customer&gt;, returns BulkInsertResult</item>
/// </list>
/// </para>
/// </remarks>
public interface IDataCommand<TResult, TInput> : IDataCommand<TResult>
{
    /// <summary>
    /// Gets the input data for this command.
    /// </summary>
    /// <value>The typed input data.</value>
    TInput Data { get; }
}
