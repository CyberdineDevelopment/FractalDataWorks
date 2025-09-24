using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions.Types;

/// <summary>
/// Configuration settings specific to CSV data containers.
/// </summary>
public sealed class CsvContainerConfiguration : IContainerConfiguration
{
    /// <summary>
    /// Gets or sets the field delimiter character.
    /// </summary>
    public char Delimiter { get; set; } = ',';

    /// <summary>
    /// Gets or sets a value indicating whether the first row contains column headers.
    /// </summary>
    public bool HasHeaderRow { get; set; } = true;

    /// <summary>
    /// Gets or sets the text encoding for the CSV file.
    /// </summary>
    public string Encoding { get; set; } = "UTF-8";

    /// <summary>
    /// Gets or sets the character used to quote fields containing delimiters.
    /// </summary>
    public char QuoteCharacter { get; set; } = '"';

    /// <summary>
    /// Gets or sets the character used to escape quotes within quoted fields.
    /// </summary>
    public char EscapeCharacter { get; set; } = '"';

    /// <summary>
    /// Gets or sets a value indicating whether to trim whitespace from field values.
    /// </summary>
    public bool TrimWhitespace { get; set; } = true;

    /// <inheritdoc/>
    public string ContainerType => "CSV";

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object> Settings =>
        new Dictionary<string, object>(StringComparer.Ordinal)
        {
            { nameof(Delimiter), Delimiter },
            { nameof(HasHeaderRow), HasHeaderRow },
            { nameof(Encoding), Encoding },
            { nameof(QuoteCharacter), QuoteCharacter },
            { nameof(EscapeCharacter), EscapeCharacter },
            { nameof(TrimWhitespace), TrimWhitespace }
        };

    /// <inheritdoc/>
    public IFdwResult Validate()
    {
        if (Delimiter == QuoteCharacter)
            return FdwResult.Failure("Delimiter and quote character cannot be the same");

        if (string.IsNullOrEmpty(Encoding))
            return FdwResult.Failure("Encoding cannot be null or empty");

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