using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents configuration from a remote service.
/// </summary>
[EnumOption(typeof(ConfigurationSourceTypes), "Remote")]
public sealed class Remote : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Remote() : base(4, "Remote", "Configuration from a remote service")
    {
    }
}