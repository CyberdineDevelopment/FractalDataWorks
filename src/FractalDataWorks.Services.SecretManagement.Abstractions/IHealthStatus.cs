using FractalDataWorks.EnhancedEnums;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Interface for health status levels.
/// </summary>
public interface IHealthStatus : IEnumOption<HealthStatus>
{
}