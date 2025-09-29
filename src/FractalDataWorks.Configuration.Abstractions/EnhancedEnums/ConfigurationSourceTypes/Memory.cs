using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents configuration from memory/cache.
/// </summary>
[EnumOption(typeof(ConfigurationSourceTypes), "Memory")]
public sealed class Memory : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Memory() : base(5, "Memory", "Configuration from memory/cache")
    {
    }
}