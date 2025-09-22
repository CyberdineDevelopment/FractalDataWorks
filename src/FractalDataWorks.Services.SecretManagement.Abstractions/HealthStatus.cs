using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Base class for health status levels.
/// </summary>
public abstract class HealthStatus : EnumOptionBase<HealthStatus>, IHealthStatus
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthStatus"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this health status.</param>
    /// <param name="name">The name of this health status.</param>
    protected HealthStatus(int id, string name) : base(id, name)
    {
    }
}