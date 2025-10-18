using System.Collections.Generic;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Schema definition for a container (table, view, endpoint, file).
/// Contains field definitions with Identity/Attribute/Measure roles.
/// </summary>
public interface IContainerSchema
{
    /// <summary>
    /// All fields in this container.
    /// </summary>
    IReadOnlyList<IField> Fields { get; }

    /// <summary>
    /// Get fields with Identity role (primary keys, unique identifiers).
    /// </summary>
    IReadOnlyList<IField> GetIdentityFields();

    /// <summary>
    /// Get fields with Attribute role (descriptive, dimensional).
    /// </summary>
    IReadOnlyList<IField> GetAttributeFields();

    /// <summary>
    /// Get fields with Measure role (numeric, aggregatable).
    /// </summary>
    IReadOnlyList<IField> GetMeasureFields();

    /// <summary>
    /// Get a specific field by name (case-insensitive).
    /// </summary>
    IField? GetField(string name);

    /// <summary>
    /// Whether this schema contains nested types (arrays or objects).
    /// </summary>
    bool SupportsNesting { get; }
}
