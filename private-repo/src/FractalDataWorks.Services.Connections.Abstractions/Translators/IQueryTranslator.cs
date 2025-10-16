using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FractalDataWorks.Data.DataSets.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.Services.Connections.Abstractions.Translators;

/// <summary>
/// Interface for query translators that convert LINQ queries to connection-specific commands.
/// </summary>
public interface IQueryTranslator
{
    /// <summary>
    /// Gets the connection type this translator supports.
    /// </summary>
    string ConnectionType { get; }

    /// <summary>
    /// Gets the container types supported by this translator.
    /// </summary>
    IEnumerable<string> SupportedContainerTypes { get; }

    /// <summary>
    /// Translates a LINQ query into a connection-specific command.
    /// </summary>
    /// <param name="query">The query to translate.</param>
    /// <param name="dataSet">The dataset being queried.</param>
    /// <param name="containerType">The type of container (table, view, etc.).</param>
    /// <returns>A result containing the translated connection command.</returns>
    Task<IGenericResult<IConnectionCommand>> TranslateAsync(
        IDataQuery query,
        IDataSetType dataSet,
        string containerType);

    /// <summary>
    /// Validates whether a query can be translated by this translator.
    /// </summary>
    /// <param name="query">The query to validate.</param>
    /// <param name="dataSet">The dataset being queried.</param>
    /// <param name="containerType">The type of container (table, view, etc.).</param>
    /// <returns>A result indicating whether the query is valid.</returns>
    Task<IGenericResult> ValidateQueryAsync(
        IDataQuery query,
        IDataSetType dataSet,
        string containerType);

    /// <summary>
    /// Gets the capabilities of this translator.
    /// </summary>
    /// <returns>The translator capabilities.</returns>
    TranslatorCapabilities GetCapabilities();
}
