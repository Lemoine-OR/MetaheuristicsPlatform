namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Linear population-size reduction schedule used by the L-SHADE family.
///
/// The target size is linearly interpolated from the initial population toward
/// the minimum population according to consumed function evaluations.
/// </summary>
public sealed class LinearDePopulationSizeReductionPolicy :
    IDePopulationSizePolicy
{
    public string Id =>
        "linear-evaluation-budget";

    public int GetTargetPopulationSize(
        in DePopulationSizeContext context)
    {
        Validate(
            in context);

        long boundedEvaluations =
            Math.Clamp(
                context.FunctionEvaluations,
                0L,
                context.MaximumFunctionEvaluations);

        double progress =
            (double)boundedEvaluations /
            context.MaximumFunctionEvaluations;

        double interpolated =
            context.InitialPopulationSize +
            (context.MinimumPopulationSize -
                context.InitialPopulationSize) *
            progress;

        int target =
            (int)Math.Round(
                interpolated,
                MidpointRounding.AwayFromZero);

        return Math.Clamp(
            target,
            context.MinimumPopulationSize,
            context.InitialPopulationSize);
    }

    private static void Validate(
        in DePopulationSizeContext context)
    {
        if (context.InitialPopulationSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(context));
        }

        if (context.MinimumPopulationSize <= 0 ||
            context.MinimumPopulationSize >
                context.InitialPopulationSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(context));
        }

        if (context.MaximumFunctionEvaluations <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(context));
        }
    }
}