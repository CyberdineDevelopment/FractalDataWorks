using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Configuration;
using FluentValidation;

namespace FractalDataWorks.Data.DataSets.Abstractions;

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