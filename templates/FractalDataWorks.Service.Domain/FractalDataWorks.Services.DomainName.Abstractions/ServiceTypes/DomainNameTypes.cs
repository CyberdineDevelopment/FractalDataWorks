using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Services.DomainName.Abstractions.ServiceTypes;

/// <summary>
/// Collection of DomainName service types.
/// Source generator will create static properties for each [ServiceTypeOption] type found.
/// </summary>
[ServiceTypeCollection(typeof(DomainNameTypeBase<,,>), typeof(IDomainNameType), typeof(DomainNameTypes))]
public partial class DomainNameTypes : ServiceTypeCollectionBase<DomainNameTypeBase<object, object, object>, IDomainNameType>
{
}
