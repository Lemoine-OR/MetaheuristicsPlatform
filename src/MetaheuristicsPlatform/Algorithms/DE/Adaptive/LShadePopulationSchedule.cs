namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Equation (10) Linear Population Size Reduction schedule.
/// </summary>
public sealed class LShadePopulationSchedule
{
    private readonly LinearDePopulationSizeReductionPolicy _policy =
        new();

    public int GetTargetPopulationSize(
        int initialPopulationSize,
        int currentPopulationSize,
        int minimumPopulationSize,
        long functionEvaluations,
        long maximumFunctionEvaluations)
    {
        var context =
            new DePopulationSizeContext(
                initialPopulationSize,
                currentPopulationSize,
                minimumPopulationSize,
                functionEvaluations,
                maximumFunctionEvaluations);

        int target =
            _policy.GetTargetPopulationSize(
                in context);

        return Math.Min(
            currentPopulationSize,
            target);
    }
}