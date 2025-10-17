using System.Collections.Generic;
using FractalDataWorks.Commands.Abstractions;

namespace FractalDataWorks.Commands.Data.Abstractions;

/// <summary>
/// Base interface for all data commands.
/// Data commands extend IGenericCommand and can be submitted anywhere IGenericCommand is accepted.
/// </summary>
/// <remarks>
/// <para>
/// This is the non-generic marker interface used by TypeCollection source generators.
/// For type-safe execution, use <see cref="IDataCommand{TResult}"/> or <see cref="IDataCommand{TResult, TInput}"/>.
/// </para>
/// <para>
/// Data commands represent universal data operations that work across all connection types:
/// SQL, REST, File, GraphQL, etc. Translators convert IDataCommand to domain-specific commands.
/// </para>
/// </remarks>
public interface IDataCommand : IGenericCommand
{
    /// <summary>
    /// Gets the container name (table, collection, endpoint, file path).
    /// </summary>
    /// <value>
    /// The name of the data container this command operates on.
    /// Examples: "Customers", "orders", "/api/products", "customers.csv"
    /// </value>
    string ContainerName { get; }

    /// <summary>
    /// Gets metadata for the command (connection hints, caching, etc.).
    /// </summary>
    /// <value>A read-only dictionary of metadata key-value pairs.</value>
    IReadOnlyDictionary<string, object> Metadata { get; }
}
