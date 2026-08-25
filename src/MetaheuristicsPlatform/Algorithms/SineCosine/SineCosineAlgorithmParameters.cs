using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.SineCosine;
public sealed class SineCosineAlgorithmParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 30;
    public int MaximumIterations { get; init; } = 200;
    public double InitialAmplitude { get; init; } = 2.0;
    public void Validate()
    {
        if (PopulationSize < 2) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(InitialAmplitude) || InitialAmplitude <= 0.0) throw new ArgumentOutOfRangeException(nameof(InitialAmplitude));
    }
}
