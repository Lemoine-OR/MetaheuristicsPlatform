namespace MetaheuristicsPlatform.Graphs;

/// <summary>
/// Mutable builder for an immutable undirected neighborhood graph.
/// Intended for topology construction, not hot per-dimension PSO updates.
/// </summary>
public sealed class UndirectedGraphBuilder
{
    private readonly HashSet<ulong> _edges = [];

    /// <summary>Initializes a graph builder.</summary>
    public UndirectedGraphBuilder(int nodeCount)
    {
        if (nodeCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(nodeCount));
        }

        NodeCount = nodeCount;
    }

    /// <summary>Gets the number of vertices.</summary>
    public int NodeCount { get; }

    /// <summary>Adds an undirected edge. Duplicate edges are ignored.</summary>
    public void AddEdge(int first, int second)
    {
        ValidateNode(first);
        ValidateNode(second);
        _edges.Add(Encode(first, second));
    }

    /// <summary>Adds a self-loop to each node.</summary>
    public void AddSelfLoops()
    {
        for (int i = 0; i < NodeCount; i++)
        {
            AddEdge(i, i);
        }
    }

    /// <summary>Adds all pairwise edges in a clique.</summary>
    public void AddClique(ReadOnlySpan<int> nodes, bool includeSelf = false)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            ValidateNode(nodes[i]);

            if (includeSelf)
            {
                AddEdge(nodes[i], nodes[i]);
            }

            for (int j = i + 1; j < nodes.Length; j++)
            {
                AddEdge(nodes[i], nodes[j]);
            }
        }
    }

    /// <summary>Builds the immutable CSR graph.</summary>
    public NeighborhoodGraph Build()
    {
        List<int>[] adjacency = new List<int>[NodeCount];
        for (int i = 0; i < adjacency.Length; i++)
        {
            adjacency[i] = [];
        }

        int selfLoopCount = 0;

        foreach (ulong encoded in _edges)
        {
            Decode(encoded, out int first, out int second);

            adjacency[first].Add(second);

            if (first == second)
            {
                selfLoopCount++;
            }
            else
            {
                adjacency[second].Add(first);
            }
        }

        int[] offsets = new int[NodeCount + 1];
        int totalArcs = 0;

        for (int i = 0; i < NodeCount; i++)
        {
            adjacency[i].Sort();
            offsets[i] = totalArcs;
            totalArcs += adjacency[i].Count;
        }

        offsets[NodeCount] = totalArcs;

        int[] neighbors = new int[totalArcs];
        int write = 0;

        for (int i = 0; i < NodeCount; i++)
        {
            adjacency[i].CopyTo(neighbors, write);
            write += adjacency[i].Count;
        }

        return new NeighborhoodGraph(
            NodeCount,
            offsets,
            neighbors,
            _edges.Count,
            selfLoopCount);
    }

    internal static ulong Encode(int first, int second)
    {
        uint a = (uint)Math.Min(first, second);
        uint b = (uint)Math.Max(first, second);
        return ((ulong)a << 32) | b;
    }

    internal static void Decode(ulong encoded, out int first, out int second)
    {
        first = (int)(encoded >> 32);
        second = (int)(encoded & uint.MaxValue);
    }

    private void ValidateNode(int node)
    {
        if ((uint)node >= (uint)NodeCount)
        {
            throw new ArgumentOutOfRangeException(nameof(node));
        }
    }
}