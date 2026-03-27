namespace PlagiarismDetector.Application.Services.DistanceCalculator;

using PlagiarismDetector.Application.DTOs;
using System.Collections.Generic;
using System.Linq;

public class GraphNode
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<GraphNode> Children { get; set; } = new();

    public GraphNode(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Dictionary<int, GraphNode> FromEssentials(List<GraphEssentials> essentials)
    {
        if (essentials == null) return new Dictionary<int, GraphNode>();

        var nodes = essentials.ToDictionary(e => e.Id, e => new GraphNode(e.Id, e.Name));

        foreach (var e in essentials)
        {
            var node = nodes[e.Id];
            foreach (var childId in e.Children)
            {
                if (nodes.TryGetValue(childId, out var child))
                {
                    node.Children.Add(child);
                }
            }
        }

        return nodes;
    }
}
