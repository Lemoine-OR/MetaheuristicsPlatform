namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Lundy-Mees cooling: T_next = T / (1 + beta * T).
/// </summary>
/// <remarks>
/// M. Lundy, A. Mees,
/// "Convergence of an annealing algorithm",
/// Mathematical Programming 34(1), 111-124, 1986.
/// DOI: 10.1007/BF01582166.
/// </remarks>
public sealed class LundyMeesCoolingSchedule :
    ISimulatedAnnealingCoolingSchedule
{
    public LundyMeesCoolingSchedule(
        double beta)
    {
        if (!double.IsFinite(beta) ||
            beta <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(beta));
        }

        Beta = beta;
    }

    public string Id =>
        "lundy-mees-1986";

    public double Beta { get; }

    public double GetNextTemperature(
        in SimulatedAnnealingCoolingContext context)
    {
        double temperature =
            context.CurrentTemperature;

        if (!double.IsFinite(temperature) ||
            temperature <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(context));
        }

        return
            temperature /
            (1.0 +
             Beta *
             temperature);
    }
}