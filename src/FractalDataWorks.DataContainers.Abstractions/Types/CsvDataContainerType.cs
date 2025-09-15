using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions.Types;

/// <summary>
/// CSV (Comma-Separated Values) data container type.
/// Supports structured data in delimited text format with configurable separators.
/// </summary>
public sealed class CsvDataContainerType : DataContainerTypeBase<CsvDataContainerType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvDataContainerType"/> class.
    /// </summary>
    public CsvDataContainerType() : base(1, "CSV", "File") { }

    /// <inheritdoc/>
    public override string? FileExtension => ".csv";

    /// <inheritdoc/>
    public override string? MimeType => "text/csv";

    /// <inheritdoc/>
    public override bool SupportsRead => true;

    /// <inheritdoc/>
    public override bool SupportsWrite => true;

    /// <inheritdoc/>
    public override bool SupportsSchemaInference => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => true;

    /// <inheritdoc/>
    public override IEnumerable<string> CompatibleConnectionTypes => new[] { "File", "Http", "S3" };

    /// <inheritdoc/>
    public override IContainerConfiguration CreateDefaultConfiguration()
    {
        return new CsvContainerConfiguration
        {
            Delimiter = ',',
            HasHeaderRow = true,
            Encoding = "UTF-8",
            QuoteCharacter = '"',
            EscapeCharacter = '"'
        };
    }

    /// <inheritdoc/>
    public override IDataContainer CreateContainer(DataLocation location, IContainerConfiguration configuration)
    {
        if (!(configuration is CsvContainerConfiguration csvConfig))
            throw new ArgumentException("Configuration must be CsvContainerConfiguration for CSV containers");

        return new CsvDataContainer(location, csvConfig);
    }

    /// <inheritdoc/>
    public override IFdwResult ValidateConfiguration(IContainerConfiguration configuration)
    {
        if (!(configuration is CsvContainerConfiguration csvConfig))
            return FdwResult.Failure("Configuration must be CsvContainerConfiguration for CSV containers");

        if (csvConfig.Delimiter == csvConfig.QuoteCharacter)
            return FdwResult.Failure("Delimiter and quote character cannot be the same");

        if (string.IsNullOrEmpty(csvConfig.Encoding))
            return FdwResult.Failure("Encoding cannot be null or empty");

        return FdwResult.Success();
    }

    /// <inheritdoc/>
    protected override IEnumerable<string> GetTypeLimitations()
    {
        return new[]
        {
            "Does not support nested objects or arrays",
            "Limited data type inference (strings, numbers, booleans)",
            "Performance degrades with files larger than 1GB",
            "Requires consistent column structure across all rows"
        };
    }
}

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
}

/// <summary>
/// CSV data container implementation.
/// </summary>
internal sealed class CsvDataContainer : IDataContainer
{
    public CsvDataContainer(DataLocation location, CsvContainerConfiguration configuration)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public DataLocation Location { get; }
    public IContainerConfiguration Configuration { get; }
    public string ContainerType => "CSV";
}

/// <summary>
/// Marker interface for container configurations.
/// </summary>
public interface IContainerConfiguration
{
    // Marker interface - specific implementations will add their properties
}

/// <summary>
/// Marker interface for data containers.
/// </summary>
public interface IDataContainer
{
    /// <summary>
    /// Gets the data location for this container.
    /// </summary>
    DataLocation Location { get; }

    /// <summary>
    /// Gets the configuration for this container.
    /// </summary>
    IContainerConfiguration Configuration { get; }

    /// <summary>
    /// Gets the container type identifier.
    /// </summary>
    string ContainerType { get; }
}