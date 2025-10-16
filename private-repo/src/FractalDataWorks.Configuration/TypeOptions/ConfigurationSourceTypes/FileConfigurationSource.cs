using FractalDataWorks.Configuration.Abstractions;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Configuration;

/// <summary>
/// Represents configuration from a file.
/// </summary>
[TypeOption(typeof(ConfigurationSourceTypes), "FileConfigurationSource")]
public sealed class FileConfigurationSource : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public FileConfigurationSource() : base(1, "FileConfigurationSource", "Configuration from a file")
    {
    }
}