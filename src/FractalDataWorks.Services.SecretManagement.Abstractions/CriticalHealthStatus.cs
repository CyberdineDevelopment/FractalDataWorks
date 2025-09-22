using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// System is completely unavailable or failed.
/// </summary>
[EnumOption("Critical")]
public sealed class CriticalHealthStatus : HealthStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CriticalHealthStatus"/> class.
    /// </summary>
    public CriticalHealthStatus() : base(5, "Critical") { }
}