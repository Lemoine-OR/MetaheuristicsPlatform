using MetaheuristicsPlatform.Algorithms.DE;

namespace MetaheuristicsPlatform.Tests;

public sealed class DifferentialEvolutionParametersTests
{
    [Theory]
    [InlineData(DeMutationStrategy.Rand1, 4)]
    [InlineData(DeMutationStrategy.Best1, 4)]
    [InlineData(DeMutationStrategy.CurrentToBest1, 4)]
    [InlineData(DeMutationStrategy.Rand2, 6)]
    public void MutationStrategy_HasExpectedMinimumPopulation(
        DeMutationStrategy strategy,
        int expected)
    {
        Assert.Equal(
            expected,
            DifferentialEvolutionParameters
                .MinimumPopulationSizeFor(
                    strategy));
    }

    [Fact]
    public void Rand2RejectsPopulationBelowSix()
    {
        var parameters =
            new DifferentialEvolutionParameters
            {
                PopulationSize = 5,
                MutationStrategy =
                    DeMutationStrategy.Rand2
            };

        Assert.Throws<ArgumentOutOfRangeException>(
            parameters.Validate);
    }
}