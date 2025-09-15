using System;
using System.Collections.Generic;
using FractalDataWorks.Results;

namespace FractalDataWorks.DataContainers.Abstractions;

/// <summary>
/// Represents the schema definition for a data container, describing the structure,
/// field types, constraints, and relationships within the data.
/// </summary>
/// <remarks>
/// IDataSchema provides a unified way to describe data structure across different
/// container formats. It enables schema validation, type conversion, and field
/// mapping regardless of whether the data is in CSV, JSON, SQL, or other formats.
/// </remarks>
public interface IDataSchema
{
    /// <summary>
    /// Gets the unique identifier for this schema.
    /// </summary>
    /// <value>A unique identifier for this schema definition.</value>
    string Id { get; }

    /// <summary>
    /// Gets the display name for this schema.
    /// </summary>
    /// <value>A human-readable name for UI and logging purposes.</value>
    string Name { get; }

    /// <summary>
    /// Gets the schema version.
    /// </summary>
    /// <value>The version identifier for schema evolution and compatibility tracking.</value>
    string Version { get; }

    /// <summary>
    /// Gets the field definitions in this schema.
    /// </summary>
    /// <value>An ordered collection of field definitions.</value>
    IReadOnlyList<ISchemaField> Fields { get; }

    /// <summary>
    /// Gets the primary key field names for this schema.
    /// </summary>
    /// <value>
    /// Field names that constitute the primary key, or empty if no primary key is defined.
    /// </value>
    IReadOnlyList<string> PrimaryKeyFields { get; }

    /// <summary>
    /// Gets metadata about this schema.
    /// </summary>
    /// <value>Additional properties and configuration information.</value>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Gets a field definition by name.
    /// </summary>
    /// <param name="fieldName">The name of the field to retrieve.</param>
    /// <returns>The field definition, or null if not found.</returns>
    ISchemaField? GetField(string fieldName);

    /// <summary>
    /// Gets field definitions by their names.
    /// </summary>
    /// <param name="fieldNames">The names of the fields to retrieve.</param>
    /// <returns>Field definitions for the specified names, excluding any not found.</returns>
    IEnumerable<ISchemaField> GetFields(IEnumerable<string> fieldNames);

    /// <summary>
    /// Validates that a record conforms to this schema.
    /// </summary>
    /// <param name="record">The record to validate as field name/value pairs.</param>
    /// <returns>A result indicating whether the record is valid, with specific error details.</returns>
    /// <remarks>
    /// This method performs comprehensive validation including type checking,
    /// constraint validation, required field checking, and custom validation rules.
    /// </remarks>
    IFdwResult ValidateRecord(IReadOnlyDictionary<string, object> record);

    /// <summary>
    /// Validates that a strongly-typed object conforms to this schema.
    /// </summary>
    /// <typeparam name="T">The type of the object to validate.</typeparam>
    /// <param name="record">The object to validate.</param>
    /// <returns>A result indicating whether the object is valid, with specific error details.</returns>
    /// <remarks>
    /// This method performs validation by mapping object properties to schema fields
    /// and then applying the same validation rules as the dictionary-based method.
    /// </remarks>
    IFdwResult ValidateRecord<T>(T record) where T : class;

    /// <summary>
    /// Checks if this schema is compatible with another schema.
    /// </summary>
    /// <param name="otherSchema">The schema to check compatibility with.</param>
    /// <param name="compatibilityMode">The level of compatibility to check.</param>
    /// <returns>A result indicating whether the schemas are compatible.</returns>
    /// <remarks>
    /// Schema compatibility is important for data migration, merging, and
    /// cross-system data exchange. Different compatibility modes provide
    /// different levels of strictness in the compatibility check.
    /// </remarks>
    IFdwResult CheckCompatibility(IDataSchema otherSchema, SchemaCompatibilityMode compatibilityMode);

    /// <summary>
    /// Creates a new schema with additional fields.
    /// </summary>
    /// <param name="additionalFields">The fields to add to the schema.</param>
    /// <returns>A new schema instance with the combined fields.</returns>
    /// <remarks>
    /// This method enables schema composition and extension without modifying
    /// the original schema. The original schema remains unchanged.
    /// </remarks>
    IDataSchema ExtendWith(IEnumerable<ISchemaField> additionalFields);

    /// <summary>
    /// Creates a new schema containing only the specified fields.
    /// </summary>
    /// <param name="fieldNames">The names of the fields to include in the projection.</param>
    /// <returns>A new schema instance containing only the specified fields.</returns>
    /// <remarks>
    /// This method enables schema projection for operations that only need
    /// a subset of fields, which can improve performance and reduce complexity.
    /// </remarks>
    IDataSchema ProjectTo(IEnumerable<string> fieldNames);
}

