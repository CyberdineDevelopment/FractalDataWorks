using Microsoft.CodeAnalysis;

namespace RoslynMcpServer.Models;

public sealed record DiagnosticSummary
{
    public int TotalCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public int InfoCount { get; init; }
    public int HiddenCount { get; init; }
    public Dictionary<string, int> ByRule { get; init; } = new(StringComparer.Ordinal);
    public Dictionary<string, int> ByProject { get; init; } = new(StringComparer.Ordinal);
    public Dictionary<string, int> ByFile { get; init; } = new(StringComparer.Ordinal);
    public Dictionary<DiagnosticSeverity, int> BySeverity { get; init; } = new();
}

public sealed record DiagnosticInfo
{
    public string Id { get; init; } = string.Empty;
    public DiagnosticSeverity Severity { get; init; }
    public string Message { get; init; } = string.Empty;
    public string File { get; init; } = string.Empty;
    public int Line { get; init; }
    public int Column { get; init; }
    public string Project { get; init; } = string.Empty;
    public string? Category { get; init; }
    public bool HasCodeFix { get; init; }
}

public sealed record DiagnosticComparison
{
    public List<DiagnosticInfo> Resolved { get; init; } = new();
    public List<DiagnosticInfo> Introduced { get; init; } = new();
    public List<DiagnosticInfo> Unchanged { get; init; } = new();
    public int ResolvedCount => Resolved.Count;
    public int IntroducedCount => Introduced.Count;
    public int UnchangedCount => Unchanged.Count;
}