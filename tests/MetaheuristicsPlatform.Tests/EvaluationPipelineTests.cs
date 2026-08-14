using MetaheuristicsPlatform.Evaluation;
using MetaheuristicsPlatform.Evaluation.Delegates;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class EvaluationPipelineTests
{
    [Fact]
    public void DecodeRepairEvaluate_ComposesInOrder()
    {
        var pipeline =
            new EvaluationPipeline<int, MutableSolution>(
                new DelegateSolutionDecoder<int, MutableSolution>(
                    static (candidate, _) =>
                        new MutableSolution(candidate)),
                new DelegateSolutionEvaluator<MutableSolution>(
                    static (solution, _) =>
                        solution.Value),
                new EvaluationCharacteristics(
                    false,
                    EvaluationCostHint.Light,
                    EvaluationVariabilityHint.Uniform),
                repair:
                    new DelegateSolutionRepair<MutableSolution>(
                        static (solution, _) =>
                        {
                            if (solution.Value >= 0)
                            {
                                return false;
                            }

                            solution.Value = 0;
                            return true;
                        }));

        int candidate = -5;

        EvaluationPipelineResult<MutableSolution> result =
            pipeline.Evaluate(
                ref candidate,
                TestContext.Current.CancellationToken);

        Assert.Equal(0.0, result.Fitness);
        Assert.True(result.WasRepaired);
        Assert.False(result.WasImproved);
        Assert.False(result.FeedbackApplied);
        Assert.Equal(-5, candidate);
    }

    [Fact]
    public void BaldwinianImprovement_ChangesFitnessButNotCandidate()
    {
        var pipeline =
            CreateImprovingPipeline(
                ImprovementFeedbackMode.Baldwinian);

        int candidate = 10;

        EvaluationPipelineResult<MutableSolution> result =
            pipeline.Evaluate(
                ref candidate,
                TestContext.Current.CancellationToken);

        Assert.Equal(5.0, result.Fitness);
        Assert.True(result.WasImproved);
        Assert.False(result.FeedbackApplied);
        Assert.Equal(10, candidate);
    }

    [Fact]
    public void LamarckianImprovement_ProjectsSolutionBackToValueCandidate()
    {
        var pipeline =
            CreateImprovingPipeline(
                ImprovementFeedbackMode.Lamarckian);

        int candidate = 10;

        EvaluationPipelineResult<MutableSolution> result =
            pipeline.Evaluate(
                ref candidate,
                TestContext.Current.CancellationToken);

        Assert.Equal(5.0, result.Fitness);
        Assert.True(result.WasImproved);
        Assert.True(result.FeedbackApplied);
        Assert.Equal(5, candidate);
    }

    [Fact]
    public void LamarckianMode_RequiresFeedback()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new EvaluationPipeline<int, MutableSolution>(
                    new DelegateSolutionDecoder<int, MutableSolution>(
                        static (candidate, _) =>
                            new MutableSolution(candidate)),
                    new DelegateSolutionEvaluator<MutableSolution>(
                        static (solution, _) =>
                            solution.Value),
                    new EvaluationCharacteristics(false),
                    improver:
                        new DelegateSolutionImprover<MutableSolution>(
                            static (_, _) => false),
                    feedbackMode:
                        ImprovementFeedbackMode.Lamarckian));
    }

    private static EvaluationPipeline<int, MutableSolution>
        CreateImprovingPipeline(
            ImprovementFeedbackMode mode) =>
        new(
            new DelegateSolutionDecoder<int, MutableSolution>(
                static (candidate, _) =>
                    new MutableSolution(candidate)),
            new DelegateSolutionEvaluator<MutableSolution>(
                static (solution, _) =>
                    solution.Value),
            new EvaluationCharacteristics(
                true,
                EvaluationCostHint.Medium,
                EvaluationVariabilityHint.Uniform),
            improver:
                new DelegateSolutionImprover<MutableSolution>(
                    static (solution, _) =>
                    {
                        solution.Value /= 2;
                        return true;
                    }),
            feedbackMode:
                mode,
            feedback:
                mode ==
                    ImprovementFeedbackMode.Lamarckian
                    ? new DelegateLamarckianFeedback<int, MutableSolution>(
                        static (
                            MutableSolution solution,
                            ref int candidate,
                            CancellationToken _) =>
                        {
                            candidate =
                                solution.Value;
                        })
                    : null);

    private sealed class MutableSolution
    {
        internal MutableSolution(int value)
        {
            Value = value;
        }

        internal int Value { get; set; }
    }
}