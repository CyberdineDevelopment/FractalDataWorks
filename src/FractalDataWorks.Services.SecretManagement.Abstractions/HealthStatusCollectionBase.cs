using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.SecretManagement.Abstractions;

/// <summary>
/// Collection of health statuses.
/// </summary>
[EnumCollection(CollectionName = "HealthStatuses")]
public abstract class HealthStatusCollectionBase : EnumCollectionBase<HealthStatus>
{
}