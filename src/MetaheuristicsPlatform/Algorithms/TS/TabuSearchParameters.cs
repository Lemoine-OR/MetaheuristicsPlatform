using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.TS;

public enum TabuTenurePolicyKind
{
    Fixed = 0,
    UniformRandom = 1
}

public enum TabuAspirationCriterionKind
{
    BestSoFar = 0,
    None = 1
}

/// <summary>
/// Parameters for the generic short-term-memory Tabu Search engine.
/// </summary>
public sealed class TabuSearchParameters : IMetaheuristicParameters
{
    public TabuTenurePolicyKind TenurePolicyKind { get; init; } =
        TabuTenurePolicyKind.Fixed;

    public int FixedTabuTenure { get; init; } = 7;

    public int RandomTenureMinimum { get; init; } = 5;

    public int RandomTenureMaximum { get; init; } = 10;

    public ITabuTenurePolicy? CustomTenurePolicy { get; init; }

    public TabuAspirationCriterionKind AspirationCriterionKind { get; init; } =
        TabuAspirationCriterionKind.BestSoFar;

    public ITabuAspirationCriterion? CustomAspirationCriterion { get; init; }

    public int MemoryInitialCapacity { get; init; } = 128;

    public void Validate()
    {
        if (FixedTabuTenure <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(FixedTabuTenure));
        }

        if (RandomTenureMinimum <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(RandomTenureMinimum));
        }

        if (RandomTenureMaximum < RandomTenureMinimum ||
            RandomTenureMaximum == int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(RandomTenureMaximum));
        }

        if (MemoryInitialCapacity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MemoryInitialCapacity));
        }

        if (!Enum.IsDefined(TenurePolicyKind))
        {
            throw new ArgumentOutOfRangeException(nameof(TenurePolicyKind));
        }

        if (!Enum.IsDefined(AspirationCriterionKind))
        {
            throw new ArgumentOutOfRangeException(nameof(AspirationCriterionKind));
        }
    }

    public ITabuTenurePolicy CreateTenurePolicy() =>
        CustomTenurePolicy ??
        (TenurePolicyKind switch
        {
            TabuTenurePolicyKind.Fixed =>
                new FixedTabuTenurePolicy(FixedTabuTenure),
            TabuTenurePolicyKind.UniformRandom =>
                new UniformRandomTabuTenurePolicy(
                    RandomTenureMinimum,
                    RandomTenureMaximum),
            _ => throw new ArgumentOutOfRangeException(nameof(TenurePolicyKind))
        });

    public ITabuAspirationCriterion CreateAspirationCriterion() =>
        CustomAspirationCriterion ??
        (AspirationCriterionKind switch
        {
            TabuAspirationCriterionKind.BestSoFar =>
                new BestSoFarAspirationCriterion(),
            TabuAspirationCriterionKind.None =>
                new NoTabuAspirationCriterion(),
            _ => throw new ArgumentOutOfRangeException(nameof(AspirationCriterionKind))
        });
}
