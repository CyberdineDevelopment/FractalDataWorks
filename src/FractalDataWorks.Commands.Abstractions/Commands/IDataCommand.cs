namespace FractalDataWorks.Commands.Abstractions.Commands;

/// <summary>
/// Base interface for data-related commands.
/// </summary>
/// <remarks>
/// Data commands represent operations on data such as queries, mutations,
/// and bulk operations. All data commands inherit from this interface.
/// </remarks>
public interface IDataCommand : ICommand
{
    /// <summary>
    /// Gets the target data source identifier.
    /// </summary>
    /// <value>Identifier for the data source this command targets.</value>
    string? DataSource { get; }

    /// <summary>
    /// Gets the target schema for this command.
    /// </summary>
    /// <value>Schema information for command validation and translation.</value>
    IDataSchema? TargetSchema { get; }

    /// <summary>
    /// Gets whether this command requires a transaction.
    /// </summary>
    /// <value>True if the command must execute within a transaction.</value>
    bool RequiresTransaction { get; }

    /// <summary>
    /// Gets the command timeout in milliseconds.
    /// </summary>
    /// <value>Maximum time allowed for command execution.</value>
    int TimeoutMs { get; }
}