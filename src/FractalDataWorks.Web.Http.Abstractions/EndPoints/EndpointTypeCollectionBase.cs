using FractalDataWorks.Collections;
using FractalDataWorks.Collections.Attributes;

namespace FractalDataWorks.Web.Http.Abstractions.EndPoints;

/// <summary>
/// Collection class for EndpointType enhanced enums.
/// </summary>
[TypeCollection(CollectionName = "EndpointTypes", DefaultGenericReturnType = typeof(IEndpointType))]
public abstract class EndpointTypeCollectionBase : TypeCollectionBase<EndpointTypeBase>
{
}