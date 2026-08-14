using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class EvaluationExecutorTests
{
    [Fact]
    public void SequentialEvaluation_VisitsEveryCandidateOnce()
    {
        int[] visits = new int[100];

        EvaluationExecutor.ForCandidates(
            100,
            10,
            new EvaluationCharacteristics(
                true,
                EvaluationCostHint.Light,
                EvaluationVariabilityHint.Uniform),
            new EvaluationExecutionOptions
            {
                Mode =
                    EvaluationExecutionMode.Sequential
            },
            (start, end) =>
            {
                for (int i = start; i < end; i++)
                {
                    visits[i]++;
                }
            },
            TestContext.Current.CancellationToken);

        Assert.All(
            visits,
            static count =>
                Assert.Equal(1, count));
    }

    [Fact]
    public void HeavyVariableParallelEvaluation_VisitsEveryCandidateOnce()
    {
        int[] visits = new int[100];

        EvaluationExecutor.ForCandidates(
            100,
            1,
            new EvaluationCharacteristics(
                true,
                EvaluationCostHint.Heavy,
                EvaluationVariabilityHint.High),
            new EvaluationExecutionOptions
            {
                Mode =
                    EvaluationExecutionMode.Parallel,
                MaxDegreeOfParallelism = 4
            },
            (start, end) =>
            {
                for (int i = start; i < end; i++)
                {
                    Interlocked.Increment(
                        ref visits[i]);
                }
            },
            TestContext.Current.CancellationToken);

        Assert.All(
            visits,
            static count =>
                Assert.Equal(1, count));
    }
}