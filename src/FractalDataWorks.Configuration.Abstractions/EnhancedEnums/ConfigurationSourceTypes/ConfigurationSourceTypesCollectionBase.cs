using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Collection of configuration source types.
/// </summary>
[EnumCollection(CollectionName = "ConfigurationSourceTypes", DefaultGenericReturnType = typeof(ConfigurationSourceTypeBase))]
public abstract class ConfigurationSourceTypesCollectionBase : EnumCollectionBase<ConfigurationSourceTypeBase>
{
    // DO NOT IMPLEMENT BY HAND!
    // Source generator automatically creates static ConfigurationSourceTypes class with:
    // - ConfigurationSourceTypes.FileConfigurationSource (returns ConfigurationSourceTypeBase)
    // - ConfigurationSourceTypes.Environment (returns ConfigurationSourceTypeBase)
    // - ConfigurationSourceTypes.Database (returns ConfigurationSourceTypeBase)
    // - ConfigurationSourceTypes.Remote (returns ConfigurationSourceTypeBase)
    // - ConfigurationSourceTypes.Memory (returns ConfigurationSourceTypeBase)
    // - ConfigurationSourceTypes.CommandLine (returns ConfigurationSourceTypeBase)
    // - ConfigurationSourceTypes.Custom (returns ConfigurationSourceTypeBase)
    // - ConfigurationSourceTypes.All (collection of ConfigurationSourceTypeBase)
    // - ConfigurationSourceTypes.GetById(int id) (returns ConfigurationSourceTypeBase)
    // - ConfigurationSourceTypes.GetByName(string name) (returns ConfigurationSourceTypeBase)
}