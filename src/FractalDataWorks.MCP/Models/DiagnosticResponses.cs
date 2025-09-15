namespace RoslynMcpServer.Models;

public sealed record DiagnosticSummaryResponse
{
    public bool Success { get; init; } = true;
    public string? Error { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public DiagnosticSummary? Summary { get; init; }
}

public sealed record DiagnosticListResponse
{
    public bool Success { get; init; } = true;
    public string? Error { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public List<DiagnosticInfo> Diagnostics { get; init; } = new();
    public int TotalCount { get; init; }
    public bool HasMore { get; init; }
    public string? NextCursor { get; init; }
}

public sealed record DiagnosticOverviewResponse
{
    public bool Success { get; init; } = true;
    public string? Error { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public int TotalCount { get; init; }
    public List<string> TopErrorRules { get; init; } = new();
    public List<string> TopWarningRules { get; init; } = new();
    public List<string> AffectedProjects { get; init; } = new();
}