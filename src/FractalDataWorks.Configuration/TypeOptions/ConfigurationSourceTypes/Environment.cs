using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents configuration from environment variables.
/// </summary>
[TypeOption(typeof(ConfigurationSourceTypes), "Environment")]
public sealed class Environment : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Environment() : base(2, "Environment", "Configuration from environment variables")
    {
    }
}