using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// System is degraded but still partially functional.
/// </summary>
[EnumOption("Degraded")]
public sealed class DegradedHealthStatus : HealthStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DegradedHealthStatus"/> class.
    /// </summary>
    public DegradedHealthStatus() : base(4, "Degraded") { }
}