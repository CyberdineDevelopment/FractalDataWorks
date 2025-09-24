using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.DataGateway.Abstractions;

/// <summary>
/// Metadata information about a field in a dataset.
/// </summary>
/// <remarks>
/// Describes a single field/column in a dataset, including its type,
/// nullability, and other characteristics discovered from the data source.
/// </remarks>
public sealed record DataFieldMetadata
{
    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the data type of the field.
    /// </summary>
    public required Type DataType { get; init; }

    /// <summary>
    /// Gets a value indicating whether the field can contain null values.
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// Gets a value indicating whether this field is part of the primary key.
    /// </summary>
    public bool IsKey { get; init; }

    /// <summary>
    /// Gets the maximum length for string fields, if applicable.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Gets additional field-specific properties.
    /// </summary>
    public Dictionary<string, object> Properties { get; init; } = new(StringComparer.Ordinal);
}