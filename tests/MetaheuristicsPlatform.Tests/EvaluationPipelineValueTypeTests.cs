using MetaheuristicsPlatform.Evaluation;
using MetaheuristicsPlatform.Evaluation.Delegates;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class EvaluationPipelineValueTypeTests
{
    [Fact]
    public void RefRepairAndImprove_PropagateForStructSolution()
    {
        var pipeline =
            new EvaluationPipeline<int, ValueSolution>(
                new DelegateSolutionDecoder<int, ValueSolution>(
                    static (candidate, _) =>
                        new ValueSolution(candidate)),
                new DelegateSolutionEvaluator<ValueSolution>(
                    static (solution, _) =>
                        solution.Value),
                new EvaluationCharacteristics(false),
                repair:
                    new DelegateSolutionRepair<ValueSolution>(
                        static (
                            ref ValueSolution solution,
                            CancellationToken _) =>
                        {
                            solution =
                                new ValueSolution(
                                    solution.Value + 2);

                            return true;
                        }),
                improver:
                    new DelegateSolutionImprover<ValueSolution>(
                        static (
                            ref ValueSolution solution,
                            CancellationToken _) =>
                        {
                            solution =
                                new ValueSolution(
                                    solution.Value * 3);

                            return true;
                        }),
                feedbackMode:
                    ImprovementFeedbackMode.Baldwinian);

        int candidate = 4;

        EvaluationPipelineResult<ValueSolution> result =
            pipeline.Evaluate(
                ref candidate,
                TestContext.Current.CancellationToken);

        Assert.True(result.WasRepaired);
        Assert.True(result.WasImproved);
        Assert.Equal(18.0, result.Fitness);
        Assert.Equal(18, result.Solution.Value);
        Assert.Equal(4, candidate);
    }

    [Fact]
    public void ByValueDelegateAdapter_RejectsValueTypeRepair()
    {
        SolutionMutationDelegate<ValueSolution> repair =
            static (solution, _) =>
            {
                solution =
                    new ValueSolution(
                        solution.Value + 1);

                return true;
            };

        Assert.Throws<ArgumentException>(
            () =>
                new DelegateSolutionRepair<ValueSolution>(
                    repair));
    }

    [Fact]
    public void RefImprover_CanReplaceReferenceTypeSolution()
    {
        var pipeline =
            new EvaluationPipeline<int, ReferenceSolution>(
                new DelegateSolutionDecoder<int, ReferenceSolution>(
                    static (candidate, _) =>
                        new ReferenceSolution(candidate)),
                new DelegateSolutionEvaluator<ReferenceSolution>(
                    static (solution, _) =>
                        solution.Value),
                new EvaluationCharacteristics(false),
                improver:
                    new DelegateSolutionImprover<ReferenceSolution>(
                        static (
                            ref ReferenceSolution solution,
                            CancellationToken _) =>
                        {
                            solution =
                                new ReferenceSolution(
                                    solution.Value + 10);

                            return true;
                        }),
                feedbackMode:
                    ImprovementFeedbackMode.Baldwinian);

        int candidate = 5;

        EvaluationPipelineResult<ReferenceSolution> result =
            pipeline.Evaluate(
                ref candidate,
                TestContext.Current.CancellationToken);

        Assert.Equal(15.0, result.Fitness);
        Assert.Equal(15, result.Solution.Value);
    }

    private readonly record struct ValueSolution(int Value);

    private sealed record ReferenceSolution(int Value);
}