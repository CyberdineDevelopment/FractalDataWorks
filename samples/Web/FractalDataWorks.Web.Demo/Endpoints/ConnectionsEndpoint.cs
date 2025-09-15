using FastEndpoints;
using FractalDataWorks.Services.Connections;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Web.Demo.Endpoints;

/// <summary>
/// Endpoint demonstrating ConnectionProvider usage with ServiceType auto-discovery
/// </summary>
public class ConnectionsEndpoint : EndpointWithoutRequest<ConnectionsResponse>
{
    private readonly IFdwConnectionProvider _connectionProvider;

    public ConnectionsEndpoint(IFdwConnectionProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public override void Configure()
    {
        Get("/api/connections");
        Summary(s =>
        {
            s.Summary = "Demonstrate ConnectionProvider with ServiceType auto-discovery";
            s.Description = "Shows how ConnectionProvider resolves connections using auto-discovered ConnectionTypes";
            s.Response<ConnectionsResponse>(200, "Connection information and discovery details");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Demonstrate configuration-based connection resolution
        var databaseConnection = await _connectionProvider.GetConnection("Database");
        var analyticsConnection = await _connectionProvider.GetConnection("Analytics");

        await SendOkAsync(new ConnectionsResponse
        {
            Message = "ConnectionProvider successfully resolving connections via ServiceType auto-discovery",

            DiscoveredConnectionTypes = ConnectionTypes.All.Select(c => new ConnectionTypeInfo
            {
                Id = c.Id,
                Name = c.Name,
                Category = c.Category,
                FactoryType = c.FactoryType.Name,
                ConfigurationType = c.ConfigurationType?.Name ?? "Unknown"
            }).ToList(),

            ConnectionResolution = new ConnectionResolutionInfo
            {
                DatabaseConnection = new ConnectionTestResult
                {
                    ConfigurationName = "Database",
                    Success = databaseConnection.IsSuccess,
                    ConnectionType = databaseConnection.IsSuccess ? "MsSql" : "Failed",
                    Error = databaseConnection.IsSuccess ? null : databaseConnection.Error
                },
                AnalyticsConnection = new ConnectionTestResult
                {
                    ConfigurationName = "Analytics",
                    Success = analyticsConnection.IsSuccess,
                    ConnectionType = analyticsConnection.IsSuccess ? "MsSql" : "Failed",
                    Error = analyticsConnection.IsSuccess ? null : analyticsConnection.Error
                }
            },

            AutoDiscoveryBenefits = new[]
            {
                "✅ Zero manual registration - just ConnectionTypes.Register(services)",
                "✅ Adding new connection packages auto-registers them",
                "✅ Configuration-driven connection resolution",
                "✅ Type-safe factory and translator injection",
                "✅ Self-assembling architecture"
            }
        }, ct);
    }
}

public class ConnectionsResponse
{
    public string Message { get; set; } = "";
    public List<ConnectionTypeInfo> DiscoveredConnectionTypes { get; set; } = new();
    public ConnectionResolutionInfo ConnectionResolution { get; set; } = new();
    public string[] AutoDiscoveryBenefits { get; set; } = Array.Empty<string>();
}

public class ConnectionTypeInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string FactoryType { get; set; } = "";
    public string ConfigurationType { get; set; } = "";
}

public class ConnectionResolutionInfo
{
    public ConnectionTestResult DatabaseConnection { get; set; } = new();
    public ConnectionTestResult AnalyticsConnection { get; set; } = new();
}

public class ConnectionTestResult
{
    public string ConfigurationName { get; set; } = "";
    public bool Success { get; set; }
    public string ConnectionType { get; set; } = "";
    public string? Error { get; set; }
}