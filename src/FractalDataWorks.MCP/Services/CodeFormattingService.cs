using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RoslynMcpServer.Services;

public static class CodeFormattingService
{
    /// <summary>
    /// Get code string with optional comment removal for reduced context usage
    /// </summary>
    public static string FormatCode(SyntaxNode node, bool includeComments = false)
    {
        if (includeComments)
        {
            return node.ToFullString();
        }

        return node.ToString(); // ToString() excludes trivia (comments, whitespace)
    }

    /// <summary>
    /// Get code string from syntax tree with optional comment removal
    /// </summary>
    public static string FormatCode(SyntaxTree syntaxTree, bool includeComments = false)
    {
        var root = syntaxTree.GetRoot();
        return FormatCode(root, includeComments);
    }

    /// <summary>
    /// Format code with normalized whitespace (removes extra blank lines, consistent indentation)
    /// </summary>
    public static string FormatCodeNormalized(SyntaxNode node, bool includeComments = false)
    {
        var normalizedNode = node.NormalizeWhitespace();
        return FormatCode(normalizedNode, includeComments);
    }
}