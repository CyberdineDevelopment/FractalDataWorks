using System.Collections.Generic;
using FractalDataWorks.Services.Abstractions;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Interface for data provider service type definitions.
/// Provides data provider-specific metadata on top of the base service type.
/// </summary>
public interface IDataGatewayServiceType : IServiceType
{
    /// <summary>
    /// Gets the data store types supported by this provider.
    /// </summary>
    /// <value>An array of data store identifiers this provider can handle.</value>
    /// <remarks>
    /// Examples: ["MsSql", "AzureSql"] for SQL Server provider,
    /// ["PostgreSQL", "AuroraPostgreSQL"] for PostgreSQL provider,
    /// ["MongoDB", "CosmosDB"] for document database providers.
    /// Used by the DataGateway to route commands to appropriate connection types
    /// based on datastore configuration.
    /// </remarks>
    string[] SupportedDataStores { get; }

    /// <summary>
    /// Gets the priority of this data provider for datastore selection.
    /// </summary>
    /// <value>An integer representing selection priority (higher values = higher priority).</value>
    /// <remarks>
    /// When multiple data providers support the same datastore,
    /// the DataGateway selects the one with the highest priority.
    /// Use this to prefer newer/optimized providers over legacy ones.
    /// </remarks>
    int Priority { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports bulk operations.
    /// </summary>
    bool SupportsBulkOperations { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports transactions.
    /// </summary>
    bool SupportsTransactions { get; }

    /// <summary>
    /// Gets a value indicating whether this provider supports streaming results.
    /// </summary>
    bool SupportsStreaming { get; }

    /// <summary>
    /// Gets the maximum batch size for bulk operations.
    /// </summary>
    /// <value>The maximum number of records in a single batch, or 0 if unlimited.</value>
    int MaxBatchSize { get; }
}