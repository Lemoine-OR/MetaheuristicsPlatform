using MetaheuristicsPlatform.Algorithms.DE;
using MetaheuristicsPlatform.Classification;

namespace MetaheuristicsPlatform.Tests;

public sealed class DifferentialEvolutionDescriptorTests
{
    [Fact]
    public void Descriptor_ClassifiesDeAsEvolutionaryPopulationMethod()
    {
        var descriptor =
            new DifferentialEvolutionOptimizer()
                .Descriptor;

        Assert.Equal(
            MetaheuristicSolutionModel.Population,
            descriptor.SolutionModel);

        Assert.True(
            descriptor.Families.HasFlag(
                MetaheuristicFamily.Evolutionary));

        Assert.True(
            descriptor.Mechanisms.HasFlag(
                MetaheuristicMechanism.EvolutionaryOperators));

        Assert.True(
            descriptor.SearchSpaces.HasFlag(
                SearchSpaceKind.Continuous));
    }

    [Fact]
    public void Descriptor_ContainsStornPriceReference()
    {
        var descriptor =
            new DifferentialEvolutionOptimizer()
                .Descriptor;

        Assert.Contains(
            descriptor.References,
            reference =>
                reference.Doi ==
                "10.1023/A:1008202821328");
    }
}