using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Abstractions;

/// <summary>
/// Collection base class for service lifetime types.
/// Source generator will populate this with all service lifetime options.
/// </summary>
[EnumCollection(CollectionName = "ServiceLifetimes")]
public abstract class ServiceLifetimeCollectionBase : EnumCollectionBase<ServiceLifetimeBase>
{
    // Source generator populates this automatically
}