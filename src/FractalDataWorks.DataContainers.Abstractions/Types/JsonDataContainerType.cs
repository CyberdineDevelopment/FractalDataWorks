using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions.Types;

/// <summary>
/// JSON (JavaScript Object Notation) data container type.
/// Supports hierarchical structured data with nested objects and arrays.
/// </summary>
[TypeOption("Json")]
public sealed class JsonDataContainerType : DataContainerTypeBase<JsonDataContainerType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDataContainerType"/> class.
    /// </summary>
    public JsonDataContainerType() : base(
        id: 2,
        name: "JSON",
        fileExtension: ".json",
        mimeType: "application/json",
        supportsRead: true,
        supportsWrite: true,
        supportsSchemaInference: true,
        supportsStreaming: false, // JSON requires complete parsing for validation
        compatibleConnectionTypes: new[] { "File", "Http", "S3", "REST" },
        category: "File")
    {
    }

    /// <inheritdoc/>
    public override IContainerConfiguration CreateDefaultConfiguration()
    {
        return new JsonContainerConfiguration
        {
            Indented = false,
            CamelCaseProperties = false,
            IgnoreNullValues = false,
            StrictTypeValidation = true
        };
    }

    /// <inheritdoc/>
    public override IDataContainer CreateContainer(DataLocation location, IContainerConfiguration configuration)
    {
        if (!(configuration is JsonContainerConfiguration jsonConfig))
            throw new ArgumentException("Configuration must be JsonContainerConfiguration for JSON containers");

        return new JsonDataContainer(location, jsonConfig);
    }

    /// <inheritdoc/>
    public override IFdwResult ValidateConfiguration(IContainerConfiguration configuration)
    {
        if (!(configuration is JsonContainerConfiguration))
            return FdwResult.Failure("Configuration must be JsonContainerConfiguration for JSON containers");

        return FdwResult.Success();
    }

    /// <inheritdoc/>
    protected override IEnumerable<string> GetTypeLimitations()
    {
        return new[]
        {
            "Entire file must be loaded into memory for processing",
            "No streaming support for large datasets",
            "Schema inference can be expensive for large or complex structures",
            "No built-in compression support"
        };
    }

    /// <inheritdoc/>
    public override Task<IFdwResult<IDataSchema>> DiscoverSchemaAsync(DataLocation location, int sampleSize = 1000) =>
        Task.FromResult(FdwResult<IDataSchema>.Failure("Not implemented"));

    /// <inheritdoc/>
    public override IFdwResult<ContainerMetadata> GetMetadata(DataLocation location) =>
        FdwResult<ContainerMetadata>.Failure("Not implemented");
}

