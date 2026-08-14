using MetaheuristicsPlatform.Algorithms.DE.Adaptive;
using MetaheuristicsPlatform.Algorithms.DE.Random;
using MetaheuristicsPlatform.Random;

namespace MetaheuristicsPlatform.Tests;

public sealed class DeAdaptiveArchitectureTests
{
    [Fact]
    public void ParameterBuffers_AcceptTrialCopiesTrialToParent()
    {
        var buffers =
            new DeParameterBuffers(4);

        var parent =
            new DeControlParameters(0.5, 0.2);

        var trial =
            new DeControlParameters(0.8, 0.9);

        buffers.SetParent(
            1,
            in parent);

        buffers.SetTrial(
            1,
            in trial);

        buffers.AcceptTrial(1);

        Assert.Equal(
            trial,
            buffers.GetParent(1));
    }

    [Fact]
    public void FixedPolicy_FillsParentAndTrialParameters()
    {
        var buffers =
            new DeParameterBuffers(8);

        var policy =
            new FixedDeParameterAdaptationPolicy(
                0.7,
                0.9);

        policy.Initialize(
            buffers,
            activePopulationSize: 8);

        for (int target = 0;
             target < 8;
             target++)
        {
            Assert.Equal(
                new DeControlParameters(0.7, 0.9),
                buffers.GetParent(target));

            Assert.Equal(
                new DeControlParameters(0.7, 0.9),
                buffers.GetTrial(target));
        }
    }

    [Fact]
    public void FixedPolicy_IsGenerationStable()
    {
        var buffers =
            new DeParameterBuffers(4);

        var policy =
            new FixedDeParameterAdaptationPolicy(
                0.6,
                0.4);

        policy.Initialize(
            buffers,
            activePopulationSize: 4);

        var changed =
            new DeControlParameters(1.5, 0.1);

        buffers.SetTrial(
            0,
            in changed);

        var streams =
            new DeTargetRandomStreams(
                4,
                123UL,
                Xoshiro256StarStarRandomSourceFactory.Instance);

        var context =
            new DeGenerationAdaptationContext(
                Generation: 1,
                ActivePopulationSize: 4,
                FunctionEvaluations: 8,
                MaximumFunctionEvaluations: 100);

        policy.PrepareGeneration(
            in context,
            buffers,
            streams);

        Assert.Equal(
            new DeControlParameters(0.6, 0.4),
            buffers.GetTrial(0));
    }

    [Fact]
    public void LinearPopulationReduction_HitsEndpointsAndMidpoint()
    {
        var policy =
            new LinearDePopulationSizeReductionPolicy();

        var start =
            new DePopulationSizeContext(
                InitialPopulationSize: 100,
                CurrentPopulationSize: 100,
                MinimumPopulationSize: 4,
                FunctionEvaluations: 0,
                MaximumFunctionEvaluations: 1000);

        var middle =
            start with
            {
                FunctionEvaluations = 500
            };

        var end =
            start with
            {
                FunctionEvaluations = 1000
            };

        Assert.Equal(
            100,
            policy.GetTargetPopulationSize(
                in start));

        Assert.Equal(
            52,
            policy.GetTargetPopulationSize(
                in middle));

        Assert.Equal(
            4,
            policy.GetTargetPopulationSize(
                in end));
    }

    [Fact]
    public void ExternalArchive_CopiesVectorsAndDoesNotAliasCaller()
    {
        var archive =
            new DeExternalArchive(
                capacity: 3,
                dimension: 2);

        IRandomSource random =
            Xoshiro256StarStarRandomSourceFactory
                .Instance
                .Create(99UL);

        double[] source =
            new[] { 1.0, 2.0 };

        archive.Add(
            source,
            random);

        source[0] = 999.0;

        double[] sampled =
            new double[2];

        archive.CopyRandomTo(
            random,
            sampled);

        Assert.Equal(
            new[] { 1.0, 2.0 },
            sampled);
    }

    [Fact]
    public void ExternalArchive_NeverExceedsCapacity()
    {
        var archive =
            new DeExternalArchive(
                capacity: 4,
                dimension: 3);

        IRandomSource random =
            Xoshiro256StarStarRandomSourceFactory
                .Instance
                .Create(1234UL);

        for (int i = 0;
             i < 100;
             i++)
        {
            double[] vector =
                new[]
                {
                    (double)i,
                    i + 1.0,
                    i + 2.0
                };

            archive.Add(
                vector,
                random);

            Assert.InRange(
                archive.Count,
                1,
                archive.Capacity);
        }

        Assert.Equal(
            4,
            archive.Count);
    }

    [Fact]
    public void AdaptiveReferences_ContainCanonicalDois()
    {
        Assert.Equal(
            "10.1109/TEVC.2006.872133",
            DeAdaptiveReferences.BrestEtAl2006.Doi);

        Assert.Equal(
            "10.1109/TEVC.2009.2014613",
            DeAdaptiveReferences.ZhangSanderson2009.Doi);

        Assert.Equal(
            "10.1109/CEC.2013.6557555",
            DeAdaptiveReferences.TanabeFukunaga2013.Doi);

        Assert.Equal(
            "10.1109/CEC.2014.6900380",
            DeAdaptiveReferences.TanabeFukunaga2014.Doi);
    }
}