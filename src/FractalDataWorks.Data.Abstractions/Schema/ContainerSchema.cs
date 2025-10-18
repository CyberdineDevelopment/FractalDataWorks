using System;
using System.Collections.Generic;
using System.Linq;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Implementation of container schema with field role filtering.
/// </summary>
public sealed class ContainerSchema : IContainerSchema
{
    /// <summary>
    /// Gets or initializes all fields in this container.
    /// </summary>
    public required IReadOnlyList<IField> Fields { get; init; }

    /// <summary>
    /// Get fields with Identity role (primary keys, unique identifiers).
    /// </summary>
    public IReadOnlyList<IField> GetIdentityFields() =>
        Fields.Where(f => f.Role == FieldRole.Identity).ToList();

    /// <summary>
    /// Get fields with Attribute role (descriptive, dimensional).
    /// </summary>
    public IReadOnlyList<IField> GetAttributeFields() =>
        Fields.Where(f => f.Role == FieldRole.Attribute).ToList();

    /// <summary>
    /// Get fields with Measure role (numeric, aggregatable).
    /// </summary>
    public IReadOnlyList<IField> GetMeasureFields() =>
        Fields.Where(f => f.Role == FieldRole.Measure).ToList();

    /// <summary>
    /// Get a specific field by name (case-insensitive).
    /// </summary>
    public IField? GetField(string name) =>
        Fields.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Whether this schema contains nested types (arrays or objects).
    /// </summary>
    public bool SupportsNesting =>
        Fields.Any(f => f.FieldType.IsNested);
}
