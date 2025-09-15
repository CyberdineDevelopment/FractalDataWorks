using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents configuration from command line arguments.
/// </summary>
[EnumOption("CommandLine")]
public sealed class CommandLine : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public CommandLine() : base(6, "CommandLine", "Configuration from command line arguments")
    {
    }
}