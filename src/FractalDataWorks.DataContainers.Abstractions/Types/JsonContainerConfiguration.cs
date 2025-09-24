using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions.Types;

/// <summary>
/// Configuration settings specific to JSON data containers.
/// </summary>
public sealed class JsonContainerConfiguration : IContainerConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether JSON output should be indented.
    /// </summary>
    public bool Indented { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether property names should use camelCase.
    /// </summary>
    public bool CamelCaseProperties { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether null values should be ignored in output.
    /// </summary>
    public bool IgnoreNullValues { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether strict type validation should be enforced.
    /// </summary>
    public bool StrictTypeValidation { get; set; } = true;

    /// <inheritdoc/>
    public string ContainerType => "JSON";

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Settings =>
        new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { nameof(Indented), Indented },
            { nameof(CamelCaseProperties), CamelCaseProperties },
            { nameof(IgnoreNullValues), IgnoreNullValues },
            { nameof(StrictTypeValidation), StrictTypeValidation }
        };

    /// <inheritdoc/>
    public IFdwResult Validate()
    {
        return FdwResult.Success();
    }

    /// <inheritdoc/>
    public T GetValue<T>(string key, T defaultValue = default!)
    {
        if (Settings.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;
        return defaultValue;
    }
}