using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.WhaleOptimization;
public sealed class WhaleOptimizationAlgorithmParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 30;
    public int MaximumIterations { get; init; } = 200;
    public double SpiralConstant { get; init; } = 1.0;
    public void Validate()
    {
        if (PopulationSize < 2) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(SpiralConstant) || SpiralConstant <= 0.0) throw new ArgumentOutOfRangeException(nameof(SpiralConstant));
    }
}
