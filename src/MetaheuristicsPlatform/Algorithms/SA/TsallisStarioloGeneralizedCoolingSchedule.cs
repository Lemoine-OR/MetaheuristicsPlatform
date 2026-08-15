namespace MetaheuristicsPlatform.Algorithms.SA;

/// <summary>
/// Visiting-temperature cooling law from Tsallis-Stariolo generalized
/// simulated annealing.
/// </summary>
/// <remarks>
/// For artificial time t >= 1:
/// T_q(t) = T_q(1) (2^(q-1)-1) / ((1+t)^(q-1)-1).
/// The q -> 1 limit is logarithmic cooling and q = 2 is the Szu-Hartley
/// inverse-linear law. The full GSA algorithm also changes visiting and
/// acceptance distributions; this class implements only Eq. (14)'s
/// temperature law.
/// DOI: 10.1016/S0378-4371(96)00271-3.
/// </remarks>
public sealed class TsallisStarioloGeneralizedCoolingSchedule :
    ISimulatedAnnealingCoolingSchedule
{
    private const double LogarithmicLimitTolerance =
        1e-10;

    public TsallisStarioloGeneralizedCoolingSchedule(
        double visitingQ)
    {
        if (!double.IsFinite(visitingQ) ||
            visitingQ < 1.0 ||
            visitingQ >= 3.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(visitingQ),
                "The implemented Tsallis-Stariolo visiting-q range is 1 <= q < 3.");
        }

        VisitingQ =
            visitingQ;
    }

    public double VisitingQ { get; }

    public string Id =>
        SimulatedAnnealingCoolingScheduleIds.TsallisStariolo1996;

    public double GetNextTemperature(
        in SimulatedAnnealingCoolingContext context)
    {
        double artificialTime =
            context.CompletedTemperatureLevels +
            1.0;

        double qMinusOne =
            VisitingQ -
            1.0;

        if (Math.Abs(qMinusOne) <=
            LogarithmicLimitTolerance)
        {
            return
                context.InitialTemperature *
                Math.Log(2.0) /
                Math.Log(
                    1.0 +
                    artificialTime);
        }

        double numerator =
            Math.Pow(
                2.0,
                qMinusOne) -
            1.0;

        double denominator =
            Math.Pow(
                1.0 +
                artificialTime,
                qMinusOne) -
            1.0;

        return
            context.InitialTemperature *
            numerator /
            denominator;
    }
}
