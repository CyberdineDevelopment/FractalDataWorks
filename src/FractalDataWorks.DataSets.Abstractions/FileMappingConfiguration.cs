using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Configuration;
using FluentValidation;

namespace FractalDataWorks.DataSets.Abstractions;

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