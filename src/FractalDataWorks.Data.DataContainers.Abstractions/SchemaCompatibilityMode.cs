using System;
using System.Collections.Generic;
using FractalDataWorks.Results;

namespace FractalDataWorks.Data.DataContainers.Abstractions;

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