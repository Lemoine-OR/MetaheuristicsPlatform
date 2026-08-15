namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Stateful feedback controller for Reactive Tabu Search tenure and escape requests.
/// </summary>
public interface IReactiveTabuTenurePolicy
{
    int CurrentTenure { get; }
    double MovingAverageCycleLength { get; }
    long RepetitionsObserved { get; }

    ReactiveTabuReaction Observe(
        in ReactiveTabuTenureContext context);

    void AcknowledgeDiversification();
}
