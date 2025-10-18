using System;
using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Represents a data type converter type definition - metadata about type converters.
/// </summary>
/// <remarks>
/// Converter types handle type mapping between data stores and CLR types (SQL → CLR, JSON → CLR, etc.).
/// </remarks>
public interface IDataTypeConverterType : ITypeOption
{
    /// <summary>
    /// Gets the configuration key for this converter type value.
    /// </summary>
    string ConfigurationKey { get; }

    /// <summary>
    /// Gets the display name for this converter type value.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the description of this converter type value.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the source type name (e.g., "int" for SQL, "integer" for JSON).
    /// </summary>
    string SourceTypeName { get; }

    /// <summary>
    /// Gets the target CLR type.
    /// </summary>
    Type TargetClrType { get; }
}
