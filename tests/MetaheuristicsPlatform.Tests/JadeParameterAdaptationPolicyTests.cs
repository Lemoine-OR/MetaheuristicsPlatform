using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Algorithms.DE.Random;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class JadeParameterAdaptationPolicyTests
{
    [Fact]
    public void DefaultsMatchCanonicalJadeValues()
    {
        var policy =
            new JadeParameterAdaptationPolicy();

        Assert.Equal(
            0.5,
            policy.MeanDifferentialWeight);

        Assert.Equal(
            0.5,
            policy.MeanCrossoverProbability);

        Assert.Equal(
            0.1,
            policy.AdaptationRate);

        Assert.Equal(
            0.1,
            policy.DistributionScale);
    }

    [Fact]
    public void GeneratedParametersRespectJadeRanges()
    {
        const int populationSize = 256;

        var buffers =
            new DeParameterBuffers(
                populationSize);

        var policy =
            new JadeParameterAdaptationPolicy();

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
            DeControlParameters sampled =
                buffers.GetTrial(
                    target);

            Assert.InRange(
                sampled.DifferentialWeight,
                double.Epsilon,
                1.0);

            Assert.InRange(
                sampled.CrossoverProbability,
                0.0,
                1.0);
        }
    }

    [Fact]
    public void SuccessfulParametersUpdateArithmeticAndLehmerMeans()
    {
        var buffers =
            new DeParameterBuffers(2);

        var policy =
            new JadeParameterAdaptationPolicy(
                initialMeanDifferentialWeight: 0.5,
                initialMeanCrossoverProbability: 0.5,
                adaptationRate: 0.1,
                distributionScale: 0.1);

        policy.Initialize(
            buffers,
            activePopulationSize: 2);

        var first =
            new DeControlParameters(
                0.2,
                0.3);

        var second =
            new DeControlParameters(
                0.8,
                0.9);

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
                    0,
                    Accepted: true,
                    ParentFitness: 10.0,
                    TrialFitness: 9.0,
                    Improvement: 1.0),
                new DeSelectionFeedback(
                    1,
                    Accepted: true,
                    ParentFitness: 12.0,
                    TrialFitness: 11.0,
                    Improvement: 1.0)
            };

        policy.CompleteGeneration(
            in context,
            buffers,
            feedback);

        // Arithmetic mean CR = (0.3 + 0.9)/2 = 0.6
        // mu_CR' = 0.9*0.5 + 0.1*0.6 = 0.51
        Assert.Equal(
            0.51,
            policy.MeanCrossoverProbability,
            precision: 12);

        // Lehmer F = (0.2^2 + 0.8^2)/(0.2+0.8) = 0.68
        // mu_F' = 0.9*0.5 + 0.1*0.68 = 0.518
        Assert.Equal(
            0.518,
            policy.MeanDifferentialWeight,
            precision: 12);
    }

    [Fact]
    public void NoSuccessLeavesMeansUnchanged()
    {
        var buffers =
            new DeParameterBuffers(1);

        var policy =
            new JadeParameterAdaptationPolicy();

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
                    0,
                    Accepted: false,
                    ParentFitness: 1.0,
                    TrialFitness: 2.0,
                    Improvement: 0.0)
            };

        policy.CompleteGeneration(
            in context,
            buffers,
            feedback);

        Assert.Equal(
            0.5,
            policy.MeanDifferentialWeight);

        Assert.Equal(
            0.5,
            policy.MeanCrossoverProbability);
    }
}