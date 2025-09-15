using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace RoslynMcpServer.Models;

public sealed class CompilationSession
{
    public string Id { get; init; } = string.Empty;
    public string SolutionPath { get; init; } = string.Empty;
    public Workspace Workspace { get; init; } = null!;
    public Solution Solution { get; set; } = null!;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    public ImmutableArray<DiagnosticAnalyzer> Analyzers { get; set; } = ImmutableArray<DiagnosticAnalyzer>.Empty;
    public Dictionary<DocumentId, SyntaxTree> PendingChanges { get; } = new();
    public bool HasPendingChanges => PendingChanges.Count > 0;
    
    // Pause/Resume support for incremental compilation
    public bool IsPaused { get; set; }
    public DateTime? PausedAt { get; set; }
    
    public void UpdateAccess() => LastAccessedAt = DateTime.UtcNow;
}