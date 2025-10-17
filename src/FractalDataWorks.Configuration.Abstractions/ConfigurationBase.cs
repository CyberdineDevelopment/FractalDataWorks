using System;

namespace FractalDataWorks.Configuration.Abstractions;

/// <summary>
/// Base class for all configuration types in the FractalDataWorks framework.
/// Provides common metadata properties and automatic service type identification.
/// </summary>
/// <typeparam name="T">The derived configuration type (for type-safe chaining).</typeparam>
/// <remarks>
/// <para>
/// This base class provides:
/// <list type="bullet">
/// <item><description>Automatic ServiceType = nameof(T)</description></item>
/// <item><description>Identity (Id, Name, SectionName)</description></item>
/// <item><description>Audit timestamps (CreatedAt, ModifiedAt)</description></item>
/// </list>
/// </para>
/// <para>
/// Services can inherit this class or implement IGenericConfiguration directly.
/// </para>
/// <example>
/// <code>
/// public class EmailSettings : ConfigurationBase&lt;EmailSettings&gt;
/// {
///     public override string SectionName => "Email";
///
///     public string SmtpHost { get; set; } = "smtp.gmail.com";
///     public int SmtpPort { get; set; } = 587;
/// }
/// </code>
/// </example>
/// </remarks>
public abstract class ConfigurationBase<T> : IGenericConfiguration<T>
    where T : ConfigurationBase<T>
{
    /// <summary>
    /// Gets or sets the unique identifier for this configuration instance.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of this configuration for lookup and display.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets the section name for this configuration in appsettings or database.
    /// Must be overridden by derived classes.
    /// </summary>
    /// <example>"Email", "Database", "Notification"</example>
    public abstract string SectionName { get; }

    /// <summary>
    /// Gets the service type identifier for this configuration.
    /// Automatically set to typeof(T).Name.
    /// </summary>
    /// <remarks>
    /// This enables type-safe identification without reflection at runtime.
    /// For EmailSettings : ConfigurationBase&lt;EmailSettings&gt;, this returns "EmailSettings".
    /// </remarks>
    public string ServiceType { get; } = typeof(T).Name;

    /// <summary>
    /// Gets the timestamp when this configuration instance was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when this configuration was last modified.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Marks this configuration as modified by setting ModifiedAt to current UTC time.
    /// </summary>
    protected void MarkAsModified()
    {
        ModifiedAt = DateTime.UtcNow;
    }
}
