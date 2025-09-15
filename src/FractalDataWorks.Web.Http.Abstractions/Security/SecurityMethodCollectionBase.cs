using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

/// <summary>
/// Collection class for SecurityMethod enhanced enums.
/// </summary>
[StaticEnumCollection(CollectionName = "SecurityMethods", DefaultGenericReturnType = typeof(ISecurityMethod), UseSingletonInstances = true)]
public abstract class SecurityMethodCollectionBase : EnumCollectionBase<SecurityMethodBase>
{
}