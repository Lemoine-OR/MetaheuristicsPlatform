namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Finite-horizon linear temperature decrement.
/// </summary>
/// <remarks>
/// T_(k+1) = max(0, T_k - beta), beta &gt; 0.
/// Linear schedules are classical finite-length practical schedules; they do
/// not carry the asymptotic global-convergence guarantee of sufficiently slow
/// logarithmic cooling.
/// </remarks>
public sealed class LinearCoolingSchedule :
    ISimulatedAnnealingCoolingSchedule
{
    public LinearCoolingSchedule(
        double decrement)
    {
        if (!double.IsFinite(decrement) ||
            decrement <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(decrement));
        }

        Decrement = decrement;
    }

    public double Decrement { get; }

    public string Id =>
        SimulatedAnnealingCoolingScheduleIds.Linear;

    public double GetNextTemperature(
        in SimulatedAnnealingCoolingContext context) =>
        Math.Max(
            0.0,
            context.CurrentTemperature -
            Decrement);
}
