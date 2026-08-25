using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.BatAlgorithm;

public sealed class BatAlgorithmParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 25;
    public int MaximumIterations { get; init; } = 200;
    public double MinimumFrequency { get; init; } = 0.0;
    public double MaximumFrequency { get; init; } = 2.0;
    public double InitialLoudness { get; init; } = 1.0;
    public double InitialPulseRate { get; init; } = 0.5;
    public double LoudnessDecay { get; init; } = 0.9;
    public double PulseGrowth { get; init; } = 0.9;

    public void Validate()
    {
        if (PopulationSize < 2)
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumIterations <= 0)
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (!double.IsFinite(MinimumFrequency) || MinimumFrequency < 0.0)
            throw new ArgumentOutOfRangeException(nameof(MinimumFrequency));
        if (!double.IsFinite(MaximumFrequency) || MaximumFrequency < MinimumFrequency)
            throw new ArgumentOutOfRangeException(nameof(MaximumFrequency));
        if (!double.IsFinite(InitialLoudness) || InitialLoudness <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(InitialLoudness));
        if (!double.IsFinite(InitialPulseRate) || InitialPulseRate < 0.0 || InitialPulseRate > 1.0)
            throw new ArgumentOutOfRangeException(nameof(InitialPulseRate));
        if (!double.IsFinite(LoudnessDecay) || LoudnessDecay <= 0.0 || LoudnessDecay >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(LoudnessDecay));
        if (!double.IsFinite(PulseGrowth) || PulseGrowth <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(PulseGrowth));
    }
}
