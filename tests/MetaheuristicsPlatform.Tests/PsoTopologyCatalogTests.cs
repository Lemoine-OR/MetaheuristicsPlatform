using MetaheuristicsPlatform.Algorithms.PSO.Topologies;

namespace MetaheuristicsPlatform.Tests;

public sealed class PsoTopologyCatalogTests
{
    [Fact]
    public void DefaultCatalog_UsesUniqueIds()
    {
        IReadOnlyList<IPsoTopology> topologies =
            PsoTopologyCatalog.CreateDefaults();

        string[] ids = topologies
            .Select(static topology => topology.Descriptor.Id)
            .ToArray();

        Assert.Equal(ids.Length, ids.Distinct().Count());
    }

    [Fact]
    public void EveryLiteratureBackedDefault_HasReference()
    {
        foreach (IPsoTopology topology in
                 PsoTopologyCatalog.CreateDefaults())
        {
            if (topology is CustomGraphTopology)
            {
                continue;
            }

            Assert.NotEmpty(topology.Descriptor.References);
        }
    }
}