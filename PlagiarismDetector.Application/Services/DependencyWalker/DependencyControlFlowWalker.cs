using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlagiarismDetector.Application.Services.DependencyWalker;
public class DependencyControlFlowWalker : CSharpSyntaxWalker
{
    private readonly SemanticModel _semanticModel;

    private DependencyNode<string> _currentNode;
    private DependencyNode<string> _parentNode;

    //Last control node
    public Dictionary<string, DependencyNode<string>> DependencyGraph { get; } =
        new Dictionary<string, DependencyNode<string>>();

    public DependencyControlFlowWalker(SemanticModel semanticModel)
        : base(SyntaxWalkerDepth.StructuredTrivia)
    {
        _semanticModel = semanticModel;
        _parentNode = new DependencyNode<string>("START", "START");
        _currentNode = _parentNode;

        DependencyGraph["START"] = _parentNode;
    }

    public static string GenerateUniqueHash(SyntaxNode node)
    {
        var location = node.GetLocation().ToString();
        var span = node.Span.ToString();
        var leadingTrivia = node.GetLeadingTrivia().ToString();
        var trailingTrivia = node.GetTrailingTrivia().ToString();

        return $"{location}_{span}_{leadingTrivia}_{trailingTrivia}";
    }

    public void CreateNewNode(SyntaxNode node, string label)
    {
        _currentNode = new DependencyNode<string>(GenerateUniqueHash(node), label, _parentNode);
        _parentNode.DependantNodes.Add(_currentNode);
    }

    public void CreateNewDependence(SyntaxNode node, string label)
    {
        CreateNewNode(node, label);

        DependencyGraph[_currentNode.UniqueID] = _currentNode;
    }

    public override void Visit(SyntaxNode? node)
    {
        if (node == null)
        {
            return;
        }

        if (node.Parent != null)
        {
            _parentNode = DependencyGraph[GenerateUniqueHash(node.Parent)];
        }

        _currentNode = new DependencyNode<string>(
            GenerateUniqueHash(node),
            "RANDOM_PLACEHOLDER",
            _parentNode
        );

        DependencyGraph[_currentNode.UniqueID] = _parentNode;

        base.Visit(node);
    }

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        CreateNewDependence(node, "NAMESPACE_NODE");

        base.VisitNamespaceDeclaration(node);
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        CreateNewDependence(node, "CLASS_NODE");

        base.VisitClassDeclaration(node);
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        CreateNewDependence(node, "STRUCT_NODE");

        base.VisitStructDeclaration(node);
    }

    public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        CreateNewDependence(node, "INTERFACE_NODE");

        base.VisitInterfaceDeclaration(node);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        CreateNewDependence(node, "METHOD_NODE");

        base.VisitMethodDeclaration(node);
    }

    public override void VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        CreateNewDependence(node, "LOCAL_FUNCTION_NODE");

        base.VisitLocalFunctionStatement(node);
    }

    public override void VisitIfStatement(IfStatementSyntax node)
    {
        CreateNewDependence(node, "CONDITIONAL_NODE");

        base.VisitIfStatement(node);
    }

    public override void VisitElseClause(ElseClauseSyntax node)
    {
        CreateNewDependence(node, "CONDITIONAL_NODE");

        base.VisitElseClause(node);
    }

    public override void VisitSwitchStatement(SwitchStatementSyntax node)
    {
        CreateNewDependence(node, "CONDITIONAL_NODE");

        base.VisitSwitchStatement(node);
    }

    public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        CreateNewDependence(node, "CONDITIONAL_NODE");

        base.VisitConditionalExpression(node);
    }

    public override void VisitForStatement(ForStatementSyntax node)
    {
        CreateNewDependence(node, "LOOP_NODE");

        base.VisitForStatement(node);
    }

    public override void VisitForEachStatement(ForEachStatementSyntax node)
    {
        CreateNewDependence(node, "LOOP_NODE");

        base.VisitForEachStatement(node);
    }

    public override void VisitWhileStatement(WhileStatementSyntax node)
    {
        CreateNewDependence(node, "LOOP_NODE");

        base.VisitWhileStatement(node);
    }

    public override void VisitDoStatement(DoStatementSyntax node)
    {
        CreateNewDependence(node, "LOOP_NODE");

        base.VisitDoStatement(node);
    }

    public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
    {
        CreateNewDependence(node, "ANONYMOUS_METHOD_NODE");

        base.VisitAnonymousMethodExpression(node);
    }

    public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
    {
        CreateNewDependence(node, "LAMBDA_NODE");

        base.VisitSimpleLambdaExpression(node);
    }

    public override void VisitParenthesizedLambdaExpression(
        ParenthesizedLambdaExpressionSyntax node
    )
    {
        CreateNewDependence(node, "LAMBDA_NODE");

        base.VisitParenthesizedLambdaExpression(node);
    }
}
