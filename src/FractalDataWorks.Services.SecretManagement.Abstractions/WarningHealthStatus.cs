using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// System is operational but has warnings or minor issues.
/// </summary>
[EnumOption("Warning")]
public sealed class WarningHealthStatus : HealthStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WarningHealthStatus"/> class.
    /// </summary>
    public WarningHealthStatus() : base(2, "Warning") { }
}