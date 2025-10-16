using System;
using System.Collections.Generic;

namespace FractalDataWorks.Services.Connections.Abstractions.Translators;

/// <summary>
/// Describes the capabilities and limitations of a query translator.
/// </summary>
public sealed class TranslatorCapabilities
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorCapabilities"/> class.
    /// </summary>
    /// <param name="supportedOperations">The LINQ operations supported by this translator.</param>
    /// <param name="maxComplexity">The maximum query complexity allowed.</param>
    /// <param name="supportsJoins">Whether the translator supports joins.</param>
    /// <param name="supportsAggregation">Whether the translator supports aggregation.</param>
    /// <param name="limitations">Known limitations of this translator.</param>
    public TranslatorCapabilities(
        IEnumerable<string> supportedOperations,
        int maxComplexity,
        bool supportsJoins,
        bool supportsAggregation,
        IEnumerable<string> limitations)
    {
        SupportedOperations = new List<string>(supportedOperations ?? []).AsReadOnly();
        MaxComplexity = maxComplexity;
        SupportsJoins = supportsJoins;
        SupportsAggregation = supportsAggregation;
        Limitations = new List<string>(limitations ?? []).AsReadOnly();
    }

    /// <summary>
    /// Gets the LINQ operations supported by this translator.
    /// </summary>
    public IReadOnlyList<string> SupportedOperations { get; }

    /// <summary>
    /// Gets the maximum query complexity allowed.
    /// </summary>
    public int MaxComplexity { get; }

    /// <summary>
    /// Gets a value indicating whether the translator supports joins.
    /// </summary>
    public bool SupportsJoins { get; }

    /// <summary>
    /// Gets a value indicating whether the translator supports aggregation.
    /// </summary>
    public bool SupportsAggregation { get; }

    /// <summary>
    /// Gets known limitations of this translator.
    /// </summary>
    public IReadOnlyList<string> Limitations { get; }
}
