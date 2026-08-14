using MetaheuristicsPlatform.Execution;

namespace MetaheuristicsPlatform.Tests;

public sealed class EvaluationExecutionPolicyTests
{
    [Fact]
    public void UnsupportedParallelEvaluation_IsAlwaysSequential()
    {
        var characteristics =
            new EvaluationCharacteristics(
                SupportsParallelEvaluation: false,
                EvaluationCostHint.Heavy,
                EvaluationVariabilityHint.High);

        Assert.False(
            EvaluationExecutionPolicy.ShouldAutoParallelize(
                256,
                128,
                characteristics,
                16));
    }

    [Fact]
    public void MediumEvaluation_ParallelizesEarlierThanCheapEvaluation()
    {
        var cheap =
            new EvaluationCharacteristics(
                true,
                EvaluationCostHint.Trivial,
                EvaluationVariabilityHint.Uniform);

        var medium =
            new EvaluationCharacteristics(
                true,
                EvaluationCostHint.Medium,
                EvaluationVariabilityHint.Uniform);

        Assert.False(
            EvaluationExecutionPolicy.ShouldAutoParallelize(
                64,
                32,
                cheap,
                16));

        Assert.True(
            EvaluationExecutionPolicy.ShouldAutoParallelize(
                64,
                32,
                medium,
                16));
    }

    [Fact]
    public void HeavyEvaluation_CanParallelizeSmallCandidateSets()
    {
        var heavy =
            new EvaluationCharacteristics(
                true,
                EvaluationCostHint.Heavy,
                EvaluationVariabilityHint.High);

        Assert.True(
            EvaluationExecutionPolicy.ShouldAutoParallelize(
                4,
                1,
                heavy,
                16));
    }
}