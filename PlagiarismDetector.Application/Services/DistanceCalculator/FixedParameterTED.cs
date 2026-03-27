using System.Security.Cryptography.Pkcs;
using PlagiarismDetector.Application.DTOs;

namespace PlagiarismDetector.Application.Services.DistanceCalculator;

public enum EditOpType
{
    Insert,
    Delete,
    Relabel,
    InsertSubtree,
    DeleteSubtree,
}

public class EditOp
{
    public EditOpType Type { get; }
    public TreeNode? sourceNode;
    public TreeNode? targetNode;

    public EditOp(EditOpType type, TreeNode? sourceNode = null, TreeNode? targetNode = null)
    {
        Type = type;
        this.sourceNode = sourceNode;
        this.targetNode = targetNode;
    }
}

public class EditResult
{
    public int Cost;
    public List<EditOp> Operations = new();

    public EditResult(int cost)
    {
        Cost = cost;
    }
}

public class FixedParameterTED
{
    private struct Key
    {
        public TreeNode A;
        public TreeNode B;
        public int k;

        public Key(TreeNode a, TreeNode b, int k)
        {
            A = a;
            B = b;
            this.k = k;
        }
    }

    private readonly Func<TreeNode, TreeNode, bool> _isEqual;
    private readonly Dictionary<Key, EditResult> _memo = new();
    private readonly Dictionary<TreeNode, int> _t1SubtreeSize = new();
    private readonly Dictionary<TreeNode, int> _t2SubtreeSize = new();
    private EditResult? _result;
    private bool _verbose;
    public EditResult Result =>
        _result ?? throw new InvalidOperationException("Result not computed yet.");

    public FixedParameterTED(
        TreeNode t1,
        TreeNode t2,
        int k,
        bool verbose = false,
        Func<TreeNode, TreeNode, bool>? isEqual = null
    )
    {
        _verbose = verbose;

        ComputeSubtreeSizes(t1, _t1SubtreeSize);
        ComputeSubtreeSizes(t2, _t2SubtreeSize);

        if (isEqual == null)
            _isEqual = (a, b) => a.Name == b.Name;
        else
            _isEqual = isEqual;

        _result = TED(t1, t2, k);
    }

    private int ComputeSubtreeSizes(TreeNode node, Dictionary<TreeNode, int> subtreeSize)
    {
        int size = 1;
        foreach (var child in node.Children)
        {
            size += ComputeSubtreeSizes(child, subtreeSize);
        }
        subtreeSize[node] = size;
        return size;
    }

    private EditResult TED(TreeNode a, TreeNode b, int k)
    {
        var key = new Key(a, b, k);

        if (_memo.TryGetValue(key, out var cachedResult))
            return cachedResult;

        EditResult result = new EditResult(_t1SubtreeSize[a] + _t2SubtreeSize[b]);

        if (k > 0)
        {
            EditResult rename = MatchForest(a.Children, b.Children, _isEqual(a, b) ? k : k - 1);
            if (!_isEqual(a, b))
            {
                rename.Operations.Add(new EditOp(EditOpType.Relabel, a, b));
                rename.Cost += 1;
            }

            EditResult insert = MatchForest(new List<TreeNode> { a }, b.Children, k - 1);
            insert.Operations.Add(new EditOp(EditOpType.Insert, b));
            insert.Cost += 1;

            EditResult delete = MatchForest(a.Children, new List<TreeNode> { b }, k - 1);
            delete.Operations.Add(new EditOp(EditOpType.Delete, a));
            delete.Cost += 1;

            if (result.Cost > rename.Cost)
                result = rename;
            if (result.Cost > insert.Cost)
                result = insert;
            if (result.Cost > delete.Cost)
                result = delete;

            if (_verbose)
            {
                Console.WriteLine($"Key ({key.A.Id}, {key.B.Id}, {key.k})");
                Console.WriteLine(
                    $"Relabel cost: {rename.Cost}, Insert cost {insert.Cost}, Delete cost {delete.Cost}"
                );
            }
        }

        _memo[key] = result;

        return result;
    }

    private EditResult MatchForest(List<TreeNode> A, List<TreeNode> B, int k)
    {
        int n = A.Count;
        int m = B.Count;
        int size = n + m;

        var costMat = new int[size, size];

        for (int i = 0; i < n; i++)
        for (int j = 0; j < m; j++)
            costMat[i, j] = TED(A[i], B[j], k).Cost;

        for (int i = 0; i < n; i++)
        for (int j = m; j < size; j++)
            costMat[i, j] = _t1SubtreeSize[A[i]];

        for (int i = n; i < size; i++)
        for (int j = 0; j < m; j++)
            costMat[i, j] = _t2SubtreeSize[B[j]];

        HungarianResult solution = Hungarian.Solve(costMat);

        var editResult = new EditResult(solution.TotalCost);

        for (int i = 0; i < size; i++)
        {
            int j = solution.RowToCol[i];

            if (i < n && j < m)
            {
                editResult.Operations.AddRange(_memo[new Key(A[i], B[j], k)].Operations);
            }
            else if (i < n)
            {
                editResult.Operations.Add(new EditOp(EditOpType.DeleteSubtree, A[i]));
            }
            else if (j < m)
            {
                editResult.Operations.Add(new EditOp(EditOpType.InsertSubtree, B[j]));
            }
        }

        return editResult;
    }
}
