using FractalDataWorks.Collections;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Base class for translator type definitions.
/// Provides metadata about query translators that convert universal requests to domain-specific queries.
/// </summary>
public abstract class TranslatorTypeBase : TypeOptionBase<TranslatorTypeBase>, ITranslatorType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatorTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this translator type.</param>
    /// <param name="name">The name of this translator type.</param>
    /// <param name="displayName">The display name for this translator type.</param>
    /// <param name="description">The description of this translator type.</param>
    /// <param name="domain">The domain this translator targets (Sql, Rest, GraphQL, File).</param>
    /// <param name="category">The category for this translator type (defaults to "Translator").</param>
    protected TranslatorTypeBase(
        int id,
        string name,
        string displayName,
        string description,
        string domain,
        string? category = null)
        : base(id, name, $"Translators:{name}", displayName, description, category ?? "Translator")
    {
        Domain = domain;
    }

    /// <inheritdoc/>
    public string Domain { get; }
}
