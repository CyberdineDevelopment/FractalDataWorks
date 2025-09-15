using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace RoslynMcpServer.Models;

public sealed record ChangePreview
{
    public string SessionId { get; init; } = string.Empty;
    public List<FileChange> Changes { get; init; } = new();
    public DiagnosticComparison DiagnosticDiff { get; init; } = new();
    public bool CompilationSucceeded { get; init; }
    public List<string> CompilationErrors { get; init; } = new();
    public DateTime PreviewTime { get; init; } = DateTime.UtcNow;
}

public sealed record FileChange
{
    public string FilePath { get; init; } = string.Empty;
    public ChangeType Type { get; init; }
    public string? OldContent { get; init; }
    public string? NewContent { get; init; }
    public List<TextChange> TextChanges { get; init; } = new();
}


public enum ChangeType
{
    Modified,
    Added,
    Removed,
    Renamed
}

public sealed record CommitResult
{
    public bool Success { get; init; }
    public List<string> ModifiedFiles { get; init; } = new();
    public List<string> Errors { get; init; } = new();
    public DateTime CommitTime { get; init; } = DateTime.UtcNow;
}

public sealed record AnalyzerConfiguration
{
    public bool UseDefaults { get; init; } = true;
    public List<string> AdditionalAnalyzers { get; init; } = new();
    public List<string> DisabledRules { get; init; } = new();
    public Dictionary<string, string> RuleSeverities { get; init; } = new(StringComparer.Ordinal);
}

public sealed record CodeFixConfiguration
{
    public StringComparisonDefault DefaultStringComparison { get; init; } = new();
    public RegexTimeoutSettings RegexTimeouts { get; init; } = new();
    public NullDefenseSettings NullDefense { get; init; } = new();
    public bool EnableRedundantNullDefenseDetection { get; init; } = true;
}

public sealed record StringComparisonDefault
{
    public string CaseSensitive { get; init; } = "StringComparison.Ordinal";
    public string CaseInsensitive { get; init; } = "StringComparison.OrdinalIgnoreCase";
}

public sealed record RegexTimeoutSettings
{
    public int DefaultTimeoutMs { get; init; } = 1000;
    public int MaxTimeoutMs { get; init; } = 5000;
    public bool EnableTimeoutValidation { get; init; } = true;
}

public sealed record NullDefenseSettings
{
    public bool DetectRedundantArgumentNullChecks { get; init; } = true;
    public bool DetectRedundantNullForgivingOperators { get; init; } = true;
    public bool SuggestNullableAnnotations { get; init; } = true;
}

public sealed record MissingTypesAnalysis
{
    public string SessionId { get; init; } = string.Empty;
    public int TotalMissingTypes { get; init; }
    public int TotalOccurrences { get; init; }
    public Dictionary<string, List<MissingTypeLocation>> MissingTypesByName { get; init; } = new(StringComparer.Ordinal);
    public DateTime AnalyzedAt { get; init; } = DateTime.UtcNow;
}

public sealed record MissingTypeLocation
{
    public string ProjectName { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public int LineNumber { get; init; }
    public int ColumnNumber { get; init; }
    public string LineText { get; init; } = string.Empty;
    public string DiagnosticMessage { get; init; } = string.Empty;
}

public sealed record UsingStatementSuggestion
{
    public string NamespaceName { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public string FullTypeName { get; init; } = string.Empty;
    public string AssemblyName { get; init; } = string.Empty;
    public bool IsSystemType { get; init; }
    public double Confidence { get; init; }
}

public sealed record UsingStatementFix
{
    public string TypeName { get; init; } = string.Empty;
    public string NamespaceName { get; init; } = string.Empty;
    public List<string> FilePaths { get; init; } = new();
}

public sealed record UsingAdditionResult
{
    public string FilePath { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public string NamespaceName { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}