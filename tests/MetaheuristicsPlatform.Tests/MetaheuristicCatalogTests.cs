using MetaheuristicsPlatform.Catalog;

namespace MetaheuristicsPlatform.Tests;

public sealed class MetaheuristicCatalogTests
{
    [Fact]
    public void StableIdsAreUniqueAndNonEmpty()
    {
        string[] ids =
            MetaheuristicCatalog.All
                .Select(static entry => entry.Id)
                .ToArray();

        Assert.All(
            ids,
            static id =>
                Assert.False(
                    string.IsNullOrWhiteSpace(id)));

        Assert.Equal(
            ids.Length,
            ids.Distinct(
                    StringComparer.Ordinal)
                .Count());
    }

    [Fact]
    public void CurrentPublicAlgorithmsAreCatalogued()
    {
        Assert.Contains(
            MetaheuristicCatalog.All,
            static entry =>
                entry.Id ==
                MetaheuristicAlgorithmIds.ParticleSwarm);

        Assert.Contains(
            MetaheuristicCatalog.All,
            static entry =>
                entry.Id ==
                MetaheuristicAlgorithmIds.DifferentialEvolution);

        Assert.Contains(
            MetaheuristicCatalog.All,
            static entry =>
                entry.Id ==
                MetaheuristicAlgorithmIds.SimulatedAnnealing);
    }

    [Fact]
    public void CompositionAlgorithmCanUseStableFactoryId()
    {
        MetaheuristicFactory.Register(
            MetaheuristicAlgorithmIds.SimulatedAnnealing,
            static () =>
                new DummyAlgorithm(),
            replace: true);

        DummyAlgorithm algorithm =
            MetaheuristicFactory.Create<DummyAlgorithm>(
                MetaheuristicAlgorithmIds.SimulatedAnnealing);

        Assert.NotNull(
            algorithm);
    }

    private sealed class DummyAlgorithm
    {
    }
}
