using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>Parameters for GRASP with elite-memory path relinking.</summary>
public sealed class GraspPathRelinkingParameters : IMetaheuristicParameters
{
    public int MaximumIterations { get; set; } = 100;

    public double Alpha { get; set; } = 0.2;

    public int MaximumConstructionSteps { get; set; } = int.MaxValue;

    public int ElitePoolSize { get; set; } = 10;

    public int MinimumEliteDistance { get; set; } = 1;

    public int MaximumPathSteps { get; set; } = int.MaxValue;

    public PathRelinkingDirectionStrategy PathDirection { get; set; } =
        PathRelinkingDirectionStrategy.Forward;

    public PathRelinkingMoveSelectionStrategy PathMoveSelection { get; set; } =
        PathRelinkingMoveSelectionStrategy.Greedy;

    public double PathFraction { get; set; } = 1.0;

    public double PathRelinkingAlpha { get; set; } = 0.2;

    /// <summary>
    /// Enables the generational evolutionary path-relinking post-optimization phase.
    /// Disabled by default for strict v0.31.0 behavioral compatibility.
    /// </summary>
    public bool EvolutionaryPathRelinkingEnabled { get; set; }

    public int MaximumEvolutionaryGenerations { get; set; } = 10;

    public int MaximumEvolutionaryPathSteps { get; set; } = int.MaxValue;

    public bool ImproveEvolutionaryOffspring { get; set; } = true;

    /// <summary>
    /// Mixed is the efficient default because it explores both endpoint regions without
    /// the two complete traversals required by back-and-forward path relinking.
    /// </summary>
    public PathRelinkingDirectionStrategy EvolutionaryPathDirection { get; set; } =
        PathRelinkingDirectionStrategy.Mixed;

    /// <summary>
    /// Randomized adaptive selection is the default to avoid deterministic replay when
    /// elite pairs recur across evolutionary intensification.
    /// </summary>
    public PathRelinkingMoveSelectionStrategy EvolutionaryPathMoveSelection { get; set; } =
        PathRelinkingMoveSelectionStrategy.GreedyRandomizedAdaptive;

    public double EvolutionaryPathFraction { get; set; } = 1.0;

    public double EvolutionaryPathRelinkingAlpha { get; set; } = 0.2;

    public PathRelinkingExecutionOptions CreatePathRelinkingExecutionOptions() =>
        new(
            PathDirection,
            PathMoveSelection,
            PathFraction,
            PathRelinkingAlpha);

    public PathRelinkingExecutionOptions CreateEvolutionaryPathRelinkingExecutionOptions() =>
        new(
            EvolutionaryPathDirection,
            EvolutionaryPathMoveSelection,
            EvolutionaryPathFraction,
            EvolutionaryPathRelinkingAlpha);

    public void Validate()
    {
        if (MaximumIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumIterations));
        }

        if (!double.IsFinite(Alpha) || Alpha < 0.0 || Alpha > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(Alpha));
        }

        if (MaximumConstructionSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumConstructionSteps));
        }

        if (ElitePoolSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ElitePoolSize));
        }

        if (MinimumEliteDistance <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MinimumEliteDistance));
        }

        if (MaximumPathSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumPathSteps));
        }

        if (MaximumEvolutionaryGenerations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumEvolutionaryGenerations));
        }

        if (MaximumEvolutionaryPathSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumEvolutionaryPathSteps));
        }

        CreatePathRelinkingExecutionOptions().Validate();
        CreateEvolutionaryPathRelinkingExecutionOptions().Validate();
    }
}