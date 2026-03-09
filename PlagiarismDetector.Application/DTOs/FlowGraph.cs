namespace PlagiarismDetector.Application.DTOs;

public record NodeEssentials(int Id, string Name, int ParentId);
public record GraphEssentials(int Id, string Name, List<int> Children);

public record FlowGraphs(
    List<NodeEssentials> Trees,
    List<GraphEssentials> Graphs
);

public struct FlowGraphResponse
{
    public string filename;
    public FlowGraphs flowGraphs;
}