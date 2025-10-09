using System.Collections.Generic;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Represents the schema definition for data structures.
/// </summary>
/// <remarks>
/// Data schemas define the structure, types, and constraints of data
/// used in command translation and execution contexts.
/// </remarks>
public interface IDataSchema
{
    /// <summary>
    /// Gets the schema name.
    /// </summary>
    /// <value>The unique name identifying this schema.</value>
    string Name { get; }

    /// <summary>
    /// Gets the schema version.
    /// </summary>
    /// <value>Version string for schema compatibility checking.</value>
    string Version { get; }

    /// <summary>
    /// Gets the field definitions in this schema.
    /// </summary>
    /// <value>Collection of field metadata.</value>
    IReadOnlyCollection<ISchemaField> Fields { get; }

    /// <summary>
    /// Gets the primary key field names.
    /// </summary>
    /// <value>Names of fields that form the primary key.</value>
    IReadOnlyCollection<string> PrimaryKeyFields { get; }

    /// <summary>
    /// Gets the schema constraints.
    /// </summary>
    /// <value>Collection of constraints applied to this schema.</value>
    IReadOnlyCollection<ISchemaConstraint> Constraints { get; }

    /// <summary>
    /// Gets additional schema metadata.
    /// </summary>
    /// <value>Key-value pairs of schema metadata.</value>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Represents a field in a data schema.
/// </summary>
public interface ISchemaField
{
    /// <summary>
    /// Gets the field name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the field data type.
    /// </summary>
    string DataType { get; }

    /// <summary>
    /// Gets whether the field allows null values.
    /// </summary>
    bool IsNullable { get; }

    /// <summary>
    /// Gets whether the field is part of the primary key.
    /// </summary>
    bool IsPrimaryKey { get; }

    /// <summary>
    /// Gets the maximum length for string fields.
    /// </summary>
    int? MaxLength { get; }

    /// <summary>
    /// Gets the field metadata.
    /// </summary>
    IReadOnlyDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Represents a constraint on a data schema.
/// </summary>
public interface ISchemaConstraint
{
    /// <summary>
    /// Gets the constraint name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the constraint type.
    /// </summary>
    string ConstraintType { get; }

    /// <summary>
    /// Gets the fields affected by this constraint.
    /// </summary>
    IReadOnlyCollection<string> AffectedFields { get; }
}