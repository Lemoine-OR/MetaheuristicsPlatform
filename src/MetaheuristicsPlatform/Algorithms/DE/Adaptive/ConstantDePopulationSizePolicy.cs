namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

public sealed class ConstantDePopulationSizePolicy :
    IDePopulationSizePolicy
{
    public string Id =>
        "constant";

    public int GetTargetPopulationSize(
        in DePopulationSizeContext context)
    {
        Validate(
            in context);

        return context.InitialPopulationSize;
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
    }
}