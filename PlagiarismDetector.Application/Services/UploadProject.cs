using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using PlagiarismDetector.Domain.Enums;
using PlagiarismDetector.Application.DTOs;
using PlagiarismDetector.Application.Services.DependencyWalker;
using System.Collections;
using System.IO;

namespace PlagiarismDetector.Application.Services;

public class UploadProjectsFolder
{
    public static IEnumerable<FlowGraphResponse> ExtractFlowGraphs(string path, ImportType importType)
    {
        foreach (var file in GetTargetFiles(path, importType))
        {
            var nodeEssentials = GetControlTree(file.path, importType);
            var graphEssentials = GetDataGraph(file.path, importType);

            yield return new FlowGraphResponse
            {
                filename = file.filename,
                flowGraphs = new FlowGraphs(nodeEssentials, graphEssentials)
            };
        } 
    }

    private static List<NodeEssentials> GetControlTree(
        string path,
        ImportType importType
    )
    {
        DependencyNode<string> flowDependencies = new DependencyNode<string>("START", "START");

        foreach (var syntaxTree in FileLoader.LoadFrom(path, importType))
        {
            CompilationUnitSyntax? root = syntaxTree?.GetCompilationUnitRoot();

            if (root != null && syntaxTree != null)
            {
                var flowWalker = new DependencyControlFlowWalker(
                    CSharpCompilation
                        .Create("DependencyWalker")
                        .AddSyntaxTrees(syntaxTree)
                        .GetSemanticModel(syntaxTree)
                );

                flowWalker.Visit(root);

                var flowRoot = flowWalker.DependencyGraph["START"];

                if (flowRoot != null && flowRoot.DependantNodes.Count > 0)
                    foreach (var child in flowRoot.DependantNodes)
                        flowDependencies.AddDependency(child);
            }
            else
            {
                Console.WriteLine($"Unable to create flow for file: {path}");
            }
        }

        return FromDependencyNode(flowDependencies, new Dictionary<string, int>(), -1).ToList();
    }

    private static IEnumerable<NodeEssentials> FromDependencyNode(
        DependencyNode<string> node,
        Dictionary<string, int> visited,
        int fatherId
    )
    {
        int id = visited.Count;
        visited[node.UniqueID] = id;

        var nodeEssentials = new NodeEssentials(id, node.Label, fatherId);

        yield return nodeEssentials;

        foreach (var child in node.DependantNodes)
        {
            if (visited.ContainsKey(child.UniqueID))
                continue;

            foreach (var nestedNode in FromDependencyNode(child, visited, id))
                yield return nestedNode;
        }
    }

    private static List<GraphEssentials> GetDataGraph(
        string path,
        ImportType importType
    )
    {
        Dictionary<ISymbol, DependencyNode<ISymbol>> dataDict = new(SymbolEqualityComparer.Default);

        foreach (var syntaxTree in FileLoader.LoadFrom(path, importType))
        {
            CompilationUnitSyntax? root = syntaxTree?.GetCompilationUnitRoot();

            if (root != null && syntaxTree != null)
            {
                var dataWalker = new DependencyDataFlowWalker(
                    CSharpCompilation
                        .Create("DependencyWalker")
                        .AddSyntaxTrees(syntaxTree)
                        .GetSemanticModel(syntaxTree),
                    false
                );

                dataWalker.Visit(root);

                foreach (var key in dataWalker.DependencyGraph.Keys)
                {
                    if (dataDict.ContainsKey(key))
                        continue;

                    var node = dataWalker.DependencyGraph[key];

                    if (dataDict.ContainsKey(node.UniqueID))
                        foreach (var dependency in node.DependantNodes)
                            dataDict[node.UniqueID].AddDependency(dependency);

                    dataDict[key] = node;
                }
            }
            else
            {
                Console.WriteLine("Failed to parse the code.");
            }
        }

        return FromDependencyGraph(dataDict).ToList();
    }

    private static IEnumerable<GraphEssentials> FromDependencyGraph(Dictionary<ISymbol, DependencyNode<ISymbol>> data)
    {
        Dictionary<ISymbol, int> id = new(SymbolEqualityComparer.Default);

        foreach (var symbol in data.Keys)
            id[symbol] = id.Count;

        foreach (var node in data.Values)
        {
            var children = node.DependantNodes
                .Where(child => id.ContainsKey(child.UniqueID))
                .Select(child => id[child.UniqueID])
                .ToList();

            yield return new GraphEssentials(id[node.UniqueID], node.Label, children);
        }
    }

    private static IEnumerable<(string path, string filename)> GetTargetFiles(string path, ImportType importType)
    {
        string extension = "";

        switch (importType)
        {
            case ImportType.CSproj:
                extension = ".csproj";
                break;
            case ImportType.Solution:
                extension = ".sln";
                break;
            default:
                throw new NotImplementedException("La lectura de archivos .cs individuales no está implementada");
        }

        if (!System.IO.Directory.Exists(path))
        {
            Console.WriteLine($"The path '{path}' does not exist.");
            yield break;
        }

        string searchPattern = $"*{extension}";

        var files = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories);

        foreach (var filePath in files)
        {
            string? directoryPath = Path.GetDirectoryName(filePath);

            string innermostFolder = directoryPath != null ? Path.GetFileName(directoryPath) : string.Empty;

            yield return (filePath, innermostFolder);
        }
    }
}