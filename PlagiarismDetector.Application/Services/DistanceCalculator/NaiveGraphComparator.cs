using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PlagiarismDetector.Application.Services.DistanceCalculator
{
    public static class NaiveGraphComparator
    {
        private static string GetOrderedChildrenString(
            Dictionary<int, GraphNode> nodesDict,
            int nodeId
        )
        {
            GraphNode node = nodesDict[nodeId];

            // Get all child nodes from the dictionary
            var childNodes = node.Children;

            // Order children by their labels
            var orderedChildren = childNodes.OrderBy(child => child.Name).ToList();

            // Concatenate the ordered children's labels
            var sb = new StringBuilder();

            sb.Append(node.Name);
            foreach (var child in orderedChildren)
            {
                sb.Append(child.Name);
            }

            return sb.ToString();
        }

        public static HashSet<string> GetOrderedNodes(Dictionary<int, GraphNode> nodesDict)
        {
            HashSet<string> nodes = new();
            foreach (var key in nodesDict.Keys)
            {
                nodes.Add(GetOrderedChildrenString(nodesDict, key));
            }

            return nodes;
        }

        private static int Distance(HashSet<string> a, HashSet<string> b)
        {
            var diff = new HashSet<string>(a);
            diff.SymmetricExceptWith(b);

            return diff.Count;
        }

        public static int[,] Distance(List<Dictionary<int, GraphNode>> dictionaries)
        {
            int n = dictionaries.Count;
            int[,] matrix = new int[n, n];

            List<HashSet<string>> elements = new List<HashSet<string>>();
            for (int i = 0; i < n; i++)
            {
                elements.Add(GetOrderedNodes(dictionaries[i]));
            }

            for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                matrix[i, j] = Distance(elements[i], elements[j]);

            return matrix;
        }
    }
}
