using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Static Watts-Strogatz-style small-world communication graph.
/// </summary>
/// <remarks>
/// Gong and Zhang (2013) go further and adapt small-world topology parameters during
/// optimization. That exact adaptive method will be implemented separately once the
/// PSO runtime exposes stagnation and convergence state.
/// </remarks>
public sealed class WattsStrogatzSmallWorldTopology : IPsoTopology
{
    public WattsStrogatzSmallWorldTopology(
        int neighborhoodSize = 4,
        double rewiringProbability = 0.1,
        bool includeSelf = true)
    {
        if (neighborhoodSize <= 0 || (neighborhoodSize & 1) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(neighborhoodSize),
                "Neighborhood size must be a positive even integer.");
        }

        if (rewiringProbability is < 0.0 or > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(rewiringProbability));
        }

        NeighborhoodSize = neighborhoodSize;
        RewiringProbability = rewiringProbability;
        IncludeSelf = includeSelf;
    }

    public int NeighborhoodSize { get; }
    public double RewiringProbability { get; }
    public bool IncludeSelf { get; }

    public PsoTopologyDescriptor Descriptor { get; } = new()
    {
        Id = "small-world-watts-strogatz",
        Name = "Small World (Watts-Strogatz style)",
        Dynamics = PsoTopologyDynamics.RandomStatic,
        IsPublishedExactVariant = false,
        Notes = "Static reusable small-world graph. The adaptive SWPSO of Gong & Zhang (2013) is a distinct planned exact variant.",
        References = new[] { PsoTopologyReferences.GongZhang2013 }
    };

    public NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(random);

        int n = context.SwarmSize;

        if (NeighborhoodSize >= n)
        {
            throw new ArgumentException(
                "Neighborhood size must be smaller than swarm size.");
        }

        HashSet<ulong> edges = [];
        int half = NeighborhoodSize / 2;

        for (int i = 0; i < n; i++)
        {
            for (int d = 1; d <= half; d++)
            {
                int target = (i + d) % n;
                edges.Add(UndirectedGraphBuilder.Encode(i, target));
            }
        }

        for (int i = 0; i < n; i++)
        {
            for (int d = 1; d <= half; d++)
            {
                int originalTarget = (i + d) % n;
                ulong original = UndirectedGraphBuilder.Encode(i, originalTarget);

                if (!edges.Contains(original) ||
                    random.NextDouble() >= RewiringProbability)
                {
                    continue;
                }

                List<int> candidates = [];
                for (int candidate = 0; candidate < n; candidate++)
                {
                    if (candidate == i)
                    {
                        continue;
                    }

                    ulong key = UndirectedGraphBuilder.Encode(i, candidate);
                    if (!edges.Contains(key))
                    {
                        candidates.Add(candidate);
                    }
                }

                if (candidates.Count == 0)
                {
                    continue;
                }

                int replacement =
                    candidates[random.NextInt32(candidates.Count)];

                edges.Remove(original);
                edges.Add(UndirectedGraphBuilder.Encode(i, replacement));
            }
        }

        var builder = new UndirectedGraphBuilder(n);
        foreach (ulong encoded in edges)
        {
            UndirectedGraphBuilder.Decode(
                encoded,
                out int first,
                out int second);

            builder.AddEdge(first, second);
        }

        PsoTopologyUtilities.AddOptionalSelfLoops(builder, IncludeSelf);
        return builder.Build();
    }
}