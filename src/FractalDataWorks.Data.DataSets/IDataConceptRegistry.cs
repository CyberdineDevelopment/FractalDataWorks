using System.Collections.Generic;
using FractalDataWorks.Data.DataSets.Abstractions;

namespace FractalDataWorks.Data.DataSets;

/// <summary>
/// Registry for managing data concept configurations.
/// </summary>
public interface IDataConceptRegistry
{
    /// <summary>
    /// Gets a data concept configuration by name.
    /// </summary>
    /// <param name="name">The concept name.</param>
    /// <returns>The data concept configuration.</returns>
    DataSetConfiguration GetDataConcept(string name);

    /// <summary>
    /// Tries to get a data concept configuration by name.
    /// </summary>
    /// <param name="name">The concept name.</param>
    /// <param name="concept">The concept if found.</param>
    /// <returns>True if the concept was found, false otherwise.</returns>
    bool TryGetDataConcept(string name, out DataSetConfiguration? concept);

    /// <summary>
    /// Gets all registered data concepts.
    /// </summary>
    /// <returns>All data concept configurations.</returns>
    IEnumerable<DataSetConfiguration> GetAllConcepts();

    /// <summary>
    /// Checks if a concept with the specified name exists.
    /// </summary>
    /// <param name="name">The concept name.</param>
    /// <returns>True if the concept exists, false otherwise.</returns>
    bool HasConcept(string name);
}
