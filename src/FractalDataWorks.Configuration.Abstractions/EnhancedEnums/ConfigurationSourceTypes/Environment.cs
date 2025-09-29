using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents configuration from environment variables.
/// </summary>
[EnumOption(typeof(ConfigurationSourceTypes), "Environment")]
public sealed class Environment : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Environment() : base(2, "Environment", "Configuration from environment variables")
    {
    }
}