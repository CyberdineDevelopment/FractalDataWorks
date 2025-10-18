using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Represents a translator type definition - metadata about query translators.
/// </summary>
/// <remarks>
/// Translator types convert universal data requests to domain-specific queries (SQL, OData, GraphQL, etc.).
/// </remarks>
public interface ITranslatorType : ITypeOption
{
    /// <summary>
    /// Gets the domain this translator targets (Sql, Rest, GraphQL, File).
    /// </summary>
    string Domain { get; }
}
