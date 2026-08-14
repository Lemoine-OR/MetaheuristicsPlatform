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

    public double GeometricAlpha { get; init; } =
        0.95;

    public double LundyMeesBeta { get; init; } =
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

        _ =
            CreateCoolingSchedule();
    }

    internal ISimulatedAnnealingCoolingSchedule
        CreateCoolingSchedule() =>
        CoolingSchedule switch
        {
            SimulatedAnnealingCoolingScheduleKind.Geometric =>
                new GeometricCoolingSchedule(
                    GeometricAlpha),

            SimulatedAnnealingCoolingScheduleKind.LundyMees =>
                new LundyMeesCoolingSchedule(
                    LundyMeesBeta),

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(CoolingSchedule))
        };
}