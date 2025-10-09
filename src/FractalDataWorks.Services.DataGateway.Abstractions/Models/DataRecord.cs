using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.Services.DataGateway.Abstractions.Models;

/// <summary>
/// Represents a single record (row) of data with multiple fields.
/// </summary>
public sealed class DataRecord
{
    private readonly List<Datum> _data;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataRecord"/> class.
    /// </summary>
    /// <param name="data">The collection of data fields.</param>
    public DataRecord(IEnumerable<Datum> data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        _data = data.ToList();
    }

    /// <summary>
    /// Gets the collection of data fields in this record.
    /// </summary>
    public IReadOnlyList<Datum> Data => _data;

    /// <summary>
    /// Gets the number of fields in this record.
    /// </summary>
    public int FieldCount => _data.Count;

    /// <summary>
    /// Gets a datum by field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>The datum if found; otherwise, null.</returns>
    public Datum? GetDatum(string fieldName)
    {
        return _data.FirstOrDefault(d => string.Equals(d.Name, fieldName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a value by field name.
    /// </summary>
    /// <param name="fieldName">The name of the field.</param>
    /// <returns>The value if found; otherwise, null.</returns>
    public object? GetValue(string fieldName)
    {
        return GetDatum(fieldName)?.Value;
    }
}
