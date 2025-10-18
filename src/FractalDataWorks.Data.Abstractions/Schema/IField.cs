namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Represents a single field/column with type and role metadata.
/// </summary>
public interface IField
{
    /// <summary>
    /// Field name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Field type (can be simple, array, or object with recursive nesting).
    /// </summary>
    IFieldType FieldType { get; }

    /// <summary>
    /// Semantic role (Identity/Attribute/Measure) - EXPLICITLY specified in configuration.
    /// </summary>
    FieldRole Role { get; }

    /// <summary>
    /// Whether this field can be null.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    /// Optional description/documentation.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Converter type name for this field (resolved at runtime).
    /// Example: "SqlInt32", "JsonString", "SqlDateTime"
    /// </summary>
    string? ConverterTypeName { get; }
}
