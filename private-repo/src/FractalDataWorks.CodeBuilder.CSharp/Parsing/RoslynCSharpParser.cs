using System;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.CodeBuilder.Abstractions;
using FractalDataWorks.Results;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace FractalDataWorks.CodeBuilder.CSharp.Parsing;

/// <summary>
/// Roslyn-based parser for C# code.
/// </summary>
public sealed class RoslynCSharpParser : ICodeParser
{
    /// <inheritdoc/>
    public string Language => "csharp";

    /// <inheritdoc/>
    public async Task<IGenericResult<ISyntaxTree>> ParseAsync(
        string sourceCode,
        string? filePath = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(sourceCode))
        {
            return GenericResult<ISyntaxTree>.Failure("Source code cannot be null or empty");
        }

        try
        {
            return await Task.Run(() =>
            {
                // Parse the source code using Roslyn
                var options = CSharpParseOptions.Default
                    .WithLanguageVersion(LanguageVersion.Latest)
                    .WithDocumentationMode(DocumentationMode.Parse);

                var syntaxTree = CSharpSyntaxTree.ParseText(
                    sourceCode,
                    options,
                    filePath ?? string.Empty,
                    cancellationToken: cancellationToken);

                var roslynTree = new RoslynSyntaxTree(syntaxTree, sourceCode, Language, filePath);
                return GenericResult<ISyntaxTree>.Success(roslynTree);
            }, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return GenericResult<ISyntaxTree>.Failure("Parse operation was cancelled");
        }
        catch (Exception ex)
        {
            return GenericResult<ISyntaxTree>.Failure($"Parse error: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<IGenericResult> ValidateAsync(
        string sourceCode,
        CancellationToken cancellationToken = default)
    {
        var parseResult = await ParseAsync(sourceCode, null, cancellationToken).ConfigureAwait(false);
        
        if (parseResult.IsFailure)
        {
            return GenericResult.Failure(parseResult.CurrentMessage ?? "Validation failed");
        }

        if (parseResult.Value!.HasErrors)
        {
            var errorCount = 0;
            foreach (var _ in parseResult.Value.GetErrors())
            {
                errorCount++;
            }
            return GenericResult.Failure($"Source code contains {errorCount} syntax error(s)");
        }

        return GenericResult.Success();
    }
}
