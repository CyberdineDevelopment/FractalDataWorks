using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Collection of configuration change types.
/// </summary>
[EnumCollection(typeof(ConfigurationChangeTypeBase), typeof(ConfigurationChangeTypeBase), typeof(ConfigurationChangeTypes))]
public abstract class ConfigurationChangeTypes : EnumCollectionBase<ConfigurationChangeTypeBase>
{
    // DO NOT IMPLEMENT BY HAND!
    // Source generator automatically creates static ConfigurationChangeTypes class with:
    // - ConfigurationChangeTypes.Added (returns ConfigurationChangeTypeBase)
    // - ConfigurationChangeTypes.Updated (returns ConfigurationChangeTypeBase)
    // - ConfigurationChangeTypes.Deleted (returns ConfigurationChangeTypeBase)
    // - ConfigurationChangeTypes.Reloaded (returns ConfigurationChangeTypeBase)
    // - ConfigurationChangeTypes.All (collection of ConfigurationChangeTypeBase)
    // - ConfigurationChangeTypes.GetById(int id) (returns ConfigurationChangeTypeBase)
    // - ConfigurationChangeTypes.GetByName(string name) (returns ConfigurationChangeTypeBase)
}