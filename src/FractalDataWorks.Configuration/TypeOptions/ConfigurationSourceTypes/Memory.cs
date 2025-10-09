using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents configuration from memory/cache.
/// </summary>
[TypeOption(typeof(ConfigurationSourceTypes), "Memory")]
public sealed class Memory : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Memory() : base(5, "Memory", "Configuration from memory/cache")
    {
    }
}