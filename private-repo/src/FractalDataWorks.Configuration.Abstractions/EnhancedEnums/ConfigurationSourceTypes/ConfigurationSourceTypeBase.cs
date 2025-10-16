using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Base class for configuration source types.
/// </summary>
public abstract class ConfigurationSourceTypeBase : EnumOptionBase<ConfigurationSourceTypeBase>, IEnumOption<ConfigurationSourceTypeBase>, IConfigurationSourceType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationSourceTypeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="name">The name of the configuration source type.</param>
    /// <param name="description">The description of the configuration source type.</param>
    protected ConfigurationSourceTypeBase(int id, string name, string description) 
        : base(id, name)
    {
        Description = description;
    }
    
    /// <summary>
    /// Description of the configuration source type.
    /// </summary>
    public string Description { get; }
}