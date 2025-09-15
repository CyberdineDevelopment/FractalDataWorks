using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using FractalDataWorks.MCP.Extensions;
using FractalDataWorks.MCP.Services;
using RoslynMcpServer.Logging;
using RoslynMcpServer.Services;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoslynMcpServer;

internal sealed partial class Program
{
    private Program() { }
    
    static async Task<int> Main(string[] args)
    {
        // CRITICAL: Register MSBuild FIRST before any MSBuild types are used
        if (!MSBuildLocator.IsRegistered)
        {
            try
            {
                // Try multiple registration strategies
                var instances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
                if (instances.Length > 0)
                {
                    var instance = instances.OrderByDescending(x => x.Version).First();
                    Console.WriteLine($"[ROSLYN-MCP] Registering MSBuild: {instance.Name} v{instance.Version}");
                    MSBuildLocator.RegisterInstance(instance);
                }
                else
                {
                    Console.WriteLine("[ROSLYN-MCP] No VS instances found, trying RegisterDefaults");
                    try
                    {
                        MSBuildLocator.RegisterDefaults();
                        Console.WriteLine("[ROSLYN-MCP] RegisterDefaults succeeded");
                    }
                    catch (Exception defaultsEx)
                    {
                        Console.WriteLine($"[ROSLYN-MCP] RegisterDefaults failed: {defaultsEx.Message}");
                        
                        // Try manual registration with .NET SDK path
                        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT") ?? 
                                        (Environment.Is64BitOperatingSystem ? 
                                         @"C:\Program Files\dotnet" : 
                                         @"C:\Program Files (x86)\dotnet");
                        
                        var sdkPath = Path.Combine(dotnetRoot, "sdk");
                        if (Directory.Exists(sdkPath))
                        {
                            var sdkVersions = Directory.GetDirectories(sdkPath)
                                .Select(d => new { Path = d, Version = Path.GetFileName(d) })
                                .Where(v => Version.TryParse(v.Version.Split('-')[0], out _))
                                .OrderByDescending(v => v.Version, StringComparer.Ordinal)
                                .ToArray();
                                
                            if (sdkVersions.Length > 0)
                            {
                                var latestSdk = sdkVersions[0];
                                Console.WriteLine($"[ROSLYN-MCP] Manually registering .NET SDK: {latestSdk.Version}");
                                MSBuildLocator.RegisterMSBuildPath(latestSdk.Path);
                            }
                            else
                            {
                                Console.WriteLine("[ROSLYN-MCP] No .NET SDK versions found");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[ROSLYN-MCP] .NET SDK path not found: {sdkPath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ROSLYN-MCP] All MSBuild registration attempts failed: {ex.Message}");
                // Continue without MSBuild registration - basic loading only
            }
        }
        
        // Create bootstrap logger that will be replaced by configuration-based logger
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateBootstrapLogger();
        
        try
        {
            Log.Information("Roslyn MCP Server starting up");
            Log.Information("Process ID: {ProcessId}, .NET Version: {DotNetVersion}", 
                Environment.ProcessId, Environment.Version);
            Log.Debug("Args: {Args}", string.Join(" ", args));
            
            // Log MSBuild registration status
            if (MSBuildLocator.IsRegistered)
            {
                var registeredInstance = MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault();
                if (registeredInstance != null)
                {
                    Log.Information("MSBuild registered: {Name} v{Version}", 
                        registeredInstance.Name, registeredInstance.Version);
                }
                else
                {
                    Log.Information("MSBuild registered via defaults");
                }
            }
            else
            {
                Log.Warning("MSBuild not registered - project loading will be limited");
            }
            
            var builder = Host.CreateApplicationBuilder(args);
        
        // Configure Serilog as the logging provider using configuration
        builder.Services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());
        
        // Configure OpenTelemetry
        var serviceName = "RoslynMcpServer";
        var serviceVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
                .AddAttributes(new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["host.name"] = Environment.MachineName,
                    ["process.pid"] = Environment.ProcessId
                }))
            .WithTracing(tracing => tracing
                .AddSource(serviceName)
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                }))
            .WithMetrics(metrics => metrics
                .AddMeter(serviceName)
                .AddRuntimeInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317");
                }));

        // Register core services
        builder.Services.AddSingleton<ProjectDependencyService>();
        builder.Services.AddSingleton<CompilationCacheService>();
        builder.Services.AddSingleton<AnalyzerService>();
        builder.Services.AddSingleton<WorkspaceSessionManager>();
        builder.Services.AddSingleton<VirtualEditService>();
        builder.Services.AddSingleton<FileSystemWatcherService>();

        // Register plugin system and all services/plugins
        builder.Services.AddFractalMcpServices(builder.Configuration);
        builder.Services.AddFractalMcpPlugins();

        // Configure MCP server with plugin-based tools
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .AddTools(sp =>
            {
                var registry = sp.GetRequiredService<IPluginRegistry>();
                var tools = registry.GetAllTools();
                return tools.Cast<ITool>().ToList();
            });

        var host = builder.Build();

        // Get logger from host for source-generated logging
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        logger.ServerStarted(Environment.ProcessId, Environment.Version.ToString());
        logger.HostBuiltSuccessfully();

        // Initialize plugin system
        var pluginRegistry = host.Services.GetRequiredService<IPluginRegistry>();

        // Discover and load all plugins
        var loadResult = await pluginRegistry.DiscoverAndLoadPluginsAsync();
        if (loadResult.IsSuccess)
        {
            Log.Information("Loaded {Count} plugins successfully", loadResult.Value);
        }
        else
        {
            Log.Warning("Plugin loading completed with warnings: {Message}", loadResult.Message);
        }

        // Initialize all plugins
        var initResult = await pluginRegistry.InitializeAllPluginsAsync();
        if (!initResult.IsSuccess)
        {
            Log.Warning("Some plugins failed to initialize: {Message}", initResult.Message);
        }

        // Display loaded plugins and tools
        var plugins = pluginRegistry.GetAllPlugins();
        var tools = pluginRegistry.GetAllTools();

        Console.WriteLine($"MCP Server configured with {plugins.Count} plugins and {tools.Count} tools:");
        Console.WriteLine();

        foreach (var plugin in plugins.OrderBy(p => p.Category?.DisplayPriority ?? 999))
        {
            Console.WriteLine($"Plugin: {plugin.Name} ({plugin.Category?.Name ?? "Unknown"})");
            var pluginTools = plugin.GetTools();
            foreach (var tool in pluginTools.Take(5))
            {
                Console.WriteLine($"  - {tool.Name}");
            }
            if (pluginTools.Count > 5)
            {
                Console.WriteLine($"  ... and {pluginTools.Count - 5} more tools");
            }
        }

        Console.WriteLine();
        Console.WriteLine("IMPORTANT: Sessions persist until explicitly ended - reuse for multiple operations!");
        Console.WriteLine("Server is running. Use Ctrl+C to stop.");
        Console.WriteLine();

            WorkspaceSessionManager? sessionManager = null;
            try
            {
                sessionManager = host.Services.GetRequiredService<WorkspaceSessionManager>();
                await host.RunAsync();
            }
            catch (OperationCanceledException ex)
            {
                Log.Information(ex, "Server stopped gracefully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal error occurred");
                Environment.Exit(1);
            }
            finally
            {
                // Gracefully shutdown all plugins
                try
                {
                    await pluginRegistry.ShutdownAllPluginsAsync();
                    Log.Information("All plugins shut down successfully");
                }
                catch (Exception shutdownEx)
                {
                    Log.Warning(shutdownEx, "Error during plugin shutdown");
                }

                sessionManager?.Dispose();
                Log.Information("Server shutdown complete");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Startup failed");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
        
        return 0;
    }
}
