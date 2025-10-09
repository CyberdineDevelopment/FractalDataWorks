using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents configuration from a remote service.
/// </summary>
[TypeOption(typeof(ConfigurationSourceTypes), "Remote")]
public sealed class Remote : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Remote() : base(4, "Remote", "Configuration from a remote service")
    {
    }
}