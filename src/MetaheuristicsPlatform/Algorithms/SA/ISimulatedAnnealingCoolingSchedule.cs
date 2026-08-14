namespace MetaheuristicsPlatform.Algorithms.SA;

public interface ISimulatedAnnealingCoolingSchedule
{
    string Id { get; }

    double GetNextTemperature(
        in SimulatedAnnealingCoolingContext context);
}