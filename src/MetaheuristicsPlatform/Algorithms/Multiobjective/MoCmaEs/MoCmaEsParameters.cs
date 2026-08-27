using MetaheuristicsPlatform.Parameters;
namespace MetaheuristicsPlatform.Algorithms.Multiobjective.MoCmaEs;
public sealed class MoCmaEsParameters : IMetaheuristicParameters
{
    public int PopulationSize { get; init; } = 40;
    public int MaximumGenerations { get; init; } = 200;
    public double InitialStepSizeFraction { get; init; } = 0.2;
    public double CovarianceLearningRate { get; init; } = 0.2;
    public double SuccessTarget { get; init; } = 0.2;
    public double StepSizeDamping { get; init; } = 1.0;
    public void Validate()
    {
        if (PopulationSize < 4) throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        if (MaximumGenerations <= 0) throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        if (!double.IsFinite(InitialStepSizeFraction) || InitialStepSizeFraction <= 0) throw new ArgumentOutOfRangeException(nameof(InitialStepSizeFraction));
        if (!double.IsFinite(CovarianceLearningRate) || CovarianceLearningRate <= 0 || CovarianceLearningRate >= 1) throw new ArgumentOutOfRangeException(nameof(CovarianceLearningRate));
        if (!double.IsFinite(SuccessTarget) || SuccessTarget <= 0 || SuccessTarget >= 1) throw new ArgumentOutOfRangeException(nameof(SuccessTarget));
        if (!double.IsFinite(StepSizeDamping) || StepSizeDamping <= 0) throw new ArgumentOutOfRangeException(nameof(StepSizeDamping));
    }
}
