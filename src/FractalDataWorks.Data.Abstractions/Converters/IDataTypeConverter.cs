using System;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Interface for data type converters that handle type mapping between data stores and CLR types.
/// Examples: SqlInt32Converter (SQL int → CLR int), JsonStringConverter (JSON string → CLR string)
/// </summary>
public interface IDataTypeConverter
{
    /// <summary>
    /// Source type name (e.g., "int" for SQL, "integer" for JSON).
    /// </summary>
    string SourceTypeName { get; }

    /// <summary>
    /// Target CLR type.
    /// </summary>
    Type TargetClrType { get; }

    /// <summary>
    /// Convert from source data type to CLR type.
    /// </summary>
    /// <param name="value">Source value (from data store).</param>
    /// <returns>CLR value.</returns>
    object? Convert(object? value);

    /// <summary>
    /// Convert from CLR type back to source data type.
    /// </summary>
    /// <param name="clrValue">CLR value.</param>
    /// <returns>Source value (for data store).</returns>
    object? ConvertBack(object? clrValue);
}
