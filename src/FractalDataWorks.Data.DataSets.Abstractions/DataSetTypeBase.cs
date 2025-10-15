using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.DataSets.Abstractions;

/// <summary>
/// Abstract base class for dataset type definitions following the FractalDataWorks.Collections pattern.
/// Provides common functionality and enforces the structure required for source generation.
/// </summary>
public abstract class DataSetTypeBase : TypeOptionBase<DataSetTypeBase>, IDataSetType
{
    private readonly IReadOnlyCollection<IDataField> _fields;
    private readonly System.Collections.ObjectModel.ReadOnlyCollection<string> _keyFields;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSetTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this dataset.</param>
    /// <param name="name">The name of this dataset.</param>
    /// <param name="description">The description of this dataset.</param>
    /// <param name="recordType">The .NET type of records in this dataset.</param>
    /// <param name="fields">The field definitions for this dataset schema.</param>
    /// <param name="category">The optional category for this dataset. Defaults to "Dataset" if not specified.</param>
    /// <exception cref="ArgumentNullException">Thrown when name, description, recordType, or fields are null.</exception>
    /// <exception cref="ArgumentException">Thrown when fields collection is empty or contains duplicate names.</exception>
    protected DataSetTypeBase(
        int id,
        string name,
        string description,
        Type recordType,
        IReadOnlyCollection<IDataField> fields,
        string? category = null)
        : base(id, name, $"DataSets:{name}", $"{name} Data Set", description, category ?? "Dataset")
    {
        Description = description;
        RecordType = recordType;
        
        if (fields == null) throw new ArgumentNullException(nameof(fields));
        if (fields.Count == 0) throw new ArgumentException("Dataset must have at least one field.", nameof(fields));
        
        // Validate no duplicate field names
        var fieldNames = fields.Select(f => f.Name).ToList();
        var duplicateNames = fieldNames.GroupBy(n => n, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicateNames.Count > 0)
        {
            throw new ArgumentException($"Duplicate field names found: {string.Join(", ", duplicateNames)}", nameof(fields));
        }

        _fields = fields;
        _keyFields = fields.Where(f => f.IsKey).Select(f => f.Name).ToList().AsReadOnly();
        
        // Validate at least one key field exists
        if (_keyFields.Count == 0)
        {
            throw new ArgumentException("Dataset must have at least one key field.", nameof(fields));
        }
    }

    /// <inheritdoc/>
    public new string Description { get; }

    /// <inheritdoc/>
    public Type RecordType { get; }

    /// <inheritdoc/>
    public IReadOnlyCollection<IDataField> Fields => _fields;

    /// <inheritdoc/>
    public IReadOnlyCollection<string> KeyFields => _keyFields;

    /// <inheritdoc/>
    public virtual string ConfigurationSection => $"DataSets:{Name}";

    /// <inheritdoc/>
    public virtual string Version => "1.0";

    /// <summary>
    /// Gets a field by name.
    /// </summary>
    /// <param name="fieldName">The name of the field to retrieve.</param>
    /// <returns>The field with the specified name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fieldName is null.</exception>
    /// <exception cref="ArgumentException">Thrown when no field with the specified name exists.</exception>
    public IDataField GetField(string fieldName)
    {
        var field = _fields.FirstOrDefault(f => string.Equals(f.Name, fieldName, StringComparison.OrdinalIgnoreCase));
        if (field == null)
        {
            throw new ArgumentException($"Field '{fieldName}' not found in dataset '{Name}'.", nameof(fieldName));
        }
        
        return field;
    }

    /// <summary>
    /// Determines whether this dataset has a field with the specified name.
    /// </summary>
    /// <param name="fieldName">The name of the field to check for.</param>
    /// <returns><c>true</c> if a field with the specified name exists; otherwise, <c>false</c>.</returns>
    public bool HasField(string fieldName)
    {
        if (fieldName == null) return false;
        return _fields.Any(f => string.Equals(f.Name, fieldName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets all fields of the specified type.
    /// </summary>
    /// <param name="fieldType">The .NET type to filter fields by.</param>
    /// <returns>A collection of fields that match the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when fieldType is null.</exception>
    public IEnumerable<IDataField> GetFieldsByType(Type fieldType)
    {
        if (fieldType == null) throw new ArgumentNullException(nameof(fieldType));
        return _fields.Where(f => f.FieldType == fieldType);
    }

    /// <summary>
    /// Gets all key fields for this dataset.
    /// </summary>
    /// <returns>A collection of fields that are marked as key fields.</returns>
    public IEnumerable<IDataField> GetKeyFieldDefinitions()
    {
        return _fields.Where(f => f.IsKey);
    }

    /// <summary>
    /// Creates a LINQ query builder for this dataset.
    /// </summary>
    /// <typeparam name="TRecord">The record type to query.</typeparam>
    /// <returns>A query builder that supports LINQ operations.</returns>
    /// <remarks>
    /// This method enables LINQ syntax on datasets:
    /// <code>
    /// var query = myDataSet.CreateQuery&lt;MyRecord&gt;()
    ///     .Where(r => r.Status == "Active")
    ///     .Select(r => new { r.Id, r.Name });
    /// </code>
    /// The returned query builder captures LINQ expressions for translation by connection providers.
    /// </remarks>
    public DataQueryBuilder<TRecord> CreateQuery<TRecord>()
    {
        return new DataQueryBuilder<TRecord>(Name);
    }

    /// <summary>
    /// Creates a LINQ query builder using the dataset's record type.
    /// </summary>
    /// <returns>A query builder that supports LINQ operations.</returns>
    /// <remarks>
    /// This is a convenience method that uses the dataset's RecordType.
    /// Equivalent to calling CreateQuery&lt;RecordType&gt;() but with type inference.
    /// </remarks>
    public IDataQuery CreateQuery()
    {
        var queryBuilderType = typeof(DataQueryBuilder<>).MakeGenericType(RecordType);
        return (IDataQuery)Activator.CreateInstance(queryBuilderType, this)!;
    }

    /// <summary>
    /// Returns a string representation of this dataset.
    /// </summary>
    /// <returns>A string containing the name and description of this dataset.</returns>
    public override string ToString()
    {
        return $"DataSet: {Name} - {Description} ({_fields.Count} fields)";
    }
}