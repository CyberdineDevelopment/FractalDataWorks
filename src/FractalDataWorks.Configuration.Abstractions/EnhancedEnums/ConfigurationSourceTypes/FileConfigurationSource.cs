using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Represents configuration from a file.
/// </summary>
[EnumOption("FileConfigurationSource")]
public sealed class FileConfigurationSource : ConfigurationSourceTypeBase
{
    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public FileConfigurationSource() : base(1, "FileConfigurationSource", "Configuration from a file")
    {
    }
}