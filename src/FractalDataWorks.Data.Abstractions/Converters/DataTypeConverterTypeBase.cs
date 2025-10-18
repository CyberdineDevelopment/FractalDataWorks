using System;
using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for data type converter type definitions.
/// Provides metadata about type converters that handle data store ↔ CLR type mapping.
/// </summary>
public abstract class DataTypeConverterTypeBase : TypeOptionBase<DataTypeConverterTypeBase>, IDataTypeConverterType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeConverterTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this converter type.</param>
    /// <param name="name">The name of this converter type.</param>
    /// <param name="displayName">The display name for this converter type.</param>
    /// <param name="description">The description of this converter type.</param>
    /// <param name="sourceTypeName">The source type name (e.g., "int" for SQL).</param>
    /// <param name="targetClrType">The target CLR type.</param>
    /// <param name="category">The category for this converter type (defaults to "Converter").</param>
    protected DataTypeConverterTypeBase(
        int id,
        string name,
        string displayName,
        string description,
        string sourceTypeName,
        Type targetClrType,
        string? category = null)
        : base(id, name, $"Converters:{name}", displayName, description, category ?? "Converter")
    {
        SourceTypeName = sourceTypeName;
        TargetClrType = targetClrType;
    }

    /// <inheritdoc/>
    public string SourceTypeName { get; }

    /// <inheritdoc/>
    public Type TargetClrType { get; }
}
