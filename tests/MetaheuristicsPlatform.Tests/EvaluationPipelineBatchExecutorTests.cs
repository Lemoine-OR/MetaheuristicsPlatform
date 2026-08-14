using MetaheuristicsPlatform.Evaluation;
using MetaheuristicsPlatform.Evaluation.Delegates;
using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class EvaluationPipelineBatchExecutorTests
{
    [Fact]
    public void ParallelLamarckianBatch_UpdatesCandidatesAndFitness()
    {
        int[] candidates =
            Enumerable.Range(1, 64)
                .Select(static value => value * 2)
                .ToArray();

        double[] fitness =
            new double[candidates.Length];

        var pipeline =
            new EvaluationPipeline<int, MutableSolution>(
                new DelegateSolutionDecoder<int, MutableSolution>(
                    static (candidate, _) =>
                        new MutableSolution(candidate)),
                new DelegateSolutionEvaluator<MutableSolution>(
                    static (solution, _) =>
                        solution.Value * solution.Value),
                new EvaluationCharacteristics(
                    true,
                    EvaluationCostHint.Heavy,
                    EvaluationVariabilityHint.High),
                improver:
                    new DelegateSolutionImprover<MutableSolution>(
                        static (solution, _) =>
                        {
                            solution.Value /= 2;
                            return true;
                        }),
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

        EvaluationPipelineBatchExecutor.Evaluate(
            candidates,
            fitness,
            representationDimension: 1,
            pipeline,
            new EvaluationExecutionOptions
            {
                Mode =
                    EvaluationExecutionMode.Parallel,
                MaxDegreeOfParallelism = 4
            },
            TestContext.Current.CancellationToken);

        for (int i = 0;
             i < candidates.Length;
             i++)
        {
            int expected = i + 1;

            Assert.Equal(
                expected,
                candidates[i]);

            Assert.Equal(
                expected * expected,
                fitness[i]);
        }
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