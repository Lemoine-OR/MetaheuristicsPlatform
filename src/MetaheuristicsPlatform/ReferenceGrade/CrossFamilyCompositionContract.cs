namespace MetaheuristicsPlatform.ReferenceGrade;

public enum ReferenceCompositionRole
{
    PrimarySearch = 0,
    Construction = 1,
    Improvement = 2,
    Acceptance = 3,
    Selection = 4,
    Repair = 5,
    ExactRepair = 6
}

public sealed record ReferenceCompositionNode(
    string NodeId,
    string StableAlgorithmId,
    ReferenceCompositionRole Role);

public sealed record ReferenceCompositionEdge(
    string FromNodeId,
    string ToNodeId);

public sealed class CrossFamilyCompositionContract
{
    public CrossFamilyCompositionContract(
        IEnumerable<ReferenceCompositionNode> nodes,
        IEnumerable<ReferenceCompositionEdge> edges)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(edges);

        Nodes = nodes.ToArray();
        Edges = edges.ToArray();
        ValidateAcyclic();
    }

    public IReadOnlyList<ReferenceCompositionNode> Nodes { get; }
    public IReadOnlyList<ReferenceCompositionEdge> Edges { get; }

    public void ValidateAcyclic()
    {
        if (Nodes.Count == 0)
            throw new ArgumentException(
                "A reference composition requires at least one node.");

        if (Nodes.Any(x =>
                string.IsNullOrWhiteSpace(x.NodeId) ||
                string.IsNullOrWhiteSpace(x.StableAlgorithmId)))
            throw new ArgumentException(
                "Composition node identities must not be empty.");

        if (Nodes.Select(x => x.NodeId).Distinct(StringComparer.Ordinal).Count() != Nodes.Count)
            throw new ArgumentException(
                "Composition node IDs must be unique.");

        HashSet<string> ids = Nodes.Select(x => x.NodeId).ToHashSet(StringComparer.Ordinal);
        if (Edges.Any(x => !ids.Contains(x.FromNodeId) || !ids.Contains(x.ToNodeId)))
            throw new ArgumentException(
                "Composition edges must reference declared nodes.");

        Dictionary<string, List<string>> graph =
            ids.ToDictionary(x => x, _ => new List<string>(), StringComparer.Ordinal);
        foreach (ReferenceCompositionEdge edge in Edges)
            graph[edge.FromNodeId].Add(edge.ToNodeId);

        HashSet<string> visiting = new(StringComparer.Ordinal);
        HashSet<string> visited = new(StringComparer.Ordinal);

        foreach (string id in ids)
            Visit(id, graph, visiting, visited);
    }

    private static void Visit(
        string id,
        IReadOnlyDictionary<string, List<string>> graph,
        HashSet<string> visiting,
        HashSet<string> visited)
    {
        if (visited.Contains(id))
            return;
        if (!visiting.Add(id))
            throw new ArgumentException(
                "Cross-family composition must be acyclic.");

        foreach (string next in graph[id])
            Visit(next, graph, visiting, visited);

        visiting.Remove(id);
        visited.Add(id);
    }
}
