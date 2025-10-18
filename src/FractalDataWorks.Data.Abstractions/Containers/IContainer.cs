namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Represents a data container (table, view, API endpoint, file) with schema metadata.
/// </summary>
public interface IContainer
{
    /// <summary>
    /// Container name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Path to this container within its data store.
    /// </summary>
    IPath Path { get; }

    /// <summary>
    /// Schema definition with field roles.
    /// </summary>
    IContainerSchema Schema { get; }

    /// <summary>
    /// Translator type names (resolved at runtime from configuration).
    /// Example: ["TSqlQuery", "OData"]
    /// </summary>
    string[] TranslatorTypeNames { get; }

    /// <summary>
    /// Format type names (resolved at runtime from configuration).
    /// Example: ["Tabular", "Json"]
    /// </summary>
    string[] FormatTypeNames { get; }

    /// <summary>
    /// Resolved translators (populated after type resolution).
    /// </summary>
    ITranslator[] Translators { get; set; }

    /// <summary>
    /// Resolved formats (populated after type resolution).
    /// </summary>
    IFormat[] Formats { get; set; }
}
