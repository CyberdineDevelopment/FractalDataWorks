using FastEndpoints;
using FractalDataWorks.Services.Connections;
using FractalDataWorks.Services.Authentication;
using FractalDataWorks.Services.SecretManagement;
using FractalDataWorks.Services.Transformations;
using FractalDataWorks.Services.DataGateway;
using FractalDataWorks.DataStores;

namespace FractalDataWorks.Web.Demo.Endpoints;

/// <summary>
/// Endpoint demonstrating ServiceType auto-discovery across all domains
/// </summary>
public class ServiceTypesEndpoint : EndpointWithoutRequest<ServiceTypesResponse>
{
    public override void Configure()
    {
        Get("/api/servicetypes");
        Summary(s =>
        {
            s.Summary = "View all auto-discovered ServiceTypes across domains";
            s.Description = "Shows the power of ServiceType auto-discovery - adding new packages automatically registers services";
            s.Response<ServiceTypesResponse>(200, "Complete ServiceType discovery information");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await SendOkAsync(new ServiceTypesResponse
        {
            Message = "🚀 ServiceType Auto-Discovery System Active",

            TotalDiscovered = ConnectionTypes.All.Count() +
                            AuthenticationTypes.All.Count() +
                            SecretManagementTypes.All.Count() +
                            TransformationTypes.All.Count() +
                            DataGatewayTypes.All.Count() +
                            DataStoreTypes.All.Count(),

            ServiceDomains = new ServiceDomainInfo[]
            {
                new()
                {
                    Domain = "Connections",
                    Description = "Database, HTTP, and other connection types",
                    Count = ConnectionTypes.All.Count(),
                    Types = ConnectionTypes.All.Select(MapServiceType).ToList(),
                    RegistrationMethod = "ConnectionTypes.Register(services)"
                },
                new()
                {
                    Domain = "Authentication",
                    Description = "Authentication providers (AzureEntra, Auth0, etc.)",
                    Count = AuthenticationTypes.All.Count(),
                    Types = AuthenticationTypes.All.Select(MapServiceType).ToList(),
                    RegistrationMethod = "AuthenticationTypes.Register(services)"
                },
                new()
                {
                    Domain = "Secret Management",
                    Description = "Secret storage providers (AzureKeyVault, HashiCorp, etc.)",
                    Count = SecretManagementTypes.All.Count(),
                    Types = SecretManagementTypes.All.Select(MapServiceType).ToList(),
                    RegistrationMethod = "SecretManagementTypes.Register(services)"
                },
                new()
                {
                    Domain = "Transformations",
                    Description = "Data transformation engines",
                    Count = TransformationTypes.All.Count(),
                    Types = TransformationTypes.All.Select(MapServiceType).ToList(),
                    RegistrationMethod = "TransformationTypes.Register(services)"
                },
                new()
                {
                    Domain = "Data Gateways",
                    Description = "Data access gateways and connectors",
                    Count = DataGatewayTypes.All.Count(),
                    Types = DataGatewayTypes.All.Select(MapServiceType).ToList(),
                    RegistrationMethod = "DataGatewayTypes.Register(services)"
                },
                new()
                {
                    Domain = "Data Stores",
                    Description = "Data storage abstractions",
                    Count = DataStoreTypes.All.Count(),
                    Types = DataStoreTypes.All.Select(MapServiceType).ToList(),
                    RegistrationMethod = "DataStoreTypes.Register(services)"
                }
            },

            AutoDiscoveryPower = new[]
            {
                "🔥 Zero-Configuration Registration: Just add package references",
                "⚡ Source Generator Magic: Automatic type discovery at compile time",
                "🎯 Type-Safe Resolution: Compile-time validation of service types",
                "🔄 Self-Assembling Architecture: Adding packages auto-extends system",
                "🚀 High Performance: Generated collections use FrozenDictionary",
                "📦 Package Independence: Each package handles its own registration"
            },

            HowItWorks = new[]
            {
                "1. [ServiceTypeCollection] attribute triggers source generator",
                "2. Generator scans all referenced assemblies for implementations",
                "3. Generates static collections with All, Name(), Id() methods",
                "4. Register() methods loop through All and call type.Register(services)",
                "5. Runtime resolution uses generated high-performance lookups"
            }
        }, ct);
    }

    private static ServiceTypeDetails MapServiceType(dynamic serviceType)
    {
        return new ServiceTypeDetails
        {
            Id = serviceType.Id,
            Name = serviceType.Name,
            Category = serviceType.Category,
            DisplayName = serviceType.DisplayName ?? serviceType.Name,
            Description = serviceType.Description ?? $"{serviceType.Name} service type"
        };
    }
}

public class ServiceTypesResponse
{
    public string Message { get; set; } = "";
    public int TotalDiscovered { get; set; }
    public ServiceDomainInfo[] ServiceDomains { get; set; } = Array.Empty<ServiceDomainInfo>();
    public string[] AutoDiscoveryPower { get; set; } = Array.Empty<string>();
    public string[] HowItWorks { get; set; } = Array.Empty<string>();
}

public class ServiceDomainInfo
{
    public string Domain { get; set; } = "";
    public string Description { get; set; } = "";
    public int Count { get; set; }
    public List<ServiceTypeDetails> Types { get; set; } = new();
    public string RegistrationMethod { get; set; } = "";
}

public class ServiceTypeDetails
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Description { get; set; } = "";
}