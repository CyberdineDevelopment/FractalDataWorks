using Microsoft.CodeAnalysis;

namespace RoslynMcpServer.Models;

public sealed record TypeInfoModel
{
    public string Name { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Namespace { get; init; } = string.Empty;
    public string AssemblyName { get; init; } = string.Empty;
    public string ContainingFile { get; init; } = string.Empty;
    public TypeKind Kind { get; init; }
    public bool IsGeneric { get; init; }
    public List<string> GenericParameters { get; init; } = new();
    public List<MemberInfo> Members { get; init; } = new();
    public int DeclarationCount { get; init; }
    public bool IsPartial { get; init; }
    public List<string> DeclarationFiles { get; init; } = new();
}

public sealed record MemberInfo
{
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public MemberKind Kind { get; init; }
    public string Signature { get; init; } = string.Empty;
    public bool IsStatic { get; init; }
    public bool IsPublic { get; init; }
    public string ContainingFile { get; init; } = string.Empty;
    public int Line { get; init; }
}

public enum MemberKind
{
    Field,
    Property,
    Method,
    Constructor,
    Event,
    Indexer,
    Operator
}

public sealed record AmbiguousTypeInfo
{
    public string Name { get; init; } = string.Empty;
    public List<TypeInfoModel> Conflicts { get; init; } = new();
    public int ConflictCount => Conflicts.Count;
}

public sealed record RefactoringRequest
{
    public string SessionId { get; init; } = string.Empty;
    public RefactoringType Type { get; init; }
    public Dictionary<string, object> Parameters { get; init; } = new(StringComparer.Ordinal);
}

public enum RefactoringType
{
    Rename,
    SeparateTypes,
    ApplyCodeFix,
    ExtractMethod,
    InlineVariable
}