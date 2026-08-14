namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Geometric cooling: T_next = alpha * T.
/// </summary>
public sealed class GeometricCoolingSchedule :
    ISimulatedAnnealingCoolingSchedule
{
    public GeometricCoolingSchedule(
        double alpha = 0.95)
    {
        if (!double.IsFinite(alpha) ||
            alpha <= 0.0 ||
            alpha >= 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(alpha),
                "Geometric alpha must lie strictly between zero and one.");
        }

        Alpha = alpha;
    }

    public string Id =>
        "geometric";

    public double Alpha { get; }

    public double GetNextTemperature(
        in SimulatedAnnealingCoolingContext context)
    {
        ValidateTemperature(
            context.CurrentTemperature);

        return
            Alpha *
            context.CurrentTemperature;
    }

    private static void ValidateTemperature(
        double temperature)
    {
        if (!double.IsFinite(temperature) ||
            temperature <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(temperature));
        }
    }
}