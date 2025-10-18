using System;
using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for data type converter implementations.
/// </summary>
public abstract class DataTypeConverterBase : IDataTypeConverter, ITypeOption<DataTypeConverterBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeConverterBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this converter type.</param>
    /// <param name="name">The name of this converter.</param>
    protected DataTypeConverterBase(int id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets the unique identifier for this converter type.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Gets the name of this converter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category for this converter type (default: "Converter").
    /// </summary>
    public virtual string Category => "Converter";

    /// <summary>
    /// Gets the source type name.
    /// </summary>
    public abstract string SourceTypeName { get; }

    /// <summary>
    /// Gets the target CLR type.
    /// </summary>
    public abstract Type TargetClrType { get; }

    /// <summary>
    /// Converts from source data type to CLR type.
    /// </summary>
    public abstract object? Convert(object? value);

    /// <summary>
    /// Converts from CLR type back to source data type.
    /// </summary>
    public abstract object? ConvertBack(object? clrValue);
}
