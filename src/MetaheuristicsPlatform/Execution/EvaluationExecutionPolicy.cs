namespace MetaheuristicsPlatform.Execution;

/// <summary>
/// Generic Auto decision policy for candidate evaluation.
/// </summary>
public static class EvaluationExecutionPolicy
{
    public static bool ShouldParallelize(
        int candidateCount,
        int representationDimension,
        EvaluationCharacteristics characteristics,
        EvaluationExecutionOptions options,
        int processorCount)
    {
        if (candidateCount <= 1 ||
            processorCount <= 1 ||
            !characteristics.SupportsParallelEvaluation)
        {
            return false;
        }

        options.Validate();

        return options.Mode switch
        {
            EvaluationExecutionMode.Sequential => false,
            EvaluationExecutionMode.Parallel => true,
            EvaluationExecutionMode.Auto =>
                ShouldAutoParallelize(
                    candidateCount,
                    representationDimension,
                    characteristics,
                    processorCount),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static bool ShouldAutoParallelize(
        int candidateCount,
        int representationDimension,
        EvaluationCharacteristics characteristics,
        int processorCount)
    {
        if (candidateCount <= 1 ||
            processorCount <= 1 ||
            !characteristics.SupportsParallelEvaluation)
        {
            return false;
        }

        long work =
            (long)candidateCount *
            Math.Max(1, representationDimension);

        return characteristics.CostHint switch
        {
            EvaluationCostHint.VeryHeavy =>
                candidateCount >= 2,

            EvaluationCostHint.Heavy =>
                candidateCount >= 2,

            EvaluationCostHint.Medium =>
                candidateCount >=
                    Math.Max(8, processorCount) &&
                work >=
                    Math.Max(
                        512L,
                        64L * processorCount),

            EvaluationCostHint.Trivial or
            EvaluationCostHint.Light or
            EvaluationCostHint.Unknown =>
                PsoLikeCheapWorkRule(
                    candidateCount,
                    work,
                    processorCount),

            _ => false
        };
    }

    private static bool PsoLikeCheapWorkRule(
        int candidateCount,
        long work,
        int processorCount)
    {
        int minimumCandidates =
            Math.Max(
                16,
                2 * processorCount);

        long minimumWork =
            Math.Max(
                1_024L,
                160L * processorCount);

        return
            candidateCount >= minimumCandidates &&
            work >= minimumWork;
    }
}