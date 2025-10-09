using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents a configuration that was added.
/// </summary>
[TypeOption(typeof(ConfigurationChangeTypes), "Added")]
public sealed class Added : ConfigurationChangeTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public Added() : base(1, "Added", "A configuration was added")
    {
    }
}