namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Dimension-dependent very-fast cooling law introduced by Ingber.
/// </summary>
/// <remarks>
/// T_k = T_0 exp(-c k^(1/D)).
/// The original VFSR/ASA framework also uses parameter-specific generating
/// distributions and re-annealing. This class intentionally implements only
/// the published temperature law.
/// DOI: 10.1016/0895-7177(89)90202-1.
/// </remarks>
public sealed class IngberVeryFastCoolingSchedule :
    ISimulatedAnnealingCoolingSchedule
{
    public IngberVeryFastCoolingSchedule(
        int dimension,
        double c)
    {
        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        if (!double.IsFinite(c) ||
            c <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(c));
        }

        Dimension = dimension;
        C = c;
    }

    public int Dimension { get; }

    public double C { get; }

    public string Id =>
        SimulatedAnnealingCoolingScheduleIds.Ingber1989;

    public double GetNextTemperature(
        in SimulatedAnnealingCoolingContext context)
    {
        double annealingTime =
            context.CompletedTemperatureLevels;

        double exponent =
            Math.Pow(
                annealingTime,
                1.0 / Dimension);

        return
            context.InitialTemperature *
            Math.Exp(
                -C *
                exponent);
    }
}
