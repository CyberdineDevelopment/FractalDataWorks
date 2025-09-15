using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.ServiceTypes;
using FractalDataWorks.ServiceTypes.Attributes;

namespace FractalDataWorks.Services.Connections.Abstractions;

/// <summary>
/// Concrete collection of all connection service types in the system.
/// This partial class will be extended by the source generator to include
/// all discovered connection types with high-performance lookup capabilities.
/// </summary>
[ServiceTypeCollection("ConnectionTypeBase", "ConnectionTypes")]
public partial class ConnectionTypesBase : 
    ConnectionTypeCollectionBase<
        ConnectionTypeBase<IFdwConnection, IConnectionConfiguration, IConnectionFactory<IFdwConnection, IConnectionConfiguration>>,
        ConnectionTypeBase<IFdwConnection, IConnectionConfiguration, IConnectionFactory<IFdwConnection, IConnectionConfiguration>>,
        IFdwConnection,
        IConnectionConfiguration,
        IConnectionFactory<IFdwConnection, IConnectionConfiguration>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionTypesBase"/> class.
    /// The source generator will populate all discovered connection types.
    /// </summary>
    public ConnectionTypesBase()
    {
        // Source generator will add:
        // - Static fields for each connection type (e.g., SqlServer, Http, etc.)
        // - FrozenDictionary for O(1) lookups by Id/Name
        // - Factory methods for each constructor overload
        // - Empty() method returning default instance
        // - All() method returning all connection types
    }
}