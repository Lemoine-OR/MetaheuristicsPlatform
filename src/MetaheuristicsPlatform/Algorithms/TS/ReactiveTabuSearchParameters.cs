using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.TS;

/// <summary>
/// Parameters for the Battiti-Tecchiolli Reactive Tabu Search implementation.
/// </summary>
public sealed class ReactiveTabuSearchParameters : IMetaheuristicParameters
{
    public int InitialTabuTenure { get; init; } = 1;
    public int MinimumTabuTenure { get; init; } = 1;
    public int MaximumTabuTenure { get; init; } = 128;

    public double TenureIncreaseFactor { get; init; } = 1.3;
    public double TenureDecreaseFactor { get; init; } = 0.9;
    public int TenureDecreaseAfterIterationsWithoutRepetition { get; init; } = 100;

    public double CycleLengthMovingAverageAlpha { get; init; } = 0.1;
    public int DiversificationRepetitionThreshold { get; init; } = 3;
    public double DiversificationCycleMultiplier { get; init; } = 1.0;
    public int MaximumDiversificationMoves { get; init; } = 10_000;

    /// <summary>
    /// Linear penalty applied to long-term candidate-attribute frequency.
    /// Zero preserves pure objective ranking.
    /// </summary>
    public double FrequencyPenaltyWeight { get; init; }

    /// <summary>
    /// Optional elite restart after this many iterations without a new global best.
    /// Zero disables this generic intensification mechanism.
    /// </summary>
    public int IntensificationAfterIterationsWithoutImprovement { get; init; }

    public TabuAspirationCriterionKind AspirationCriterionKind { get; init; } =
        TabuAspirationCriterionKind.BestSoFar;

    public ITabuAspirationCriterion? CustomAspirationCriterion { get; init; }

    public IReactiveTabuTenurePolicy? CustomReactiveTenurePolicy { get; init; }

    public int MemoryInitialCapacity { get; init; } = 256;

    public void Validate()
    {
        if (MinimumTabuTenure <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MinimumTabuTenure));
        }

        if (MaximumTabuTenure < MinimumTabuTenure)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumTabuTenure));
        }

        if (InitialTabuTenure < MinimumTabuTenure ||
            InitialTabuTenure > MaximumTabuTenure)
        {
            throw new ArgumentOutOfRangeException(nameof(InitialTabuTenure));
        }

        if (!double.IsFinite(TenureIncreaseFactor) ||
            TenureIncreaseFactor <= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(TenureIncreaseFactor));
        }

        if (!double.IsFinite(TenureDecreaseFactor) ||
            TenureDecreaseFactor <= 0.0 ||
            TenureDecreaseFactor >= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(TenureDecreaseFactor));
        }

        if (TenureDecreaseAfterIterationsWithoutRepetition <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(TenureDecreaseAfterIterationsWithoutRepetition));
        }

        if (!double.IsFinite(CycleLengthMovingAverageAlpha) ||
            CycleLengthMovingAverageAlpha <= 0.0 ||
            CycleLengthMovingAverageAlpha > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(CycleLengthMovingAverageAlpha));
        }

        if (DiversificationRepetitionThreshold <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(DiversificationRepetitionThreshold));
        }

        if (!double.IsFinite(DiversificationCycleMultiplier) ||
            DiversificationCycleMultiplier <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(DiversificationCycleMultiplier));
        }

        if (MaximumDiversificationMoves <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumDiversificationMoves));
        }

        if (!double.IsFinite(FrequencyPenaltyWeight) ||
            FrequencyPenaltyWeight < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(FrequencyPenaltyWeight));
        }

        if (IntensificationAfterIterationsWithoutImprovement < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(IntensificationAfterIterationsWithoutImprovement));
        }

        if (MemoryInitialCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MemoryInitialCapacity));
        }

        if (!Enum.IsDefined(AspirationCriterionKind))
        {
            throw new ArgumentOutOfRangeException(nameof(AspirationCriterionKind));
        }
    }

    public IReactiveTabuTenurePolicy CreateReactiveTenurePolicy() =>
        CustomReactiveTenurePolicy ??
        new ReactiveTabuTenurePolicy(
            InitialTabuTenure,
            MinimumTabuTenure,
            MaximumTabuTenure,
            TenureIncreaseFactor,
            TenureDecreaseFactor,
            TenureDecreaseAfterIterationsWithoutRepetition,
            CycleLengthMovingAverageAlpha,
            DiversificationRepetitionThreshold,
            DiversificationCycleMultiplier,
            MaximumDiversificationMoves);

    public ITabuAspirationCriterion CreateAspirationCriterion() =>
        CustomAspirationCriterion ??
        (AspirationCriterionKind switch
        {
            TabuAspirationCriterionKind.BestSoFar =>
                new BestSoFarAspirationCriterion(),
            TabuAspirationCriterionKind.None =>
                new NoTabuAspirationCriterion(),
            _ => throw new ArgumentOutOfRangeException(
                nameof(AspirationCriterionKind))
        });
}
