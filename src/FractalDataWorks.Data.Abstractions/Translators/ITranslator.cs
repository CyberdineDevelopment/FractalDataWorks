using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Interface for translators that convert universal data requests to domain-specific queries.
/// Examples: TSqlQueryTranslator, ODataTranslator, GraphQLTranslator
/// </summary>
public interface ITranslator
{
    /// <summary>
    /// Domain name this translator targets (Sql, Rest, GraphQL, File).
    /// </summary>
    string DomainName { get; }

    /// <summary>
    /// Whether this translator supports the given container schema.
    /// </summary>
    /// <param name="schema">The container schema to check.</param>
    /// <returns>True if this translator can handle the schema.</returns>
    bool SupportsSchema(IContainerSchema schema);

    /// <summary>
    /// Translates a data request into a domain-specific query string.
    /// </summary>
    /// <param name="container">The container being queried.</param>
    /// <param name="request">The data request (filters, projections, aggregations).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the translated query.</returns>
    Task<IGenericResult<string>> Translate(
        IContainer container,
        IDataRequest request,
        CancellationToken cancellationToken = default);
}
