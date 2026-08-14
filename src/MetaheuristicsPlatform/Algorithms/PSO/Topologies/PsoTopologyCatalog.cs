namespace MetaheuristicsPlatform.Algorithms.PSO.Topologies;

/// <summary>
/// Built-in topology factory catalog.
/// Parameterized methods expose conservative representative defaults.
/// </summary>
public static class PsoTopologyCatalog
{
    /// <summary>Creates representative built-in topology instances.</summary>
    public static IReadOnlyList<IPsoTopology> CreateDefaults() =>
        new IPsoTopology[]
        {
            new FullyConnectedTopology(),
            new RingTopology(),
            new HubAndSpokeTopology(),
            new ToroidalVonNeumannTopology(),
            new RandomConnectedTopology(),
            new ClusteredTopology(clusterCount: 4),
            new WattsStrogatzSmallWorldTopology(),
            new BarabasiAlbertScaleFreeTopology()
        };
}