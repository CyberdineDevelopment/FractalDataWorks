using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// Collection class for RateLimitPolicy enhanced enums.
/// </summary>
[EnumCollection(CollectionName = "RateLimitPolicies", DefaultGenericReturnType = typeof(IRateLimitPolicy))]
public abstract class RateLimitPolicyCollectionBase : EnumCollectionBase<RateLimitPolicyBase>
{
}