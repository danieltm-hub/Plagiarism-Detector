using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlagiarismDetector.Application.Services.DependencyWalker;
public class DependencyDataFlowWalker : CSharpSyntaxWalker
{
    private readonly SemanticModel _semanticModel;
    private readonly bool _verbose;
    private Queue<List<ISymbol>> _logicDependencies = new Queue<List<ISymbol>>();

    public Dictionary<ISymbol, DependencyNode<ISymbol>> DependencyGraph { get; } =
        new Dictionary<ISymbol, DependencyNode<ISymbol>>(SymbolEqualityComparer.Default);

    public DependencyDataFlowWalker(SemanticModel semanticModel, bool verbose = false)
        : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        _semanticModel = semanticModel;
        _verbose = verbose;
    }

    private DependencyNode<ISymbol>? GetDependencyNode(ISymbol? symbol)
    {
        if (symbol == null)
            return null;

        var type = SymbolUtils.GetITypeSymbol(symbol);

        if (symbol.Name == "this")
            return null;

        if (DependencyGraph.ContainsKey(symbol))
            return DependencyGraph[symbol];

        if (type == null)
        {
            if (symbol is not INamedTypeSymbol)
                return null;

            DependencyGraph[symbol] = new DependencyNode<ISymbol>(symbol, symbol.Name);
        }
        else
        {
            // if (type.TypeKind == TypeKind.Class)
            //     DependencyGraph[symbol] = new DependencyNode<ISymbol>(symbol, "Class");
            // else
            DependencyGraph[symbol] = new DependencyNode<ISymbol>(
                symbol,
                $"{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"
            );

            // $"{type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {symbol.Name}"
        }

        return DependencyGraph[symbol];
    }

    private void AddIndexers(ISymbol? leftSymbol, IEnumerable<ArgumentSyntax> arguments)
    {
        DependencyNode<ISymbol>? leftNode = GetDependencyNode(leftSymbol);

        if (leftNode == null)
        {
            return;
        }

        foreach (var argument in arguments)
        {
            var symbol = _semanticModel.GetSymbolInfo(argument.Expression).Symbol;
            leftNode.AddDependency(GetDependencyNode(symbol));
        }
    }

    private void AddAllDependencies(ISymbol? leftSymbol, SyntaxNode? rightNode)
    {
        DependencyNode<ISymbol>? leftNode = GetDependencyNode(leftSymbol);

        if (leftNode == null || rightNode == null)
        {
            return;
        }

        foreach (var symbol in SymbolUtils.GetSymbolsInScope(_semanticModel, rightNode))
        {
            leftNode.AddDependency(GetDependencyNode(symbol));
        }

        foreach (var symbols in _logicDependencies)
        {
            foreach (var symbol in symbols)
            {
                leftNode.AddDependency(GetDependencyNode(symbol));
            }
        }
    }

    private void AddAllDependants(ISymbol? leftSymbol, SyntaxList<MemberDeclarationSyntax> body)
    {
        DependencyNode<ISymbol>? leftNode = GetDependencyNode(leftSymbol);

        if (leftNode == null)
        {
            return;
        }

        foreach (var member in body)
        {
            if (member is MethodDeclarationSyntax method)
            {
                var methodSymbol = _semanticModel.GetDeclaredSymbol(method);
                GetDependencyNode(methodSymbol)?.AddDependency(leftNode);
            }
            else if (member is PropertyDeclarationSyntax property)
            {
                var propertySymbol = _semanticModel.GetDeclaredSymbol(property);
                GetDependencyNode(propertySymbol)?.AddDependency(leftNode);
            }
            else if (member is FieldDeclarationSyntax field)
            {
                foreach (var variable in field.Declaration.Variables)
                {
                    var fieldSymbol = _semanticModel.GetDeclaredSymbol(variable);
                    GetDependencyNode(fieldSymbol)?.AddDependency(leftNode);
                }
            }
        }

        if (leftSymbol is not INamedTypeSymbol)
            return;

        INamedTypeSymbol namedSymbol = (INamedTypeSymbol)leftSymbol;
        INamedTypeSymbol? baseClass = namedSymbol.BaseType;

        if (baseClass != null)
        {
            var baseNode = GetDependencyNode(baseClass);

            leftNode.AddDependency(baseNode);
        }

        foreach (var interfaceType in namedSymbol.Interfaces)
        {
            var interfaceNode = GetDependencyNode(interfaceType);
            leftNode.AddDependency(interfaceNode);
        }
    }

    public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting assignment expression");
            Console.WriteLine(node.ToString());
        }

        ISymbol? leftSymbol = _semanticModel.GetSymbolInfo(node.Left).Symbol;

        if (node.Left is ElementAccessExpressionSyntax elementAccess)
        {
            leftSymbol = _semanticModel.GetSymbolInfo(elementAccess.Expression).Symbol;

            AddIndexers(leftSymbol, elementAccess.ArgumentList.Arguments);
        }

        AddAllDependencies(leftSymbol, node.Right);

        base.VisitAssignmentExpression(node);
    }

    public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting variable declarator");
            Console.WriteLine(node.ToString());
        }

        ISymbol? leftSymbol = _semanticModel.GetDeclaredSymbol(node);

        AddAllDependencies(leftSymbol, node.Initializer?.Value);

        base.VisitVariableDeclarator(node);
    }

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting property declaration");
            Console.WriteLine(node.ToString());
        }

        ISymbol? leftSymbol = _semanticModel.GetDeclaredSymbol(node);

        AddAllDependencies(leftSymbol, node.Initializer?.Value);

        base.VisitPropertyDeclaration(node);
    }

    public override void VisitParameter(ParameterSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting parameter");
            Console.WriteLine(node.ToString());
        }

        ISymbol? leftSymbol = _semanticModel.GetDeclaredSymbol(node);

        AddAllDependencies(leftSymbol, null);

        base.VisitParameter(node);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting method declaration");
            Console.WriteLine(node.ToString());
        }

        ISymbol? leftSymbol = _semanticModel.GetDeclaredSymbol(node);

        AddAllDependencies(leftSymbol, node.Body);

        base.VisitMethodDeclaration(node);
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting If Statement");
            Console.WriteLine(node.ToString());
        }

        List<ISymbol> conditionalSymbols = SymbolUtils
            .GetSymbolsInScope(_semanticModel, node.Condition)
            .ToList();

        _logicDependencies.Enqueue(conditionalSymbols);

        base.VisitIfStatement(node);

        _logicDependencies.Dequeue();
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting For Statement");
            Console.WriteLine(node.ToString());
        }

        List<ISymbol> conditionalSymbols = SymbolUtils
            .GetSymbolsInScope(_semanticModel, node.Condition)
            .ToList();

        _logicDependencies.Enqueue(conditionalSymbols);

        base.VisitForStatement(node);

        _logicDependencies.Dequeue();
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting For Each Statement");
            Console.WriteLine(node.ToString());
        }

        ISymbol? leftSymbol = _semanticModel.GetDeclaredSymbol(node);

        AddAllDependencies(leftSymbol, node.Expression);

        base.VisitForEachStatement(node);
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting While Statement");
            Console.WriteLine(node.ToString());
        }

        List<ISymbol> conditionalSymbols = SymbolUtils
            .GetSymbolsInScope(_semanticModel, node.Condition)
            .ToList();

        _logicDependencies.Enqueue(conditionalSymbols);

        base.VisitWhileStatement(node);

        _logicDependencies.Dequeue();
    }

    public override void VisitDoStatement(DoStatementSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting Do Statement");
            Console.WriteLine(node.ToString());
        }

        List<ISymbol> conditionalSymbols = SymbolUtils
            .GetSymbolsInScope(_semanticModel, node.Condition)
            .ToList();

        _logicDependencies.Enqueue(conditionalSymbols);

        base.VisitDoStatement(node);

        _logicDependencies.Dequeue();
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting Switch Statement");
            Console.WriteLine(node.ToString());
        }

        List<ISymbol> conditionalSymbols = SymbolUtils
            .GetSymbolsInScope(_semanticModel, node.Expression)
            .ToList();

        _logicDependencies.Enqueue(conditionalSymbols);

        base.VisitSwitchStatement(node);

        _logicDependencies.Dequeue();
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting Class Declaration");
            Console.WriteLine(node.ToString());
        }

        ISymbol? leftSymbol = _semanticModel.GetDeclaredSymbol(node);

        AddAllDependants(leftSymbol, node.Members);

        base.VisitClassDeclaration(node);
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting Struct Declaration");
            Console.WriteLine(node.ToString());
        }

        ISymbol? leftSymbol = _semanticModel.GetDeclaredSymbol(node);

        AddAllDependants(leftSymbol, node.Members);

        base.VisitStructDeclaration(node);
    }

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        if (_verbose)
        {
            Console.WriteLine("Visiting Interface Declaration");
            Console.WriteLine(node.ToString());
        }

        ISymbol? leftSymbol = _semanticModel.GetDeclaredSymbol(node);

        AddAllDependants(leftSymbol, node.Members);

        base.VisitInterfaceDeclaration(node);
    }
}