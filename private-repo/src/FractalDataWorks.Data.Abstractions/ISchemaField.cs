using System.Collections.Generic;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Represents a field in a data schema.
/// </summary>
public interface ISchemaField
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the field data type.
    /// </summary>
    string DataType { get; }

    /// <summary>
    /// Gets whether the field allows null values.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    /// Gets whether the field is part of the primary key.
    /// </summary>
    bool IsPrimaryKey { get; }

    /// <summary>
    /// Gets the maximum length for string fields.
    /// </summary>
    int? MaxLength { get; }

    /// <summary>
    /// Gets the field metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}
