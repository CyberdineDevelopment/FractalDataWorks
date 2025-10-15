namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Data command with typed result.
/// Use this interface for commands that return a specific result type without requiring input data.
/// </summary>
/// <typeparam name="TResult">The type of result this command returns.</typeparam>
/// <remarks>
/// <para>
/// This interface provides compile-time type safety for command results, eliminating the need for casting.
/// </para>
/// <para>
/// Examples:
/// <list type="bullet">
/// <item>QueryCommand&lt;Customer&gt; returns IEnumerable&lt;Customer&gt;</item>
/// <item>DeleteCommand returns int (affected rows)</item>
/// </list>
/// </para>
/// </remarks>
public interface IDataCommand<TResult> : IDataCommand
{
    // Marker interface for type-safe result
    // No additional members needed - type parameter provides compile-time safety
}
