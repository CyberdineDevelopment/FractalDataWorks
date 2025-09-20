using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.Security;

/// <summary>
/// Collection class for SecurityMethod enhanced enums.
/// </summary>
[TypeCollection(CollectionName = "SecurityMethods", DefaultGenericReturnType = typeof(ISecurityMethod))]
public abstract class SecurityMethodCollectionBase : TypeCollectionBase<SecurityMethodBase>
{
}