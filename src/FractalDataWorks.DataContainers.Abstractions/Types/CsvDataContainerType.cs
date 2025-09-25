using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions.Types;

/// <summary>
/// CSV (Comma-Separated Values) data container type.
/// Supports structured data in delimited text format with configurable separators.
/// </summary>
[TypeOption(typeof(DataContainerTypes),"Csv")]
public sealed class CsvDataContainerType : DataContainerTypeBase<CsvDataContainerType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvDataContainerType"/> class.
    /// </summary>
    public CsvDataContainerType() : base(
        id: 1,
        name: "CSV",
        fileExtension: ".csv",
        mimeType: "text/csv",
        supportsRead: true,
        supportsWrite: true,
        supportsSchemaInference: true,
        supportsStreaming: true,
        compatibleConnectionTypes: new[] { "File", "Http", "S3" },
        category: "File")
    {
    }

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

    /// <inheritdoc/>
    public override Task<IFdwResult<IDataSchema>> DiscoverSchemaAsync(DataLocation location, int sampleSize = 1000) =>
        Task.FromResult(FdwResult<IDataSchema>.Failure("Not implemented"));

    /// <inheritdoc/>
    public override IFdwResult<ContainerMetadata> GetMetadata(DataLocation location) =>
        FdwResult<ContainerMetadata>.Failure("Not implemented");
}

