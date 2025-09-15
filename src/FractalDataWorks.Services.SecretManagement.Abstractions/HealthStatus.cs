using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Interface for health status levels.
/// </summary>
public interface IHealthStatus : IEnumOption<HealthStatus>
{
}

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

/// <summary>
/// Health status is unknown or could not be determined.
/// </summary>
[EnumOption("Unknown")]
public sealed class UnknownHealthStatus : HealthStatus
{
    public UnknownHealthStatus() : base(0, "Unknown") { }
}

/// <summary>
/// System is healthy and fully operational.
/// </summary>
[EnumOption("Healthy")]
public sealed class HealthyHealthStatus : HealthStatus
{
    public HealthyHealthStatus() : base(1, "Healthy") { }
}

/// <summary>
/// System is operational but has warnings or minor issues.
/// </summary>
[EnumOption("Warning")]
public sealed class WarningHealthStatus : HealthStatus
{
    public WarningHealthStatus() : base(2, "Warning") { }
}

/// <summary>
/// System is degraded but still partially functional.
/// </summary>
[EnumOption("Degraded")]
public sealed class DegradedHealthStatus : HealthStatus
{
    public DegradedHealthStatus() : base(3, "Degraded") { }
}

/// <summary>
/// System is unhealthy and not functioning properly.
/// </summary>
[EnumOption("Unhealthy")]
public sealed class UnhealthyHealthStatus : HealthStatus
{
    public UnhealthyHealthStatus() : base(4, "Unhealthy") { }
}

/// <summary>
/// System is completely unavailable or failed.
/// </summary>
[EnumOption("Critical")]
public sealed class CriticalHealthStatus : HealthStatus
{
    public CriticalHealthStatus() : base(5, "Critical") { }
}

/// <summary>
/// Collection of health statuses.
/// </summary>
[EnumCollection(CollectionName = "HealthStatuses")]
public abstract class HealthStatusCollectionBase : EnumCollectionBase<HealthStatus>
{
}