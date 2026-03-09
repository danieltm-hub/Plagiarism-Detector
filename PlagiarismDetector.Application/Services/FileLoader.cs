using Microsoft.Build.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using PlagiarismDetector.Domain.Enums;

namespace PlagiarismDetector.Application.Services;

public static class FileLoader
{
    private static SyntaxTree? LoadFromString(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            Console.WriteLine("Code is empty or null.");
            return null;
        }

        SyntaxTree? syntaxTree = CSharpSyntaxTree.ParseText(code);

        if (syntaxTree == null)
        {
            Console.WriteLine("Failed to parse the code.");
            return null;
        }

        return syntaxTree;
    }

    private static SyntaxTree? LoadFromFile(string filePath)
    {
        var workspace = MSBuildWorkspace.Create();

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return null;
        }

        string code = File.ReadAllText(filePath);

        SyntaxTree? syntaxTree = CSharpSyntaxTree.ParseText(code);

        if (syntaxTree == null)
        {
            Console.WriteLine($"Failed to parse the file: {filePath}");
            return null;
        }

        return syntaxTree;
    }

    private static IEnumerable<SyntaxTree?> LoadFromCSProj(string csprojPath)
    {
        var workspace = MSBuildWorkspace.Create();
        if (!File.Exists(csprojPath))
        {
            Console.WriteLine($"Project file not found: {csprojPath}");
            yield break;
        }
        var project = workspace.OpenProjectAsync(csprojPath).Result;
        foreach (var document in project.Documents)
        {
            var syntaxTree = document.GetSyntaxTreeAsync().Result;
            if (syntaxTree != null)
            {
                yield return syntaxTree;
            }
            else
            {
                Console.WriteLine($"Failed to load document: {document.Name}");
            }
        }
        yield break;
    }

    private static IEnumerable<SyntaxTree?> LoadFromSolution(string solutionPath)
    {
        var workspace = MSBuildWorkspace.Create();

        if (!File.Exists(solutionPath))
        {
            Console.WriteLine($"Solution file not found: {solutionPath}");
            yield break;
        }

        var solution = workspace.OpenSolutionAsync(solutionPath).Result;

        foreach (var projectId in solution.ProjectIds)
        {
            var project = solution.GetProject(projectId);
            if (project != null)
            {
                foreach (var document in project.Documents)
                {
                    var syntaxTree = document.GetSyntaxTreeAsync().Result;
                    if (syntaxTree != null)
                    {
                        yield return syntaxTree;
                    }
                    else
                    {
                        Console.WriteLine($"Failed to load document: {document.Name}");
                    }
                }
            }
        }

        yield break;
    }

    public static IEnumerable<SyntaxTree?> LoadFrom(string path, ImportType type)
    {
        switch (type)
        {
            case ImportType.String:
                yield return LoadFromString(path);
                break;
            case ImportType.File:
                yield return LoadFromFile(path);
                break;
            case ImportType.CSproj:
                foreach (var tree in LoadFromCSProj(path))
                    yield return tree;
                break;
            case ImportType.Solution:
                foreach (var tree in LoadFromSolution(path))
                    yield return tree;
                break;
            default:
                Console.WriteLine("Invalid import type.");
                yield break;
        }
        yield break;
    }
}