/// <summary>
/// Represents a single field definition within a data schema.
/// </summary>
/// <remarks>
/// ISchemaField describes the characteristics of a data field including
/// its type, constraints, and validation rules. This information is used
/// for data validation, type conversion, and query optimization.
/// </remarks>
public interface ISchemaField
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    /// <value>The name of the field as it appears in the data.</value>
    string Name { get; }

    /// <summary>
    /// Gets the field's display name.
    /// </summary>
    /// <value>A human-readable name for UI purposes, or the field name if not specified.</value>
    string DisplayName { get; }

    /// <summary>
    /// Gets the field's data type.
    /// </summary>
    /// <value>The .NET type that values in this field should conform to.</value>
    Type DataType { get; }

    /// <summary>
    /// Gets a value indicating whether this field is required.
    /// </summary>
    /// <value><c>true</c> if the field must have a non-null value; otherwise, <c>false</c>.</value>
    bool IsRequired { get; }

    /// <summary>
    /// Gets a value indicating whether this field is part of the primary key.
    /// </summary>
    /// <value><c>true</c> if this field is part of the primary key; otherwise, <c>false</c>.</value>
    bool IsPrimaryKey { get; }

    /// <summary>
    /// Gets a value indicating whether this field should be indexed.
    /// </summary>
    /// <value><c>true</c> if the field should be indexed for performance; otherwise, <c>false</c>.</value>
    bool IsIndexed { get; }

    /// <summary>
    /// Gets the maximum length for string fields.
    /// </summary>
    /// <value>The maximum string length, or null if not applicable or unlimited.</value>
    int? MaxLength { get; }

    /// <summary>
    /// Gets the default value for this field.
    /// </summary>
    /// <value>The default value to use when the field is not provided, or null if no default.</value>
    object? DefaultValue { get; }

    /// <summary>
    /// Gets the field description.
    /// </summary>
    /// <value>A description of the field's purpose and content.</value>
    string? Description { get; }

    /// <summary>
    /// Gets validation constraints for this field.
    /// </summary>
    /// <value>A collection of validation rules that values must satisfy.</value>
    IReadOnlyList<IFieldConstraint> Constraints { get; }

    /// <summary>
    /// Gets metadata about this field.
    /// </summary>
    /// <value>Additional properties and configuration information.</value>
    IReadOnlyDictionary<string, object> Metadata { get; }

    /// <summary>
    /// Validates a value for this field.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <returns>A result indicating whether the value is valid for this field.</returns>
    /// <remarks>
    /// This method performs comprehensive field-level validation including
    /// type checking, constraint validation, and format validation.
    /// </remarks>
    IFdwResult ValidateValue(object? value);

    /// <summary>
    /// Attempts to convert a value to the field's data type.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A result containing the converted value, or failure if conversion is not possible.</returns>
    /// <remarks>
    /// This method handles type conversion with appropriate error handling.
    /// It considers the field's data type and any format-specific conversion rules.
    /// </remarks>
    IFdwResult<object?> ConvertValue(object? value);
}

/// <summary>
/// Represents a validation constraint for a schema field.
/// </summary>
/// <remarks>
/// IFieldConstraint defines validation rules that field values must satisfy.
/// Different constraint types provide different validation behaviors such as
/// range checking, pattern matching, and custom validation logic.
/// </remarks>
public interface IFieldConstraint
{
    /// <summary>
    /// Gets the constraint type identifier.
    /// </summary>
    /// <value>The type of constraint (e.g., "Range", "Pattern", "Custom").</value>
    string ConstraintType { get; }

    /// <summary>
    /// Gets the constraint parameters.
    /// </summary>
    /// <value>Parameters that configure the constraint behavior.</value>
    IReadOnlyDictionary<string, object> Parameters { get; }

    /// <summary>
    /// Gets the error message to display when validation fails.
    /// </summary>
    /// <value>A human-readable error message describing the constraint violation.</value>
    string ErrorMessage { get; }

    /// <summary>
    /// Validates a value against this constraint.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <returns>A result indicating whether the value satisfies the constraint.</returns>
    /// <remarks>
    /// This method performs the actual constraint validation logic.
    /// The field name is provided for context in error messages.
    /// </remarks>
    IFdwResult ValidateValue(object? value, string fieldName);
}

/// <summary>
/// Specifies the level of compatibility checking between schemas.
/// </summary>
public enum SchemaCompatibilityMode
{
    /// <summary>
    /// Schemas must be identical in structure and constraints.
    /// </summary>
    Exact,

    /// <summary>
    /// All fields in the target schema must exist in the source schema with compatible types.
    /// The source schema may have additional fields.
    /// </summary>
    Backward,

    /// <summary>
    /// All fields in the source schema must exist in the target schema with compatible types.
    /// The target schema may have additional fields.
    /// </summary>
    Forward,

    /// <summary>
    /// Field names and types must match, but constraints may differ.
    /// </summary>
    Structural,

    /// <summary>
    /// Only field names must match; types may be different if convertible.
    /// </summary>
    Loose
}