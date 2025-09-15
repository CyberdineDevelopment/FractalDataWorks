using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FractalDataWorks.Services.DataGateway.Services;
//#if (IncludeHealthChecks)
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
//#endif
//#if (EnableSwagger)
using FastEndpoints.Swagger;
//#endif
//#if (DatabaseProvider == "SqlServer")
using Microsoft.EntityFrameworkCore;
//#endif
//#if (DatabaseProvider == "PostgreSQL")
using Npgsql.EntityFrameworkCore.PostgreSQL;
//#endif

var builder = WebApplication.CreateBuilder(args);

// Add FastEndpoints
builder.Services.AddFastEndpoints();

// Add FractalDataWorks Services
builder.Services.AddScoped<IDataGateway, DataGateway>();

//#if (EnableSwagger)
// Add Swagger support
builder.Services.SwaggerDocument();
//#endif

//#if (AuthenticationType == "JWT")
// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:JWT:Issuer"],
            ValidAudience = builder.Configuration["Authentication:JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Authentication:JWT:SecretKey"]!))
        };
    });
//#endif

//#if (AuthenticationType == "ApiKey")
// Add API Key Authentication
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });
//#endif

//#if (EnableRateLimiting)
// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
    
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded", cancellationToken: token);
    };
});
//#endif

//#if (EnableCORS)
// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "*" })
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
//#endif


//#if (DatabaseProvider == "SqlServer")
// Add SQL Server Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
//#endif

//#if (DatabaseProvider == "PostgreSQL")
// Add PostgreSQL Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
//#endif

//#if (IncludeHealthChecks)
// Add Health Checks
builder.Services.AddHealthChecks()
//#if (DatabaseProvider == "SqlServer")
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "database")
//#endif
//#if (DatabaseProvider == "PostgreSQL")
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!, name: "database")
//#endif
    .AddCheck("api", () => HealthCheckResult.Healthy("API is responding"));
//#endif

var app = builder.Build();

//#if (EnableCORS)
// Use CORS
app.UseCors("DefaultPolicy");
//#endif

//#if (EnableRateLimiting)
// Use Rate Limiting
app.UseRateLimiter();
//#endif

//#if (AuthenticationType != "None")
// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();
//#endif

//#if (EnableSwagger)
// Use Swagger in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}
//#endif

//#if (IncludeHealthChecks)
// Use Health Checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                exception = x.Value.Exception?.Message,
                duration = x.Value.Duration.ToString()
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});
//#endif

// Configure FastEndpoints
app.UseFastEndpoints(c =>
{
    c.Versioning.Prefix = "v";
    c.Versioning.PrependToRoute = true;
//#if (EnableSwagger)
    c.Endpoints.RouteModifier = (r => r.ToLowerInvariant());
//#endif
});

app.Run();

//#if (AuthenticationType == "ApiKey")
// API Key Authentication Handler
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedApiKey = apiKeyHeaderValues.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var validApiKey = Context.RequestServices.GetRequiredService<IConfiguration>()["Authentication:ApiKey:Key"];
        if (!string.Equals(providedApiKey, validApiKey, StringComparison.Ordinal))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "API User"),
            new Claim(ClaimTypes.NameIdentifier, "api-user")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class ApiKeyAuthenticationSchemeOptions : AuthenticationSchemeOptions { }
//#endif

//#if (DatabaseProvider != "None")
// Database Context
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Add your DbSets here
    // public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure your entity models here
    }
}
//#endif