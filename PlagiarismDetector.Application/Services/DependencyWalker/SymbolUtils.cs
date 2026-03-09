using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlagiarismDetector.Application.Services.DependencyWalker;
public static class SymbolUtils
{
    public static ITypeSymbol? GetITypeSymbol(ISymbol symbol)
    {
        switch (symbol)
        {
            case ILocalSymbol localSymbol:
                return localSymbol.Type;
            case IFieldSymbol fieldSymbol:
                return fieldSymbol.Type;
            case IPropertySymbol propertySymbol:
                return propertySymbol.Type;
            case IParameterSymbol parameterSymbol:
                return parameterSymbol.Type;
            case IMethodSymbol methodSymbol:
                return methodSymbol.ReturnType;
            default:
                return null;
        }
    }

    public static IEnumerable<ISymbol> GetSymbolsInScope(
        SemanticModel semanticModel,
        SyntaxNode? node,
        bool read = true
    )
    {
        if (node == null)
            yield break;

        var flow = semanticModel.AnalyzeDataFlow(node);

        if (flow == null)
            yield break;

        IEnumerable<ISymbol> dependencies = Array.Empty<ISymbol>();

        if (read)
        {
            dependencies = flow.ReadInside.Where(s =>
                s.Kind == SymbolKind.Local
                || s.Kind == SymbolKind.Parameter
                || s.Kind == SymbolKind.Field
                || s.Kind == SymbolKind.Property
                || s.Kind == SymbolKind.Method
            );

            // Method invocations
            IEnumerable<InvocationExpressionSyntax> methodInvocations =
                node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>();

            foreach (var invocation in methodInvocations)
            {
                var symbol = semanticModel.GetSymbolInfo(invocation).Symbol;

                if (symbol != null)
                    yield return symbol;
            }

            // Class instantiations
            IEnumerable<ObjectCreationExpressionSyntax> objectCreations =
                node.DescendantNodesAndSelf().OfType<ObjectCreationExpressionSyntax>();

            foreach (var objectCreation in objectCreations)
            {
                var symbol = semanticModel.GetSymbolInfo(objectCreation).Symbol;

                if (symbol != null)
                    yield return symbol;
            }

            // Element accesses
            IEnumerable<ElementAccessExpressionSyntax> elementAccesses =
                node.DescendantNodesAndSelf().OfType<ElementAccessExpressionSyntax>();

            foreach (var elementAccess in elementAccesses)
            {
                var symbol = semanticModel.GetSymbolInfo(elementAccess.Expression).Symbol;

                if (symbol != null)
                    yield return symbol;
            }
        }
        else
        {
            dependencies = flow.WrittenInside.Where(s =>
                s.Kind == SymbolKind.Local
                || s.Kind == SymbolKind.Parameter
                || s.Kind == SymbolKind.Field
                || s.Kind == SymbolKind.Property
                || s.Kind == SymbolKind.Method
            );
        }

        foreach (var symbol in dependencies)
        {
            yield return symbol;
        }

        yield break;
    }
}