using FractalDataWorks.ServiceTypes;

namespace FractalDataWorks.Services.DomainName.Abstractions.ServiceTypes;

/// <summary>
/// Base class for DomainName service type definitions.
/// </summary>
/// <typeparam name="TService">The service interface type.</typeparam>
/// <typeparam name="TFactory">The factory interface type.</typeparam>
/// <typeparam name="TConfiguration">The configuration interface type.</typeparam>
public abstract class DomainNameTypeBase<TService, TFactory, TConfiguration>
    : ServiceTypeBase<TService, TFactory, TConfiguration>, IDomainNameType
    where TService : class
    where TFactory : class
    where TConfiguration : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainNameTypeBase{TService, TFactory, TConfiguration}"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for this service type.</param>
    /// <param name="name">The name of this service type.</param>
    /// <param name="category">The category for grouping service types.</param>
    protected DomainNameTypeBase(int id, string name, string category)
        : base(id, name, category)
    {
    }
}
