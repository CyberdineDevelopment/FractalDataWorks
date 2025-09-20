using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Policies;

/// <summary>
/// Collection class for RateLimitPolicy enhanced enums.
/// </summary>
[TypeCollection(CollectionName = "RateLimitPolicies", DefaultGenericReturnType = typeof(IRateLimitPolicy))]
public abstract class RateLimitPolicyCollectionBase : TypeCollectionBase<RateLimitPolicyBase>
{
}