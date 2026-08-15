using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.SA;

public sealed class SimulatedAnnealingParameters :
    IMetaheuristicParameters
{
    public double InitialTemperature { get; init; } =
        1.0;

    public double MinimumTemperature { get; init; } =
        1e-9;

    public int TransitionsPerTemperatureLevel { get; init; } =
        100;

    public SimulatedAnnealingCoolingScheduleKind CoolingSchedule { get; init; } =
        SimulatedAnnealingCoolingScheduleKind.Geometric;

    /// <summary>
    /// Optional user-supplied schedule. When non-null, this overrides
    /// <see cref="CoolingSchedule"/> and all built-in schedule parameters.
    /// </summary>
    public ISimulatedAnnealingCoolingSchedule? CustomCoolingSchedule { get; init; }

    public double GeometricAlpha { get; init; } =
        0.95;

    public double LundyMeesBeta { get; init; } =
        0.001;

    public double LinearDecrement { get; init; } =
        0.01;

    public int IngberDimension { get; init; } =
        1;

    public double IngberC { get; init; } =
        1.0;

    public double TsallisVisitingQ { get; init; } =
        2.0;

    public double AartsVanLaarhovenDelta { get; init; } =
        0.1;

    public double HuangLambda { get; init; } =
        0.6;

    public double TrikiTargetMeanObjectiveDecrease { get; init; } =
        0.001;

    public bool StopAtMinimumTemperature { get; init; } =
        true;

    public int MaximumConsecutiveSamplingFailures { get; init; } =
        64;

    public void Validate()
    {
        if (!double.IsFinite(
                InitialTemperature) ||
            InitialTemperature <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(InitialTemperature));
        }

        if (!double.IsFinite(
                MinimumTemperature) ||
            MinimumTemperature <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumTemperature));
        }

        if (MinimumTemperature >=
            InitialTemperature)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumTemperature),
                "Minimum temperature must be smaller than initial temperature.");
        }

        if (TransitionsPerTemperatureLevel <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(TransitionsPerTemperatureLevel));
        }

        if (MaximumConsecutiveSamplingFailures <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumConsecutiveSamplingFailures));
        }

        ISimulatedAnnealingCoolingSchedule schedule =
            CreateCoolingSchedule();

        if (schedule is ISimulatedAnnealingStatisticalCoolingSchedule &&
            TransitionsPerTemperatureLevel < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(TransitionsPerTemperatureLevel),
                "Statistical cooling schedules require at least two transitions per temperature level.");
        }
    }

    internal ISimulatedAnnealingCoolingSchedule
        CreateCoolingSchedule()
    {
        if (CustomCoolingSchedule is not null)
        {
            return
                CustomCoolingSchedule;
        }

        return
            CoolingSchedule switch
            {
                SimulatedAnnealingCoolingScheduleKind.Geometric =>
                    new GeometricCoolingSchedule(
                        GeometricAlpha),

                SimulatedAnnealingCoolingScheduleKind.LundyMees =>
                    new LundyMeesCoolingSchedule(
                        LundyMeesBeta),

                SimulatedAnnealingCoolingScheduleKind.Linear =>
                    new LinearCoolingSchedule(
                        LinearDecrement),

                SimulatedAnnealingCoolingScheduleKind.HajekLogarithmic =>
                    new HajekLogarithmicCoolingSchedule(),

                SimulatedAnnealingCoolingScheduleKind.SzuHartleyFastCauchy =>
                    new SzuHartleyFastCauchyCoolingSchedule(),

                SimulatedAnnealingCoolingScheduleKind.IngberVeryFast =>
                    new IngberVeryFastCoolingSchedule(
                        IngberDimension,
                        IngberC),

                SimulatedAnnealingCoolingScheduleKind.TsallisStarioloGeneralized =>
                    new TsallisStarioloGeneralizedCoolingSchedule(
                        TsallisVisitingQ),

                SimulatedAnnealingCoolingScheduleKind.AartsVanLaarhovenStatistical =>
                    new AartsVanLaarhovenStatisticalCoolingSchedule(
                        AartsVanLaarhovenDelta),

                SimulatedAnnealingCoolingScheduleKind.HuangStatistical =>
                    new HuangStatisticalCoolingSchedule(
                        HuangLambda),

                SimulatedAnnealingCoolingScheduleKind.TrikiAdaptive =>
                    new TrikiAdaptiveCoolingSchedule(
                        TrikiTargetMeanObjectiveDecrease),

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(CoolingSchedule))
            };
    }
}
