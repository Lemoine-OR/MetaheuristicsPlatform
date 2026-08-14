using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Evaluation;
using MetaheuristicsPlatform.Evaluation.Delegates;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class EvaluationPipelineProblemAdapterTests
{
    [Fact]
    public void BaldwinianPipeline_CanBeUsedAsStandardOptimizationProblem()
    {
        var pipeline =
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
                        static (solution, _) =>
                        {
                            solution.Value = 1;
                            return true;
                        }),
                feedbackMode:
                    ImprovementFeedbackMode.Baldwinian);

        var problem =
            new EvaluationPipelineProblemAdapter<int, MutableSolution>(
                OptimizationSense.Minimize,
                pipeline);

        Assert.Equal(
            1.0,
            problem.Evaluate(100));
    }

    [Fact]
    public void LamarckianPipeline_IsRejectedByValueOnlyProblemAdapter()
    {
        var pipeline =
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
                    ImprovementFeedbackMode.Lamarckian,
                feedback:
                    new DelegateLamarckianFeedback<int, MutableSolution>(
                        static (
                            MutableSolution solution,
                            ref int candidate,
                            CancellationToken _) =>
                        {
                            candidate =
                                solution.Value;
                        }));

        Assert.Throws<ArgumentException>(
            () =>
                new EvaluationPipelineProblemAdapter<int, MutableSolution>(
                    OptimizationSense.Minimize,
                    pipeline));
    }

    private sealed class MutableSolution
    {
        internal MutableSolution(int value)
        {
            Value = value;
        }

        internal int Value { get; set; }
    }
}