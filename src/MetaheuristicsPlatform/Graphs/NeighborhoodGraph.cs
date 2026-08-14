namespace MetaheuristicsPlatform.Graphs;

/// <summary>
/// Immutable compact sparse row (CSR) neighborhood graph.
/// Neighbor lists are sorted and contain no duplicate indices.
/// </summary>
public sealed class NeighborhoodGraph
{
    private readonly int[] _offsets;
    private readonly int[] _neighbors;

    internal NeighborhoodGraph(
        int nodeCount,
        int[] offsets,
        int[] neighbors,
        int edgeCount,
        int selfLoopCount)
    {
        NodeCount = nodeCount;
        _offsets = offsets;
        _neighbors = neighbors;
        EdgeCount = edgeCount;
        SelfLoopCount = selfLoopCount;
    }

    /// <summary>Gets the number of vertices.</summary>
    public int NodeCount { get; }

    /// <summary>
    /// Gets the number of undirected edges, including self-loops as one edge each.
    /// </summary>
    public int EdgeCount { get; }

    /// <summary>Gets the number of self-loop edges.</summary>
    public int SelfLoopCount { get; }

    /// <summary>Gets the number of stored adjacency entries.</summary>
    public int ArcCount => _neighbors.Length;

    /// <summary>Gets the sorted neighborhood of one vertex.</summary>
    public ReadOnlySpan<int> GetNeighbors(int node)
    {
        ValidateNode(node);
        int start = _offsets[node];
        int length = _offsets[node + 1] - start;
        return _neighbors.AsSpan(start, length);
    }

    /// <summary>Gets the stored neighbor count, including self when present.</summary>
    public int GetNeighborCount(int node)
    {
        ValidateNode(node);
        return _offsets[node + 1] - _offsets[node];
    }

    /// <summary>Returns whether an undirected edge exists.</summary>
    public bool ContainsEdge(int from, int to)
    {
        ValidateNode(from);
        ValidateNode(to);

        ReadOnlySpan<int> neighbors = GetNeighbors(from);
        int low = 0;
        int high = neighbors.Length - 1;

        while (low <= high)
        {
            int mid = low + ((high - low) >> 1);
            int value = neighbors[mid];

            if (value == to)
            {
                return true;
            }

            if (value < to)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return false;
    }

    private void ValidateNode(int node)
    {
        if ((uint)node >= (uint)NodeCount)
        {
            throw new ArgumentOutOfRangeException(nameof(node));
        }
    }
}