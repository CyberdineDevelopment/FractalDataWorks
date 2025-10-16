using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents configuration from command line arguments.
/// </summary>
[TypeOption(typeof(ConfigurationSourceTypes), "CommandLine")]
public sealed class CommandLine : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public CommandLine() : base(6, "CommandLine", "Configuration from command line arguments")
    {
    }
}