namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

public interface IDePopulationSizePolicy
{
    string Id { get; }

    int GetTargetPopulationSize(
        in DePopulationSizeContext context);
}