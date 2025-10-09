namespace FractalDataWorks.Services.DataGateway.Abstractions.Models;

/// <summary>
/// Categorizes data fields based on their semantic role.
/// </summary>
public enum DatumCategory
{
    /// <summary>
    /// Identifier field (e.g., primary keys, IDs).
    /// </summary>
    Identifier,

    /// <summary>
    /// Regular property field.
    /// </summary>
    Property,

    /// <summary>
    /// Measure or metric field (e.g., amounts, counts, totals).
    /// </summary>
    Measure,

    /// <summary>
    /// Metadata field (e.g., timestamps, version info).
    /// </summary>
    Metadata
}
