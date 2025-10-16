using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Interface defining the contract for configuration source type enum options.
/// </summary>
public interface IConfigurationSourceType : IEnumOption<IConfigurationSourceType>
{
    /// <summary>
    /// Gets the description of this configuration source type.
    /// </summary>
    string Description { get; }
}