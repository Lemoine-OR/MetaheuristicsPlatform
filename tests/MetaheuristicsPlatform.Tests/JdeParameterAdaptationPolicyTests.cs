using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Algorithms.DE.Random;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class JdeParameterAdaptationPolicyTests
{
    [Fact]
    public void DefaultsMatchCanonicalJdeSettings()
    {
        var policy =
            new JdeParameterAdaptationPolicy();

        Assert.Equal(
            new DeControlParameters(0.5, 0.9),
            policy.InitialParameters);

        Assert.Equal(
            0.1,
            policy.DifferentialWeightLowerBound);

        Assert.Equal(
            0.9,
            policy.DifferentialWeightRange);

        Assert.Equal(
            0.1,
            policy.DifferentialWeightAdaptationProbability);

        Assert.Equal(
            0.1,
            policy.CrossoverAdaptationProbability);
    }

    [Fact]
    public void PrepareGeneration_GeneratesCanonicalRanges()
    {
        const int populationSize = 256;

        var buffers =
            new DeParameterBuffers(
                populationSize);

        var policy =
            new JdeParameterAdaptationPolicy();

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

        bool observedChangedF = false;
        bool observedChangedCr = false;

        for (int target = 0;
             target < populationSize;
             target++)
        {
            DeControlParameters trial =
                buffers.GetTrial(
                    target);

            Assert.InRange(
                trial.DifferentialWeight,
                0.1,
                1.0);

            Assert.InRange(
                trial.CrossoverProbability,
                0.0,
                1.0);

            observedChangedF |=
                trial.DifferentialWeight != 0.5;

            observedChangedCr |=
                trial.CrossoverProbability != 0.9;
        }

        Assert.True(
            observedChangedF);

        Assert.True(
            observedChangedCr);
    }

    [Fact]
    public void CompleteGeneration_CommitsOnlyAcceptedTrialParameters()
    {
        var buffers =
            new DeParameterBuffers(2);

        var policy =
            new JdeParameterAdaptationPolicy();

        policy.Initialize(
            buffers,
            activePopulationSize: 2);

        var acceptedTrial =
            new DeControlParameters(
                0.25,
                0.35);

        var rejectedTrial =
            new DeControlParameters(
                0.75,
                0.15);

        buffers.SetTrial(
            0,
            in acceptedTrial);

        buffers.SetTrial(
            1,
            in rejectedTrial);

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
                    0,
                    Accepted: true,
                    ParentFitness: 10.0,
                    TrialFitness: 9.0,
                    Improvement: 1.0),
                new DeSelectionFeedback(
                    1,
                    Accepted: false,
                    ParentFitness: 10.0,
                    TrialFitness: 11.0,
                    Improvement: 0.0)
            };

        policy.CompleteGeneration(
            in context,
            buffers,
            feedback);

        Assert.Equal(
            acceptedTrial,
            buffers.GetParent(0));

        Assert.Equal(
            new DeControlParameters(0.5, 0.9),
            buffers.GetParent(1));
    }
}