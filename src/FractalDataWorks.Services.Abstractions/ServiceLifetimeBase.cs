using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.EnhancedEnums.Attributes;

namespace FractalDataWorks.Services.Abstractions;

/// <summary>
/// Interface for service lifetime types.
/// </summary>
public interface IServiceLifeTime : IEnumOption<ServiceLifetimeBase>
{

}
/// <summary>
/// Base class for service lifetime types.
/// </summary>
[EnumCollection(CollectionName = "ServiceLifeTimes", DefaultGenericReturnType = typeof(IServiceLifeTime))]
public abstract class ServiceLifetimeBase : EnumOptionBase<ServiceLifetimeBase>
{
    /// <summary>
    /// Gets a description of when this lifetime should be used.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceLifetimeBase"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the lifetime type.</param>
    /// <param name="name">The name of the lifetime type.</param>
    /// <param name="description">A description of when this lifetime should be used.</param>
    protected ServiceLifetimeBase(int id, string name, string description)
        : base(id, name)
    {
        Description = description;
    }
}