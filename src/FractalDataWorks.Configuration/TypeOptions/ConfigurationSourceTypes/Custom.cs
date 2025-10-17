using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents a custom configuration source.
/// </summary>
[TypeOption(typeof(ConfigurationSourceTypes), "Custom")]
public sealed class Custom : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Custom() : base(7, "Custom", "Custom configuration source")
    {
    }
}