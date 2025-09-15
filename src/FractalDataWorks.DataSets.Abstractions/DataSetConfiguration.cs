using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Configuration;
using FluentValidation;

namespace FractalDataWorks.DataSets.Abstractions;

/// <summary>
/// Configuration class for runtime dataset definitions.
/// Enables dynamic creation of datasets from configuration files or other sources.
/// </summary>
public sealed class DataSetConfiguration : ConfigurationBase<DataSetConfiguration>
{
    /// <summary>
    /// Gets or sets the name of the dataset.
    /// </summary>
    /// <value>The unique name identifier for this dataset.</value>
    public string DataSetName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the dataset.
    /// </summary>
    /// <value>A detailed description explaining the purpose and content of this dataset.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of this dataset schema.
    /// </summary>
    /// <value>The schema version string for compatibility and migration purposes.</value>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets the category for grouping related datasets.
    /// </summary>
    /// <value>The category name for organizational purposes.</value>
    public string Category { get; set; } = "Dataset";

    /// <summary>
    /// Gets or sets the .NET type name of the record/entity that this dataset represents.
    /// </summary>
    /// <value>The fully qualified type name of the data record or entity class.</value>
    public string RecordTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field definitions that define the schema of this dataset.
    /// </summary>
    /// <value>A collection of field configuration objects describing the structure of records.</value>
    public List<DataFieldConfiguration> Fields { get; set; } = new List<DataFieldConfiguration>();

    /// <summary>
    /// Gets or sets the names of fields that form the primary key for this dataset.
    /// </summary>
    /// <value>A list of field names that uniquely identify records in this dataset.</value>
    public List<string> KeyFields { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets source mappings for different connection types.
    /// </summary>
    /// <value>A dictionary mapping connection type names to their specific source configurations.</value>
    public Dictionary<string, SourceMappingConfiguration> Sources { get; set; } = new Dictionary<string, SourceMappingConfiguration>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets caching configuration for this dataset.
    /// </summary>
    /// <value>Configuration settings for data caching behavior.</value>
    public CachingConfiguration? Caching { get; set; }

    /// <inheritdoc/>
    public override string SectionName => $"DataSets:{DataSetName}";

    /// <inheritdoc/>
    protected override IValidator<DataSetConfiguration>? GetValidator()
    {
        return new DataSetConfigurationValidator();
    }

    /// <inheritdoc/>
    protected override void CopyTo(DataSetConfiguration target)
    {
        base.CopyTo(target);
        target.DataSetName = DataSetName;
        target.Description = Description;
        target.Version = Version;
        target.Category = Category;
        target.RecordTypeName = RecordTypeName;
        target.Fields = new List<DataFieldConfiguration>(Fields);
        target.KeyFields = new List<string>(KeyFields);
        target.Sources = new Dictionary<string, SourceMappingConfiguration>(Sources, StringComparer.OrdinalIgnoreCase);
        target.Caching = Caching;
    }
}


/// <summary>
/// Configuration for source mappings that define how to access data from different connection types.
/// </summary>
public sealed class SourceMappingConfiguration
{
    /// <summary>
    /// Gets or sets the connection type name (e.g., "SQL", "HTTP", "File").
    /// </summary>
    /// <value>The type identifier for the connection that can provide this data.</value>
    public string ConnectionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority of this source when multiple sources are available.
    /// </summary>
    /// <value>Lower values indicate higher priority (1 = highest priority).</value>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Gets or sets the estimated cost/performance rating for this source.
    /// </summary>
    /// <value>Estimated cost where lower values indicate better performance/lower cost.</value>
    public int EstimatedCost { get; set; } = 50;

    /// <summary>
    /// Gets or sets SQL-specific mapping configuration.
    /// </summary>
    /// <value>Configuration for SQL table/view access.</value>
    public SqlMappingConfiguration? Sql { get; set; }

    /// <summary>
    /// Gets or sets HTTP-specific mapping configuration.
    /// </summary>
    /// <value>Configuration for HTTP API access.</value>
    public HttpMappingConfiguration? Http { get; set; }

    /// <summary>
    /// Gets or sets file-specific mapping configuration.
    /// </summary>
    /// <value>Configuration for file-based data access.</value>
    public FileMappingConfiguration? File { get; set; }
}

/// <summary>
/// SQL-specific mapping configuration.
/// </summary>
public sealed class SqlMappingConfiguration
{
    /// <summary>
    /// Gets or sets the database schema name.
    /// </summary>
    /// <value>The schema containing the table or view.</value>
    public string? SchemaName { get; set; }

