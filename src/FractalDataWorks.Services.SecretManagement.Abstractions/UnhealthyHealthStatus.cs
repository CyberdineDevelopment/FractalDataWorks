using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// System is unhealthy and not functioning properly.
/// </summary>
[EnumOption("Unhealthy")]
public sealed class UnhealthyHealthStatus : HealthStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnhealthyHealthStatus"/> class.
    /// </summary>
    public UnhealthyHealthStatus() : base(4, "Unhealthy") { }
}