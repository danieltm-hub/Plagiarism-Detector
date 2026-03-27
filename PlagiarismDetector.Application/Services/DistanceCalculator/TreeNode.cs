namespace PlagiarismDetector.Application.Services.DistanceCalculator;

using PlagiarismDetector.Application.DTOs;
using System.Collections.Generic;
using System.Linq;

public class TreeNode
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<TreeNode> Children { get; set; } = new();

    public TreeNode(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static TreeNode? FromEssentials(List<NodeEssentials> essentials)
    {
        if (essentials == null || essentials.Count == 0) return null;

        var nodes = essentials.ToDictionary(e => e.Id, e => new TreeNode(e.Id, e.Name));
        TreeNode? root = null;

        foreach (var e in essentials)
        {
            if (e.ParentId == -1)
            {
                root = nodes[e.Id];
            }
            else if (nodes.TryGetValue(e.ParentId, out var parent))
            {
                parent.Children.Add(nodes[e.Id]);
            }
        }

        return root;
    }
}
