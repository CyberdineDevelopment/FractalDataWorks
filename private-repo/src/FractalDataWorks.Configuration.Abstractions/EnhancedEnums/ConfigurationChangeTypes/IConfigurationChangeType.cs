using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Interface defining the contract for configuration change type enum options.
/// </summary>
public interface IConfigurationChangeType : IEnumOption<IConfigurationChangeType>
{
    /// <summary>
    /// Gets the description of this configuration change type.
    /// </summary>
    string Description { get; }
}