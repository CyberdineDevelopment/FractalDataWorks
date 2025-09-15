using FastEndpoints;
using FastEndpoints.Swagger;
using Serilog;
using FractalDataWorks.Services.Connections;
using FractalDataWorks.Services.Authentication;
using FractalDataWorks.Services.SecretManagement;
using FractalDataWorks.Services.Transformations;
using FractalDataWorks.Services.DataGateway;
using FractalDataWorks.DataStores;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Host.UseSerilog();

// Add FastEndpoints
builder.Services.AddFastEndpoints();

// Add Swagger with FastEndpoints
builder.Services.SwaggerDocument(o =>
{
    o.DocumentSettings = s =>
    {
        s.Title = "FractalDataWorks ServiceType Demo API";
        s.Description = "Demonstration of auto-discovery ServiceType pattern";
        s.Version = "v1";
    };
});

// ✨ THE MAGIC: Auto-register ALL discovered service types with a single line each
Console.WriteLine("🚀 Registering ServiceTypes via Auto-Discovery...");

// Register all connection types (MsSql, PostgreSql, Http, etc.)
ConnectionTypes.Register(builder.Services);
Console.WriteLine($"   ✅ Connections: {ConnectionTypes.All.Count()} types discovered");

// Register all authentication types (AzureEntra, Auth0, etc.)
AuthenticationTypes.Register(builder.Services);
Console.WriteLine($"   ✅ Authentication: {AuthenticationTypes.All.Count()} types discovered");

// Register all secret management types (AzureKeyVault, HashiCorp, etc.)
SecretManagementTypes.Register(builder.Services);
Console.WriteLine($"   ✅ Secret Management: {SecretManagementTypes.All.Count()} types discovered");

// Register all transformation types (Standard, Custom, etc.)
TransformationTypes.Register(builder.Services);
Console.WriteLine($"   ✅ Transformations: {TransformationTypes.All.Count()} types discovered");

// Register all data gateway types (Sql, NoSql, etc.)
DataGatewayTypes.Register(builder.Services);
Console.WriteLine($"   ✅ Data Gateways: {DataGatewayTypes.All.Count()} types discovered");

// Register all data store types (File, Database, Cloud, etc.)
DataStoreTypes.Register(builder.Services);
Console.WriteLine($"   ✅ Data Stores: {DataStoreTypes.All.Count()} types discovered");

var app = builder.Build();

// Configure FastEndpoints pipeline
app.UseFastEndpoints(c =>
{
    c.Errors.UseProblemDetails();
    c.Serializer.RequestDeserializer = async (req, tParam, jCtx, ct) =>
    {
        return await req.ReadFromJsonAsync(tParam, jCtx, ct);
    };
});

// Add Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

// Root endpoint showing ServiceType discovery summary
app.MapGet("/", () => new
{
    Message = "FractalDataWorks ServiceType Auto-Discovery Demo",
    ServiceTypes = new
    {
        Connections = ConnectionTypes.All.Select(c => new { c.Id, c.Name, c.Category }).ToList(),
        Authentication = AuthenticationTypes.All.Select(a => new { a.Id, a.Name, a.Category }).ToList(),
        SecretManagement = SecretManagementTypes.All.Select(s => new { s.Id, s.Name, s.Category }).ToList(),
        Transformations = TransformationTypes.All.Select(t => new { t.Id, t.Name, t.Category }).ToList(),
        DataGateways = DataGatewayTypes.All.Select(d => new { d.Id, d.Name, d.Category }).ToList(),
        DataStores = DataStoreTypes.All.Select(ds => new { ds.Id, ds.Name, ds.Category }).ToList()
    },
    Instructions = "Visit /swagger to see available endpoints demonstrating ServiceType patterns"
});

Console.WriteLine("🎉 ServiceType Auto-Discovery Complete!");
Console.WriteLine("   Navigate to /swagger to explore the API");

app.Run();
