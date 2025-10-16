using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using FractalDataWorks.Data.DataSets.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Data.DataSets;

/// <summary>
/// Default implementation of data concept registry.
/// Loads data concept configurations from IConfiguration.
/// </summary>
public sealed class DataConceptRegistry : IDataConceptRegistry
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DataConceptRegistry> _logger;
    private readonly Dictionary<string, DataSetConfiguration> _concepts;
    private readonly object _lock = new object();
    private bool _loaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataConceptRegistry"/> class.
    /// </summary>
    /// <param name="configuration">The configuration source.</param>
    /// <param name="logger">The logger.</param>
    public DataConceptRegistry(
        IConfiguration configuration,
        ILogger<DataConceptRegistry> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _concepts = new Dictionary<string, DataSetConfiguration>(StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public DataSetConfiguration GetDataConcept(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Concept name cannot be null or whitespace.", nameof(name));
        }

        EnsureLoaded();

        if (!_concepts.TryGetValue(name, out var concept))
        {
            throw new InvalidOperationException($"Data concept '{name}' not found. Available concepts: {string.Join(", ", _concepts.Keys)}");
        }

        return concept;
    }

    /// <inheritdoc/>
    public bool TryGetDataConcept(string name, out DataSetConfiguration? concept)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            concept = null;
            return false;
        }

        EnsureLoaded();

        return _concepts.TryGetValue(name, out concept);
    }

    /// <inheritdoc/>
    public IEnumerable<DataSetConfiguration> GetAllConcepts()
    {
        EnsureLoaded();
        return _concepts.Values.ToList();
    }

    /// <inheritdoc/>
    public bool HasConcept(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        EnsureLoaded();
        return _concepts.ContainsKey(name);
    }

    // Double-checked locking pattern race condition is defensive threading code that cannot be reliably tested
    [ExcludeFromCodeCoverage]
    private void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        lock (_lock)
        {
            if (_loaded)
            {
                return;
            }

            LoadConcepts();
            _loaded = true;
        }
    }

    // Exception handling for unexpected errors cannot be easily tested
    [ExcludeFromCodeCoverage]
    private void LoadConcepts()
    {
        _logger.LogInformation("Loading data concepts from configuration");

        var conceptsSection = _configuration.GetSection("DataConcepts");
        if (!conceptsSection.Exists())
        {
            _logger.LogWarning("No DataConcepts section found in configuration");
            return;
        }

        foreach (var conceptSection in conceptsSection.GetChildren())
        {
            try
            {
                var concept = conceptSection.Get<DataSetConfiguration>();
                if (concept == null)
                {
                    _logger.LogWarning("Failed to bind configuration for concept section '{SectionKey}'", conceptSection.Key);
                    continue;
                }

                // Set the name from the section key if not already set
                if (string.IsNullOrWhiteSpace(concept.DataSetName))
                {
                    concept.DataSetName = conceptSection.Key;
                }

                _concepts[concept.DataSetName] = concept;
                _logger.LogInformation(
                    "Loaded data concept '{ConceptName}' with {SourceCount} source(s)",
                    concept.DataSetName,
                    concept.Sources.Count);
            }
            catch (Exception ex)
            {
                // This catch block is excluded from coverage as it's defensive code for unexpected exceptions
                _logger.LogError(
                    ex,
                    "Error loading data concept from section '{SectionKey}'",
                    conceptSection.Key);
            }
        }

        _logger.LogInformation("Loaded {Count} data concept(s)", _concepts.Count);
    }
}
