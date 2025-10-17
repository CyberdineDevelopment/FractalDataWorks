using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;
using FractalDataWorks.Data.Abstractions;

namespace FractalDataWorks.Commands.Abstractions;

/// <summary>
/// Collection of translator types.
/// </summary>
/// <remarks>
/// This collection is populated by the source generator with all types
/// that inherit from TranslatorTypeBase and implement ITranslatorType.
/// Provides high-performance lookups for translator discovery and routing.
/// </remarks>
[TypeCollection(typeof(TranslatorTypeBase), typeof(ITranslatorType), typeof(TranslatorTypes))]
public abstract partial class TranslatorTypes : TypeCollectionBase<TranslatorTypeBase, ITranslatorType>
{
    // Source generator will add:
    // - public static IReadOnlyList<ITranslatorType> All { get; }
    // - public static ITranslatorType GetById(int id)
    // - public static ITranslatorType GetByName(string name)
    // - Individual static properties for each translator type

    /// <summary>
    /// Finds translators that can convert from the source format to the target format.
    /// </summary>
    /// <param name="sourceFormat">The input format to translate from.</param>
    /// <param name="targetFormat">The output format to translate to.</param>
    /// <returns>Collection of compatible translators, ordered by priority.</returns>
    public static ITranslatorType[] FindTranslators(IDataFormat sourceFormat, IDataFormat targetFormat)
    {
        var translators = new System.Collections.Generic.List<ITranslatorType>();

        foreach (var translator in All())
        {
            if (translator.SourceFormat.Id == sourceFormat.Id &&
                translator.TargetFormat.Id == targetFormat.Id)
            {
                translators.Add(translator);
            }
        }

        translators.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        return translators.ToArray();
    }
}