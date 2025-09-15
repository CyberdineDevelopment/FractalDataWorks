using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.DataStores.Abstractions;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions.Types;

/// <summary>
/// JSON (JavaScript Object Notation) data container type.
/// Supports hierarchical structured data with nested objects and arrays.
/// </summary>
public sealed class JsonDataContainerType : DataContainerTypeBase<JsonDataContainerType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDataContainerType"/> class.
    /// </summary>
    public JsonDataContainerType() : base(2, "JSON", "File") { }

    /// <inheritdoc/>
    public override string? FileExtension => ".json";

    /// <inheritdoc/>
    public override string? MimeType => "application/json";

    /// <inheritdoc/>
    public override bool SupportsRead => true;

    /// <inheritdoc/>
    public override bool SupportsWrite => true;

    /// <inheritdoc/>
    public override bool SupportsSchemaInference => true;

    /// <inheritdoc/>
    public override bool SupportsStreaming => false; // JSON requires complete parsing for validation

    /// <inheritdoc/>
    public override IEnumerable<string> CompatibleConnectionTypes => new[] { "File", "Http", "S3", "REST" };

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
}

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
}

/// <summary>
/// JSON data container implementation.
/// </summary>
internal sealed class JsonDataContainer : IDataContainer
{
    public JsonDataContainer(DataLocation location, JsonContainerConfiguration configuration)
    {
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public DataLocation Location { get; }
    public IContainerConfiguration Configuration { get; }
    public string ContainerType => "JSON";
}