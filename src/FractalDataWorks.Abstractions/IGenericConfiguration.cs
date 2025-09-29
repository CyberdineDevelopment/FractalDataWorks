namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Base interface for all configuration objects in the FractalDataWorks framework.
/// Provides common properties for all configuration types.
/// </summary>
public interface IGenericConfiguration
{
    /// <summary>
    /// Gets the unique identifier for this configuration instance.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Gets the name of this configuration for lookup and display.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the section name for this configuration in appsettings.
    /// </summary>
    string SectionName { get; }

    /// <summary>
    /// Gets a value indicating whether this configuration is enabled.
    /// </summary>
    bool IsEnabled { get; }
}
