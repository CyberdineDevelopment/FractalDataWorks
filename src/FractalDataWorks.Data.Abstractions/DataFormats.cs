using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Data.Abstractions;

/// <summary>
/// Collection of data formats.
/// </summary>
/// <remarks>
/// This collection is populated by the source generator with all types
/// that inherit from DataFormatBase and implement IDataFormat.
/// Provides standard data formats used across the framework.
/// </remarks>
[EnhancedEnumCollection(typeof(DataFormatBase), typeof(IDataFormat), typeof(DataFormats))]
public partial class DataFormats
{
    // Source generator will add:
    // - public static IReadOnlyList<IDataFormat> All { get; }
    // - public static IDataFormat GetById(int id)
    // - public static IDataFormat GetByName(string name)
    // - Individual static properties for each format
}