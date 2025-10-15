using System;

namespace FractalDataWorks.Data.DataSets.Abstractions;

/// <summary>
/// Represents metadata for a field within a dataset.
/// Provides schema information including type, constraints, and key designation.
/// </summary>
public interface IDataField
{
    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    /// <value>The field name used for identification and querying.</value>
    string Name { get; }

    /// <summary>
    /// Gets the .NET type of the field.
    /// </summary>
    /// <value>The System.Type that represents the data type of this field.</value>
    Type FieldType { get; }

    /// <summary>
    /// Gets a value indicating whether this field is part of the primary key.
    /// </summary>
    /// <value><c>true</c> if this field is a key field; otherwise, <c>false</c>.</value>
    bool IsKey { get; }

    /// <summary>
    /// Gets a value indicating whether this field is required (cannot be null).
    /// </summary>
    /// <value><c>true</c> if this field is required; otherwise, <c>false</c>.</value>
    bool IsRequired { get; }

    /// <summary>
    /// Gets the optional description of this field.
    /// </summary>
    /// <value>A description explaining the purpose and content of this field, or null if not specified.</value>
    string? Description { get; }

    /// <summary>
    /// Gets the maximum length for string fields.
    /// </summary>
    /// <value>The maximum character length for string fields, or null if not applicable or unlimited.</value>
    int? MaxLength { get; }

    /// <summary>
    /// Gets the default value for this field.
    /// </summary>
    /// <value>The default value to use when no value is provided, or null if no default is specified.</value>
    object? DefaultValue { get; }
}