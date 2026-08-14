using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Tests;

public sealed class ClassificationTests
{
    [Fact]
    public void Descriptor_AllowsMultidimensionalClassification()
    {
        var descriptor = new MetaheuristicDescriptor
        {
            Id = "pso",
            Name = "Particle Swarm Optimization",
            Acronym = "PSO",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.SwarmIntelligence,
            Mechanisms = MetaheuristicMechanism.Swarm | MetaheuristicMechanism.Adaptive,
            SearchSpaces = SearchSpaceKind.Continuous | SearchSpaceKind.Binary,
            IsStochastic = true
        };

        Assert.True(descriptor.Mechanisms.HasFlag(MetaheuristicMechanism.Swarm));
        Assert.True(descriptor.SearchSpaces.HasFlag(SearchSpaceKind.Continuous));
        Assert.Equal(MetaheuristicSolutionModel.Population, descriptor.SolutionModel);
    }
}