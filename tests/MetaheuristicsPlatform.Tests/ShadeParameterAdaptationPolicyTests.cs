using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Algorithms.DE.Random;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class ShadeParameterAdaptationPolicyTests
{
    [Fact]
    public void DefaultsMatchCanonicalShadeMemory()
    {
        var policy =
            new ShadeParameterAdaptationPolicy();

        Assert.Equal(
            100,
            policy.MemorySize);

        Assert.Equal(
            0,
            policy.MemoryPosition);

        Assert.Equal(
            0.1,
            policy.DistributionScale);

        Assert.Equal(
            0.5,
            policy.Memory.GetDifferentialWeight(0));

        Assert.Equal(
            0.5,
            policy.Memory.GetCrossoverProbability(0));
    }

    [Fact]
    public void PrepareGenerationSamplesValidRanges()
    {
        const int populationSize = 256;

        var buffers =
            new DeParameterBuffers(
                populationSize);

        var policy =
            new ShadeParameterAdaptationPolicy();

        policy.Initialize(
            buffers,
            populationSize);

        var streams =
            new DeTargetRandomStreams(
                populationSize,
                20260814UL,
                Xoshiro256StarStarRandomSourceFactory.Instance);

        var context =
            new DeGenerationAdaptationContext(
                Generation: 1,
                ActivePopulationSize: populationSize,
                FunctionEvaluations: populationSize,
                MaximumFunctionEvaluations: null);

        policy.PrepareGeneration(
            in context,
            buffers,
            streams);

        for (int target = 0;
             target < populationSize;
             target++)
        {
            DeControlParameters value =
                buffers.GetTrial(
                    target);

            Assert.InRange(
                value.DifferentialWeight,
                double.Epsilon,
                1.0);

            Assert.InRange(
                value.CrossoverProbability,
                0.0,
                1.0);
        }
    }

    [Fact]
    public void SuccessfulValuesUseImprovementWeightedMeans()
    {
        var buffers =
            new DeParameterBuffers(2);

        var policy =
            new ShadeParameterAdaptationPolicy(
                memorySize: 4);

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
                MaximumFunctionEvaluations: null);

        DeSelectionFeedback[] feedback =
            new[]
            {
                new DeSelectionFeedback(
                    TargetIndex: 0,
                    Accepted: true,
                    ParentFitness: 10.0,
                    TrialFitness: 9.0,
                    Improvement: 1.0),
                new DeSelectionFeedback(
                    TargetIndex: 1,
                    Accepted: true,
                    ParentFitness: 10.0,
                    TrialFitness: 7.0,
                    Improvement: 3.0)
            };

        policy.CompleteGeneration(
            in context,
            buffers,
            feedback);

        // Weights = 0.25 and 0.75.
        // weighted arithmetic CR = 0.25*0.3 + 0.75*0.9 = 0.75.
        Assert.Equal(
            0.75,
            policy.Memory.GetCrossoverProbability(0),
            precision: 12);

        // weighted Lehmer F =
        // (0.25*0.2^2 + 0.75*0.8^2) /
        // (0.25*0.2    + 0.75*0.8) = 0.753846153846...
        double expectedF =
            (0.25 * 0.2 * 0.2 +
             0.75 * 0.8 * 0.8) /
            (0.25 * 0.2 +
             0.75 * 0.8);

        Assert.Equal(
            expectedF,
            policy.Memory.GetDifferentialWeight(0),
            precision: 12);

        Assert.Equal(
            1,
            policy.MemoryPosition);
    }

    [Fact]
    public void NoSuccessDoesNotAdvanceMemory()
    {
        var buffers =
            new DeParameterBuffers(1);

        var policy =
            new ShadeParameterAdaptationPolicy(
                memorySize: 4);

        policy.Initialize(
            buffers,
            activePopulationSize: 1);

        var context =
            new DeGenerationAdaptationContext(
                Generation: 1,
                ActivePopulationSize: 1,
                FunctionEvaluations: 2,
                MaximumFunctionEvaluations: null);

        DeSelectionFeedback[] feedback =
            new[]
            {
                new DeSelectionFeedback(
                    TargetIndex: 0,
                    Accepted: false,
                    ParentFitness: 1.0,
                    TrialFitness: 1.0,
                    Improvement: 0.0)
            };

        policy.CompleteGeneration(
            in context,
            buffers,
            feedback);

        Assert.Equal(
            0,
            policy.MemoryPosition);

        Assert.Equal(
            0.5,
            policy.Memory.GetDifferentialWeight(0));

        Assert.Equal(
            0.5,
            policy.Memory.GetCrossoverProbability(0));
    }
}