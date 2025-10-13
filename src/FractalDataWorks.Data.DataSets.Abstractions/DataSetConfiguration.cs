using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Configuration;
using FluentValidation;

namespace FractalDataWorks.Data.DataSets.Abstractions;

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
    public IList<DataFieldConfiguration> Fields { get; set; } = new List<DataFieldConfiguration>();

    /// <summary>
    /// Gets or sets the names of fields that form the primary key for this dataset.
    /// </summary>
    /// <value>A list of field names that uniquely identify records in this dataset.</value>
    public IList<string> KeyFields { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets source mappings for different connection types.
    /// </summary>
    /// <value>A dictionary mapping connection type names to their specific source configurations.</value>
    public IDictionary<string, SourceMappingConfiguration> Sources { get; set; } = new Dictionary<string, SourceMappingConfiguration>(StringComparer.OrdinalIgnoreCase);

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

