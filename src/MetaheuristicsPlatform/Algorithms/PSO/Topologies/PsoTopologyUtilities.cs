using MetaheuristicsPlatform.Graphs;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

internal static class PsoTopologyUtilities
{
    internal static void AddOptionalSelfLoops(
        UndirectedGraphBuilder builder,
        bool includeSelf)
    {
        if (includeSelf)
        {
            builder.AddSelfLoops();
        }
    }

    internal static void ValidateSwarm(
        PsoTopologyContext context,
        IRandomSource? random = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (random is null)
        {
            return;
        }
    }
}