    /// <summary>
    /// Gets or sets the table or view name.
    /// </summary>
    /// <value>The name of the table or view containing the data.</value>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets custom field mappings from dataset fields to database columns.
    /// </summary>
    /// <value>A dictionary mapping dataset field names to database column names.</value>
    public Dictionary<string, string> FieldMappings { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// HTTP-specific mapping configuration.
/// </summary>
public sealed class HttpMappingConfiguration
{
    /// <summary>
    /// Gets or sets the API endpoint path.
    /// </summary>
    /// <value>The endpoint path for accessing this dataset via HTTP.</value>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP method to use.
    /// </summary>
    /// <value>The HTTP method (GET, POST, etc.) for accessing the data.</value>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// Gets or sets additional query parameters.
    /// </summary>
    /// <value>Static query parameters to include in HTTP requests.</value>
    public Dictionary<string, string> QueryParameters { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets custom field mappings from dataset fields to JSON properties.
    /// </summary>
    /// <value>A dictionary mapping dataset field names to JSON property names.</value>
    public Dictionary<string, string> FieldMappings { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// File-specific mapping configuration.
/// </summary>
public sealed class FileMappingConfiguration
{
    /// <summary>
    /// Gets or sets the file path pattern.
    /// </summary>
    /// <value>The path pattern for locating data files (may include placeholders).</value>
    public string PathPattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file format.
    /// </summary>
    /// <value>The format of the data file (CSV, JSON, Parquet, etc.).</value>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets custom field mappings from dataset fields to file columns/properties.
    /// </summary>
    /// <value>A dictionary mapping dataset field names to file column names or property names.</value>
    public Dictionary<string, string> FieldMappings { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Caching configuration for dataset operations.
/// </summary>
public sealed class CachingConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether caching is enabled.
    /// </summary>
    /// <value><c>true</c> if caching is enabled; otherwise, <c>false</c>.</value>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache duration in minutes.
    /// </summary>
    /// <value>The number of minutes to cache data before expiration.</value>
    public int DurationMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the cache key pattern.
    /// </summary>
    /// <value>Pattern for generating cache keys (may include placeholders).</value>
    public string KeyPattern { get; set; } = "dataset:{datasetName}:{queryHash}";
}

/// <summary>
/// Validator for DataSetConfiguration instances.
/// Ensures all required fields are provided and valid.
/// </summary>
public sealed class DataSetConfigurationValidator : AbstractValidator<DataSetConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataSetConfigurationValidator"/> class.
    /// </summary>
    public DataSetConfigurationValidator()
    {
        RuleFor(x => x.DataSetName)
            .NotEmpty()
            .WithMessage("Dataset name is required")
            .MaximumLength(100)
            .WithMessage("Dataset name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(500)
            .WithMessage("Description must not exceed 500 characters");

        RuleFor(x => x.RecordTypeName)
            .NotEmpty()
            .WithMessage("Record type name is required")
            .Must(BeValidTypeName)
            .WithMessage("Record type name must be a valid .NET type name");

        RuleFor(x => x.Fields)
            .NotEmpty()
            .WithMessage("At least one field must be defined");

        RuleForEach(x => x.Fields)
            .SetValidator(new DataFieldConfigurationValidator());

        RuleFor(x => x.KeyFields)
            .NotEmpty()
            .WithMessage("At least one key field must be specified");

        RuleFor(x => x)
            .Must(HaveValidKeyFields)
            .WithMessage("All key fields must exist in the fields collection");
    }

    private static bool BeValidTypeName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName)) return false;
        
        // Basic validation - could be enhanced with more sophisticated type name parsing
        return !typeName.Contains(" ") && typeName.Contains(".");
    }

    private static bool HaveValidKeyFields(DataSetConfiguration config)
    {
        if (config.KeyFields.Count == 0) return false;
        
        var fieldNames = new HashSet<string>(config.Fields.Select(f => f.Name), StringComparer.OrdinalIgnoreCase);
        return config.KeyFields.All(keyField => fieldNames.Contains(keyField));
    }
}

/// <summary>
/// Validator for DataFieldConfiguration instances.
/// </summary>
public sealed class DataFieldConfigurationValidator : AbstractValidator<DataFieldConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataFieldConfigurationValidator"/> class.
    /// </summary>
    public DataFieldConfigurationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Field name is required")
            .MaximumLength(50)
            .WithMessage("Field name must not exceed 50 characters");

        RuleFor(x => x.TypeName)
            .NotEmpty()
            .WithMessage("Field type name is required")
            .Must(BeValidTypeName)
            .WithMessage("Field type name must be a valid .NET type name");

        RuleFor(x => x.MaxLength)
            .GreaterThan(0)
            .When(x => x.MaxLength.HasValue)
            .WithMessage("Max length must be greater than zero when specified");
    }

    private static bool BeValidTypeName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName)) return false;
        
        // List of common .NET types that are valid
        var commonTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "System.String", "System.Int32", "System.Int64", "System.DateTime", "System.Decimal",
            "System.Double", "System.Boolean", "System.Guid", "System.Byte[]", "string", "int",
            "long", "DateTime", "decimal", "double", "bool", "Guid", "byte[]"
        };

        return commonTypes.Contains(typeName) || (!typeName.Contains(" ") && typeName.Contains("."));
    }
}