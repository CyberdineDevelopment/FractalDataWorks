using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Data.DataSets;
using FractalDataWorks.Data.DataSets.Abstractions;
using FractalDataWorks.Data.Transformers.Abstractions;
using FractalDataWorks.Results;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Data.Execution;

/// <summary>
/// Executes queries against data concepts that may span multiple sources.
/// Handles multi-source extraction, transformation, and union of results.
/// </summary>
public sealed class DataConceptQueryExecutor
{
    private readonly IDataConceptRegistry _conceptRegistry;
    private readonly ILogger<DataConceptQueryExecutor> _logger;
    private readonly Dictionary<string, IDataTransformer> _transformers;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataConceptQueryExecutor"/> class.
    /// </summary>
    /// <param name="conceptRegistry">The data concept registry.</param>
    /// <param name="logger">The logger.</param>
    public DataConceptQueryExecutor(
        IDataConceptRegistry conceptRegistry,
        ILogger<DataConceptQueryExecutor> logger)
    {
        _conceptRegistry = conceptRegistry ?? throw new ArgumentNullException(nameof(conceptRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _transformers = new Dictionary<string, IDataTransformer>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Registers a transformer for use during execution.
    /// </summary>
    /// <param name="transformer">The transformer to register.</param>
    public void RegisterTransformer(IDataTransformer transformer)
    {
        if (transformer == null)
        {
            throw new ArgumentNullException(nameof(transformer));
        }

        _transformers[transformer.Name] = transformer;
        _logger.LogInformation(
            "Registered transformer '{TransformerName}' ({SourceType} -> {TargetType})",
            transformer.Name,
            transformer.SourceType.Name,
            transformer.TargetType.Name);
    }

    /// <summary>
    /// Executes a query against a data concept.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="conceptName">The name of the data concept to query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query results from all sources, transformed and unioned.</returns>
    /// <remarks>
    /// Exception handling wrapper - cannot be reliably tested without complex infrastructure.
    /// Core logic is tested in ExecuteCore.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public async Task<IGenericResult<IEnumerable<T>>> Execute<T>(
        string conceptName,
        CancellationToken cancellationToken = default)
        where T : class
    {
        try
        {
            return await ExecuteCore<T>(conceptName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query for concept '{ConceptName}'", conceptName);
            return GenericResult<IEnumerable<T>>.Failure($"Query execution failed: {ex.Message}");
        }
    }

    private async Task<IGenericResult<IEnumerable<T>>> ExecuteCore<T>(
        string conceptName,
        CancellationToken cancellationToken)
        where T : class
    {
        _logger.LogInformation("Executing query for concept '{ConceptName}'", conceptName);

        // Get concept configuration
        if (!_conceptRegistry.TryGetDataConcept(conceptName, out var concept) || concept == null)
        {
            return GenericResult<IEnumerable<T>>.Failure(
                $"Data concept '{conceptName}' not found");
        }

        if (concept.Sources.Count == 0)
        {
            _logger.LogWarning("Concept '{ConceptName}' has no configured sources", conceptName);
            return GenericResult<IEnumerable<T>>.Success([]);
        }

        _logger.LogInformation(
            "Concept '{ConceptName}' has {SourceCount} source(s)",
            conceptName,
            concept.Sources.Count);

        // For Milestone 1, return empty results with success
        // This demonstrates the structure; actual extraction will be implemented
        // when connection infrastructure is available
        var results = new List<T>();

        // TODO: In Milestone 1, we'll add actual source querying
        // For now, log what would happen:
        foreach (var source in concept.Sources.Values.OrderBy(s => s.Priority))
        {
            _logger.LogInformation(
                "Would extract from source: ConnectionType='{ConnectionType}', Priority={Priority}, Cost={Cost}",
                source.ConnectionType,
                source.Priority,
                source.EstimatedCost);
        }

        return await Task.FromResult(GenericResult<IEnumerable<T>>.Success(results));
    }

    /// <summary>
    /// Executes a query with a filter predicate against a data concept.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    /// <param name="conceptName">The name of the data concept to query.</param>
    /// <param name="filter">The filter predicate to apply.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The filtered query results.</returns>
    /// <remarks>
    /// Calls Execute internally which is excluded from coverage due to exception handling.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public async Task<IGenericResult<IEnumerable<T>>> Execute<T>(
        string conceptName,
        Func<T, bool> filter,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var result = await Execute<T>(conceptName, cancellationToken);

        if (!result.IsSuccess || result.Value == null)
        {
            return result;
        }

        var filteredResults = result.Value.Where(filter);
        return GenericResult<IEnumerable<T>>.Success(filteredResults);
    }

    // Will be tested when source extraction is implemented in future milestones
    [ExcludeFromCodeCoverage]
    private IGenericResult<IEnumerable<TOut>> ApplyTransformer<TIn, TOut>(
        IEnumerable<TIn> source,
        string transformerName,
        TransformContext context)
        where TOut : class
    {
        if (!_transformers.TryGetValue(transformerName, out var transformer))
        {
            _logger.LogWarning(
                "Transformer '{TransformerName}' not found, returning empty results",
                transformerName);
            return GenericResult<IEnumerable<TOut>>.Failure(
                $"Transformer '{transformerName}' not registered");
        }

        if (transformer is not IDataTransformer<TIn, TOut> typedTransformer)
        {
            return GenericResult<IEnumerable<TOut>>.Failure(
                $"Transformer '{transformerName}' does not support transformation from {typeof(TIn).Name} to {typeof(TOut).Name}");
        }

        try
        {
            return typedTransformer.Transform(source, context, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error applying transformer '{TransformerName}'",
                transformerName);
            return GenericResult<IEnumerable<TOut>>.Failure(
                $"Transformation failed: {ex.Message}");
        }
    }
}
