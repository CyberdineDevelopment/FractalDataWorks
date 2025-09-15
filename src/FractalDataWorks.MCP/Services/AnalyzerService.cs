using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using RoslynMcpServer.Models;
using System.Collections.Immutable;
using System.Reflection;
namespace RoslynMcpServer.Services;

public sealed class AnalyzerService
{
    private readonly ImmutableArray<DiagnosticAnalyzer> _defaultAnalyzers;
    private readonly Dictionary<string, ImmutableArray<DiagnosticAnalyzer>> _analyzerCache = new(StringComparer.Ordinal);

    public AnalyzerService()
    {
        _defaultAnalyzers = LoadDefaultAnalyzers();
    }

    public async Task<ImmutableArray<DiagnosticAnalyzer>> LoadAnalyzersAsync(AnalyzerConfiguration config)
    {
        var cacheKey = GetCacheKey(config);
        if (_analyzerCache.TryGetValue(cacheKey, out var cached))
            return cached;

        var analyzers = ImmutableArray<DiagnosticAnalyzer>.Empty;
        
        if (config.UseDefaults)
        {
            analyzers = analyzers.AddRange(_defaultAnalyzers);
        }

        foreach (var additionalAnalyzer in config.AdditionalAnalyzers)
        {
            var loaded = await LoadAnalyzerFromPackageOrPathAsync(additionalAnalyzer);
            if (!loaded.IsEmpty)
                analyzers = analyzers.AddRange(loaded);
        }

        analyzers = FilterDisabledAnalyzers(analyzers, config.DisabledRules);
        
        _analyzerCache[cacheKey] = analyzers;
        return analyzers;
    }

    public static List<AnalyzerInfo> GetAnalyzerInfo(ImmutableArray<DiagnosticAnalyzer> analyzers)
    {
        return analyzers.Select(analyzer => new AnalyzerInfo
        {
            Name = analyzer.GetType().Name,
            AssemblyName = analyzer.GetType().Assembly.GetName().Name ?? "Unknown",
            SupportedDiagnostics = analyzer.SupportedDiagnostics.Select(d => new DiagnosticDescriptorInfo
            {
                Id = d.Id,
                Title = d.Title.ToString(),
                Category = d.Category,
                Severity = d.DefaultSeverity,
                Description = d.Description.ToString(),
                HelpLinkUri = d.HelpLinkUri
            }).ToList()
        }).ToList();
    }

    public static Task<bool> HasCodeFixProviderAsync(string diagnosticId)
    {
        return Task.FromResult(
            diagnosticId.StartsWith("CS") || // Compiler diagnostics often have fixes
            diagnosticId.StartsWith("CA") || // Code Analysis fixes
            diagnosticId.StartsWith("IDE")   // IDE suggestions
        );
    }

    private static ImmutableArray<DiagnosticAnalyzer> LoadDefaultAnalyzers()
    {
        var analyzers = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
        
        // Add our custom analyzer
        
        // Load analyzers from referenced assemblies
        var assemblyNames = new[]
        {
            "Microsoft.CodeAnalysis.CSharp",
            "Microsoft.CodeAnalysis.Features", 
            "Microsoft.CodeAnalysis.CSharp.Features",
            "Roslynator.CSharp.Analyzers",
            "AsyncFixer",
            "Meziantou.Analyzer"
        };

        foreach (var assemblyName in assemblyNames)
        {
            var loaded = LoadAnalyzersFromAssembly(assemblyName);
            if (!loaded.IsEmpty)
                analyzers.AddRange(loaded);
        }

        return analyzers.ToImmutable();
    }

    private static ImmutableArray<DiagnosticAnalyzer> LoadAnalyzersFromAssembly(string assemblyName)
    {
        try
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = loadedAssemblies.FirstOrDefault(a => 
                a.GetName().Name?.Contains(assemblyName, StringComparison.OrdinalIgnoreCase) == true);

            if (assembly == null)
            {
                try
                {
                    assembly = Assembly.LoadFrom($"{assemblyName}.dll");
                }
                catch
                {
                    return ImmutableArray<DiagnosticAnalyzer>.Empty;
                }
            }

            var analyzerTypes = assembly.GetTypes()
                .Where(type => typeof(DiagnosticAnalyzer).IsAssignableFrom(type) && 
                              !type.IsAbstract && 
                              type.IsPublic)
                .ToArray();

            var analyzers = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
            
            foreach (var type in analyzerTypes)
            {
                try
                {
                    if (Activator.CreateInstance(type) is DiagnosticAnalyzer analyzer)
                        analyzers.Add(analyzer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create analyzer {type.Name}: {ex.Message}");
                }
            }

            return analyzers.ToImmutable();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load analyzers from {assemblyName}: {ex.Message}");
            return ImmutableArray<DiagnosticAnalyzer>.Empty;
        }
    }

    private static async Task<ImmutableArray<DiagnosticAnalyzer>> LoadAnalyzerFromPackageOrPathAsync(string analyzerName)
    {
        if (File.Exists(analyzerName))
        {
            try
            {
                var assembly = Assembly.LoadFrom(analyzerName);
                return LoadAnalyzersFromAssembly(assembly.GetName().Name ?? analyzerName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load analyzer from path {analyzerName}: {ex.Message}");
            }
        }

        return await Task.FromResult(LoadAnalyzersFromAssembly(analyzerName));
    }

    private static ImmutableArray<DiagnosticAnalyzer> FilterDisabledAnalyzers(
        ImmutableArray<DiagnosticAnalyzer> analyzers, 
        List<string> disabledRules)
    {
        if (!disabledRules.Any())
            return analyzers;

        return analyzers
            .Where(analyzer => !analyzer.SupportedDiagnostics
                .Any(diagnostic => disabledRules.Contains(diagnostic.Id, StringComparer.Ordinal)))
            .ToImmutableArray();
    }

    private static string GetCacheKey(AnalyzerConfiguration config)
    {
        var parts = new List<string>
        {
            config.UseDefaults.ToString(),
            string.Join(",", config.AdditionalAnalyzers.OrderBy(x => x, StringComparer.Ordinal)),
            string.Join(",", config.DisabledRules.OrderBy(x => x, StringComparer.Ordinal))
        };
        
        return string.Join("|", parts);
    }
}

public sealed record AnalyzerInfo
{
    public string Name { get; init; } = string.Empty;
    public string AssemblyName { get; init; } = string.Empty;
    public List<DiagnosticDescriptorInfo> SupportedDiagnostics { get; init; } = new();
}

public sealed record DiagnosticDescriptorInfo
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public DiagnosticSeverity Severity { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? HelpLinkUri { get; init; }
}