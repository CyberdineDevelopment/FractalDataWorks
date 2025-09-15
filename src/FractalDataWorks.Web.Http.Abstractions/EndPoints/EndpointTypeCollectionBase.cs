using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// Collection class for EndpointType enhanced enums.
/// </summary>
[StaticEnumCollection(CollectionName = "EndpointTypes", DefaultGenericReturnType = typeof(IEndpointType), UseSingletonInstances = true)]
public abstract class EndpointTypeCollectionBase : EnumCollectionBase<EndpointTypeBase>
{
}