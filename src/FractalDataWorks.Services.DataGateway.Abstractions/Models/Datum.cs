using System;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Models;

/// <summary>
/// Represents a single data value with its metadata.
/// </summary>
public sealed class Datum
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Datum"/> class.
    /// </summary>
    /// <param name="name">The name of the data field.</param>
    /// <param name="category">The semantic category of the data.</param>
    /// <param name="valueType">The .NET type of the value.</param>
    /// <param name="value">The actual value (can be null).</param>
    public Datum(string name, DatumCategory category, Type valueType, object? value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Category = category;
        ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
        Value = value;
    }

    /// <summary>
    /// Gets the name of the data field.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the semantic category of the data.
    /// </summary>
    public DatumCategory Category { get; }

    /// <summary>
    /// Gets the .NET type of the value.
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// Gets the actual value (can be null).
    /// </summary>
    public object? Value { get; }
}
