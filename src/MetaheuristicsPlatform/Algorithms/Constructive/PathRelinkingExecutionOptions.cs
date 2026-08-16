namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>Direction policy used to connect two elite solutions.</summary>
public enum PathRelinkingDirectionStrategy
{
    /// <summary>Start at the newly generated solution and move toward the elite guide.</summary>
    Forward = 0,

    /// <summary>Start at the elite guide and move toward the newly generated solution.</summary>
    Backward = 1,

    /// <summary>Traverse a backward path and then a forward path.</summary>
    BackAndForward = 2,

    /// <summary>Alternately advance the two endpoints until the paths meet.</summary>
    Mixed = 3
}

/// <summary>Move-selection policy inside a target-directed restricted neighborhood.</summary>
public enum PathRelinkingMoveSelectionStrategy
{
    /// <summary>Select the best objective move at each path position.</summary>
    Greedy = 0,

    /// <summary>
    /// Build a GRASP-style restricted candidate list from probed move objectives and
    /// sample one eligible move uniformly.
    /// </summary>
    GreedyRandomizedAdaptive = 1
}

/// <summary>
/// Orthogonal execution policies for path relinking. Truncation is represented by
/// <see cref="PathFraction"/> because the literature applies it to multiple directions.
/// </summary>
public readonly record struct PathRelinkingExecutionOptions(
    PathRelinkingDirectionStrategy Direction,
    PathRelinkingMoveSelectionStrategy MoveSelection,
    double PathFraction,
    double GreedyRandomizedAlpha)
{
    /// <summary>Canonical v0.30.x behavior.</summary>
    public static PathRelinkingExecutionOptions CanonicalForward { get; } =
        new(
            PathRelinkingDirectionStrategy.Forward,
            PathRelinkingMoveSelectionStrategy.Greedy,
            PathFraction: 1.0,
            GreedyRandomizedAlpha: 0.2);

    /// <summary>Whether this configuration is exactly the v0.30.x greedy forward policy.</summary>
    public bool IsCanonicalGreedyForward =>
        Direction == PathRelinkingDirectionStrategy.Forward &&
        MoveSelection == PathRelinkingMoveSelectionStrategy.Greedy &&
        PathFraction == 1.0;

    /// <summary>Validates the execution policy.</summary>
    public void Validate()
    {
        if (!Enum.IsDefined(Direction))
        {
            throw new ArgumentOutOfRangeException(nameof(Direction));
        }

        if (!Enum.IsDefined(MoveSelection))
        {
            throw new ArgumentOutOfRangeException(nameof(MoveSelection));
        }

        if (!double.IsFinite(PathFraction) ||
            PathFraction <= 0.0 ||
            PathFraction > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(PathFraction));
        }

        if (!double.IsFinite(GreedyRandomizedAlpha) ||
            GreedyRandomizedAlpha < 0.0 ||
            GreedyRandomizedAlpha > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(GreedyRandomizedAlpha));
        }
    }
}