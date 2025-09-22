using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// System is healthy and fully operational.
/// </summary>
[EnumOption("Healthy")]
public sealed class HealthyHealthStatus : HealthStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthyHealthStatus"/> class.
    /// </summary>
    public HealthyHealthStatus() : base(1, "Healthy") { }
}