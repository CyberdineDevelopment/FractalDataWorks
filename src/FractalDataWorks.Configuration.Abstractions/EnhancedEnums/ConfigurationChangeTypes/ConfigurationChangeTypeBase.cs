using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Base class for configuration change types.
/// </summary>
public abstract class ConfigurationChangeTypeBase : EnumOptionBase<ConfigurationChangeTypeBase>, IEnumOption<ConfigurationChangeTypeBase>, IConfigurationChangeType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationChangeTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name of the configuration change type.</param>
    /// <param name="description">The description of the configuration change type.</param>
    protected ConfigurationChangeTypeBase(int id, string name, string description) 
        : base(id, name)
    {
        Description = description;
    }
    
    /// <summary>
    /// Description of the configuration change type.
    /// </summary>
    public string Description { get; }
}