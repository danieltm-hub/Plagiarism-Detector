using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PlagiarismDetector.Application.Services.DependencyWalker;
public class DependencyNode<T>
{
    public T UniqueID { get; }
    public string Label { get; }
    public HashSet<DependencyNode<T>> DependantNodes { get; } =
        new HashSet<DependencyNode<T>>();

    public DependencyNode<T>? Parent { get; }

    public DependencyNode(T uniqueID, string label, DependencyNode<T>? parent = null)
    {
        Parent = parent;
        UniqueID = uniqueID;
        Label = label;
    }

    public void AddDependency(DependencyNode<T>? node)
    {
        if (node == null || DependantNodes.Contains(node))
            return;

        DependantNodes.Add(node);
    }

    public override bool Equals(object? obj)
    {
        if (obj is DependencyNode<T> other)
            return Label == other.Label
                && EqualityComparer<T>.Default.Equals(UniqueID, other.UniqueID);
        return false;
    }

    public override int GetHashCode() => (UniqueID != null) ? UniqueID.GetHashCode() : -1;

    public override string ToString() => Label;
}