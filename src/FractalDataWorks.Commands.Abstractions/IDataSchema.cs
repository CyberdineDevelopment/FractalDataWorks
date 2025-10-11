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