namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Implementation of field metadata with type and role information.
/// </summary>
public sealed class Field : IField
{
    /// <summary>
    /// Gets or initializes the field name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets or initializes the field type.
    /// </summary>
    public required IFieldType FieldType { get; init; }

    /// <summary>
    /// Gets or initializes the field role.
    /// </summary>
    public required FieldRole Role { get; init; }

    /// <summary>
    /// Gets or initializes whether the field is nullable.
    /// </summary>
    public bool IsNullable { get; init; }

    /// <summary>
    /// Gets or initializes the field description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or initializes the converter type name.
    /// </summary>
    public string? ConverterTypeName { get; init; }
}
