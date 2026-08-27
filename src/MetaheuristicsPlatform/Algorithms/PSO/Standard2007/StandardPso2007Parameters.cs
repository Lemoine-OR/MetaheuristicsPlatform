using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.PSO.Standard2007;

public sealed class StandardPso2007Parameters : IMetaheuristicParameters
{
    public int? SwarmSize { get; init; }
    public int ExpectedInformerCount { get; init; } = 3;
    public int MaximumIterations { get; init; } = 1000;
    public double InertiaWeight { get; init; } = 1.0 / (2.0 * Math.Log(2.0));
    public double AccelerationCoefficient { get; init; } = 0.5 + Math.Log(2.0);

    public int ResolveSwarmSize(int dimension) =>
        SwarmSize ?? (10 + (int)Math.Floor(2.0 * Math.Sqrt(dimension)));

    public void Validate()
    {
        if (SwarmSize.HasValue && SwarmSize.Value <= 1)
            throw new ArgumentOutOfRangeException(nameof(SwarmSize));
        if (ExpectedInformerCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(ExpectedInformerCount));
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        RequirePositive(InertiaWeight, nameof(InertiaWeight));
        RequirePositive(AccelerationCoefficient, nameof(AccelerationCoefficient));
    }

    private static void RequirePositive(double value, string name)
    {
        if (!double.IsFinite(value) || value <= 0.0)
            throw new ArgumentOutOfRangeException(name);
    }
}
