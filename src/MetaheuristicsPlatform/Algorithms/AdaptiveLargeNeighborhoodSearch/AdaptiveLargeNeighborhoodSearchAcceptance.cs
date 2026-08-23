using MetaheuristicsPlatform.Algorithms.LargeNeighborhoodSearch;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Trajectory;

namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

public sealed class GeometricSimulatedAnnealingLargeNeighborhoodAcceptancePolicy :
    ILargeNeighborhoodAcceptancePolicy
{
    public GeometricSimulatedAnnealingLargeNeighborhoodAcceptancePolicy(
        double initialTemperature,
        double coolingRate)
    {
        if (!double.IsFinite(initialTemperature) || initialTemperature <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(initialTemperature));
        if (!double.IsFinite(coolingRate) || coolingRate <= 0.0 || coolingRate > 1.0)
            throw new ArgumentOutOfRangeException(nameof(coolingRate));

        InitialTemperature = initialTemperature;
        CoolingRate = coolingRate;
    }

    public double InitialTemperature { get; }
    public double CoolingRate { get; }

    public double GetTemperature(int iteration)
    {
        if (iteration <= 0)
            throw new ArgumentOutOfRangeException(nameof(iteration));

        return InitialTemperature * Math.Pow(CoolingRate, iteration - 1);
    }

    public bool ShouldAccept(
        in LargeNeighborhoodAcceptanceContext context,
        IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        double degradation =
            TrajectoryObjectiveComparison.ComputeDegradation(
                context.Sense,
                context.CurrentObjective,
                context.CandidateObjective);

        if (degradation <= 0.0)
            return true;

        double temperature = GetTemperature(context.Iteration);

        if (!double.IsFinite(temperature) || temperature <= 0.0)
            return false;

        return random.NextDouble() < Math.Exp(-degradation / temperature);
    }
}
