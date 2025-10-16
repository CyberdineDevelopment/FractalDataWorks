using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using FractalDataWorks.EnhancedEnums.Analyzers;

namespace FractalDataWorks.EnhancedEnums.CodeFixes;

/// <summary>
/// Code fix provider that adds AllowMultiple = true to EnumLookup attributes to resolve duplicate lookup value diagnostics.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DuplicateLookupValueCodeFixProvider)), Shared]
public class DuplicateLookupValueCodeFixProvider : CodeFixProvider
{
    private const string Title = "Add AllowMultiple = true";

    /// <summary>
    /// Gets the diagnostic IDs that this code fix provider can address.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds
        => [DuplicateLookupValueAnalyzer.DiagnosticId];

    /// <summary>
    /// Gets the fix all provider that enables fixing multiple instances of the diagnostic.
    /// </summary>
    /// <returns>The batch fixer for applying fixes to multiple diagnostics.</returns>
    public sealed override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    /// <summary>
    /// Registers code fixes for the diagnostics present in the given context.
    /// </summary>
    /// <param name="context">The code fix context containing diagnostics and document information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the property declaration
        var property = root.FindToken(diagnosticSpan.Start)
            .Parent?
            .AncestorsAndSelf()
            .OfType<PropertyDeclarationSyntax>()
            .FirstOrDefault();

        if (property == null) return;

        // Find the EnumLookup attribute
        var enumLookupAttribute = property.AttributeLists
            .SelectMany(al => al.Attributes)
            .FirstOrDefault(a => IsEnumLookupAttribute(a));

        if (enumLookupAttribute == null) return;

        // Register the code fix
        context.RegisterCodeFix(
            CodeAction.Create(
                title: Title,
                createChangedDocument: c => AddAllowMultipleAsync(
                    context.Document, 
                    enumLookupAttribute, 
                    c),
                equivalenceKey: Title),
            diagnostic);
    }

    private static async Task<Document> AddAllowMultipleAsync(
        Document document,
        AttributeSyntax attribute,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        var newAttribute = AddAllowMultipleToAttribute(attribute);
        var newRoot = root.ReplaceNode(attribute, newAttribute);
        
        return document.WithSyntaxRoot(newRoot);
    }

    private static AttributeSyntax AddAllowMultipleToAttribute(AttributeSyntax attribute)
    {
        // Check if the attribute already has arguments
        if (attribute.ArgumentList == null)
        {
            // No arguments, create new argument list with method name and AllowMultiple
            // This shouldn't happen as EnumLookup requires at least a method name
            return attribute;
        }

        var arguments = attribute.ArgumentList.Arguments;
        
        // Check if AllowMultiple is already present
        var hasAllowMultiple = arguments.Any(arg => 
            string.Equals(arg.NameEquals?.Name.Identifier.Text, "AllowMultiple", StringComparison.Ordinal));

        if (hasAllowMultiple)
        {
            // AllowMultiple exists, update its value
            var newArguments = arguments.Select(arg =>
            {
                if (string.Equals(arg.NameEquals?.Name.Identifier.Text, "AllowMultiple", StringComparison.Ordinal))
                {
                    return SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals(
                            SyntaxFactory.IdentifierName("AllowMultiple")),
                        null,
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.TrueLiteralExpression));
                }
                return arg;
            });

            return attribute.WithArgumentList(
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SeparatedList(newArguments)));
        }
        else
        {
            // Check if there's already a boolean second parameter
            if (arguments.Count >= 2 && arguments[1].NameEquals == null)
            {
                // Second positional parameter exists, replace it with true
                var newArguments = arguments.Select((arg, index) =>
                {
                    if (index == 1)
                    {
                        return SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.TrueLiteralExpression));
                    }
                    return arg;
                }).ToList();

                return attribute.WithArgumentList(
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList(newArguments)));
            }
            else if (arguments.Count == 1)
            {
                // Only method name exists, add true as second parameter
                var newArguments = arguments.ToList();
                newArguments.Add(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.TrueLiteralExpression)));

                return attribute.WithArgumentList(
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList(newArguments)));
            }
            else
            {
                // Add AllowMultiple as named parameter
                var newArguments = arguments.ToList();
                newArguments.Add(
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.NameEquals(
                            SyntaxFactory.IdentifierName("AllowMultiple")),
                        null,
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.TrueLiteralExpression)));

                return attribute.WithArgumentList(
                    SyntaxFactory.AttributeArgumentList(
                        SyntaxFactory.SeparatedList(newArguments)));
            }
        }
    }

    private static bool IsEnumLookupAttribute(AttributeSyntax attribute)
    {
        var name = attribute.Name.ToString();
        return string.Equals(name, "EnumLookup", StringComparison.Ordinal) || 
               string.Equals(name, "EnumLookupAttribute", StringComparison.Ordinal) ||
               name.EndsWith(".EnumLookup", StringComparison.Ordinal) ||
               name.EndsWith(".EnumLookupAttribute", StringComparison.Ordinal);
    }
}
