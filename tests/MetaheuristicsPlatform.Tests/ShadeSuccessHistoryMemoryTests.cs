using MetaheuristicsPlatform.Algorithms.DE.Adaptive;

namespace MetaheuristicsPlatform.Tests;

public sealed class ShadeSuccessHistoryMemoryTests
{
    [Fact]
    public void MemoryInitializesEveryEntryToHalf()
    {
        var memory =
            new ShadeSuccessHistoryMemory(
                capacity: 100);

        Assert.All(
            memory.DifferentialWeights.ToArray(),
            value =>
                Assert.Equal(
                    0.5,
                    value));

        Assert.All(
            memory.CrossoverProbabilities.ToArray(),
            value =>
                Assert.Equal(
                    0.5,
                    value));

        Assert.Equal(
            0,
            memory.Position);
    }

    [Fact]
    public void UpdateWritesCurrentSlotAndWrapsCircularly()
    {
        var memory =
            new ShadeSuccessHistoryMemory(
                capacity: 2);

        memory.Update(
            weightedArithmeticMeanCr: 0.3,
            weightedLehmerMeanF: 0.7);

        Assert.Equal(
            0.3,
            memory.GetCrossoverProbability(0));

        Assert.Equal(
            0.7,
            memory.GetDifferentialWeight(0));

        Assert.Equal(
            1,
            memory.Position);

        memory.Update(
            weightedArithmeticMeanCr: 0.4,
            weightedLehmerMeanF: 0.8);

        Assert.Equal(
            0,
            memory.Position);

        memory.Update(
            weightedArithmeticMeanCr: 0.6,
            weightedLehmerMeanF: 0.9);

        Assert.Equal(
            0.6,
            memory.GetCrossoverProbability(0));

        Assert.Equal(
            0.9,
            memory.GetDifferentialWeight(0));
    }
}