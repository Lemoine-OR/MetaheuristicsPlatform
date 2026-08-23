using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.AdaptiveLargeNeighborhoodSearch;

public sealed class AdaptiveLargeNeighborhoodSearchParameters : IMetaheuristicParameters
{
    public int DestructionSize { get; init; } = 10;
    public int MaximumIterations { get; init; } = 1000;
    public int SegmentLength { get; init; } = 100;
    public double ReactionFactor { get; init; } = 0.1;
    public double InitialOperatorWeight { get; init; } = 1.0;
    public double GlobalBestReward { get; init; } = 33.0;
    public double ImprovingReward { get; init; } = 9.0;
    public double AcceptedReward { get; init; } = 13.0;
    public double InitialTemperature { get; init; } = 1.0;
    public double CoolingRate { get; init; } = 0.99975;

    public void Validate()
    {
        if (DestructionSize <= 0) throw new ArgumentOutOfRangeException(nameof(DestructionSize));
        if (MaximumIterations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        if (SegmentLength <= 0) throw new ArgumentOutOfRangeException(nameof(SegmentLength));
        if (!double.IsFinite(ReactionFactor) || ReactionFactor < 0.0 || ReactionFactor > 1.0)
            throw new ArgumentOutOfRangeException(nameof(ReactionFactor));
        if (!double.IsFinite(InitialOperatorWeight) || InitialOperatorWeight <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(InitialOperatorWeight));
        ValidateReward(GlobalBestReward, nameof(GlobalBestReward));
        ValidateReward(ImprovingReward, nameof(ImprovingReward));
        ValidateReward(AcceptedReward, nameof(AcceptedReward));
        if (!double.IsFinite(InitialTemperature) || InitialTemperature <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(InitialTemperature));
        if (!double.IsFinite(CoolingRate) || CoolingRate <= 0.0 || CoolingRate > 1.0)
            throw new ArgumentOutOfRangeException(nameof(CoolingRate));
    }

    private static void ValidateReward(double reward, string name)
    {
        if (!double.IsFinite(reward) || reward < 0.0)
            throw new ArgumentOutOfRangeException(name);
    }
}
