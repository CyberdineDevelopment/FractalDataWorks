using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// TypeCollection for all translator type implementations.
/// Translators convert universal data requests to domain-specific queries (SQL, OData, GraphQL, etc.).
/// </summary>
[TypeCollection(typeof(TranslatorTypeBase), typeof(ITranslatorType), typeof(TranslatorTypes))]
public sealed partial class TranslatorTypes : TypeCollectionBase<TranslatorTypeBase, ITranslatorType>
{
    // TypeCollectionGenerator will generate all members
}
