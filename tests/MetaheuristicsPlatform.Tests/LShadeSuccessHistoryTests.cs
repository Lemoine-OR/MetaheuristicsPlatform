using MetaheuristicsPlatform.Algorithms.DE.Adaptive;

namespace MetaheuristicsPlatform.Tests;

public sealed class LShadeSuccessHistoryTests
{
    [Fact]
    public void MemoryInitializesToHalf()
    {
        var memory =
            new LShadeSuccessHistoryMemory(
                capacity: 6);

        for (int i = 0;
             i < 6;
             i++)
        {
            Assert.Equal(
                0.5,
                memory.GetDifferentialWeight(i));

            Assert.Equal(
                0.5,
                memory.GetCrossoverProbability(i));

            Assert.False(
                memory.IsCrossoverTerminal(i));
        }
    }

    [Fact]
    public void TerminalCrossoverStaysTerminalWhenItsSlotReturns()
    {
        var memory =
            new LShadeSuccessHistoryMemory(
                capacity: 1);

        memory.UpdateTerminalCrossover();

        Assert.True(
            memory.IsCrossoverTerminal(0));

        Assert.Equal(
            0.0,
            memory.GetCrossoverProbability(0));

        memory.Update(
            weightedLehmerMeanCr: 0.8,
            weightedLehmerMeanF: 0.7);

        Assert.True(
            memory.IsCrossoverTerminal(0));

        Assert.Equal(
            0.0,
            memory.GetCrossoverProbability(0));
    }

    [Fact]
    public void SuccessfulCrAndFUseWeightedLehmerMeans()
    {
        var buffers =
            new DeParameterBuffers(2);

        var policy =
            new LShadeParameterAdaptationPolicy(
                memorySize: 6);

        policy.Initialize(
            buffers,
            activePopulationSize: 2);

        var first =
            new DeControlParameters(
                DifferentialWeight: 0.2,
                CrossoverProbability: 0.3);

        var second =
            new DeControlParameters(
                DifferentialWeight: 0.8,
                CrossoverProbability: 0.9);

        buffers.SetTrial(
            0,
            in first);

        buffers.SetTrial(
            1,
            in second);

        var context =
            new DeGenerationAdaptationContext(
                Generation: 1,
                ActivePopulationSize: 2,
                FunctionEvaluations: 4,
                MaximumFunctionEvaluations: 100);

        DeSelectionFeedback[] feedback =
            new[]
            {
                new DeSelectionFeedback(
                    0,
                    Accepted: true,
                    ParentFitness: 10.0,
                    TrialFitness: 9.0,
                    Improvement: 1.0),
                new DeSelectionFeedback(
                    1,
                    Accepted: true,
                    ParentFitness: 10.0,
                    TrialFitness: 7.0,
                    Improvement: 3.0)
            };

        policy.CompleteGeneration(
            in context,
            buffers,
            feedback);

        double expectedCr =
            (0.25 * 0.3 * 0.3 +
             0.75 * 0.9 * 0.9) /
            (0.25 * 0.3 +
             0.75 * 0.9);

        double expectedF =
            (0.25 * 0.2 * 0.2 +
             0.75 * 0.8 * 0.8) /
            (0.25 * 0.2 +
             0.75 * 0.8);

        Assert.Equal(
            expectedCr,
            policy.Memory.GetCrossoverProbability(0),
            precision: 12);

        Assert.Equal(
            expectedF,
            policy.Memory.GetDifferentialWeight(0),
            precision: 12);

        Assert.Equal(
            1,
            policy.MemoryPosition);
    }

    [Fact]
    public void AllZeroSuccessfulCrCreatesTerminalMemory()
    {
        var buffers =
            new DeParameterBuffers(1);

        var policy =
            new LShadeParameterAdaptationPolicy(
                memorySize: 1);

        policy.Initialize(
            buffers,
            activePopulationSize: 1);

        var zeroCr =
            new DeControlParameters(
                DifferentialWeight: 0.6,
                CrossoverProbability: 0.0);

        buffers.SetTrial(
            0,
            in zeroCr);

        var context =
            new DeGenerationAdaptationContext(
                Generation: 1,
                ActivePopulationSize: 1,
                FunctionEvaluations: 2,
                MaximumFunctionEvaluations: 100);

        DeSelectionFeedback[] feedback =
            new[]
            {
                new DeSelectionFeedback(
                    0,
                    Accepted: true,
                    ParentFitness: 2.0,
                    TrialFitness: 1.0,
                    Improvement: 1.0)
            };

        policy.CompleteGeneration(
            in context,
            buffers,
            feedback);

        Assert.True(
            policy.Memory.IsCrossoverTerminal(0));
    }
}