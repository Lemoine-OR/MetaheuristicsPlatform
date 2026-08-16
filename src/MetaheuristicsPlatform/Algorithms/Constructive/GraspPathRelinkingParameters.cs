using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>Parameters for GRASP with an elite pool and configurable path relinking.</summary>
public sealed class GraspPathRelinkingParameters : IMetaheuristicParameters
{
    /// <summary>Maximum number of complete GRASP outer iterations.</summary>
    public int MaximumIterations { get; set; } = 100;

    /// <summary>Canonical threshold-RCL alpha in [0,1].</summary>
    public double Alpha { get; set; } = 0.2;

    /// <summary>Safety limit for one randomized greedy construction.</summary>
    public int MaximumConstructionSteps { get; set; } = int.MaxValue;

    /// <summary>Maximum number of solutions retained in the elite pool.</summary>
    public int ElitePoolSize { get; set; } = 10;

    /// <summary>
    /// Minimum integral path distance between distinct retained elite solutions.
    /// A value of one removes only exact duplicates.
    /// </summary>
    public int MinimumEliteDistance { get; set; } = 1;

    /// <summary>Safety limit for one directional or mixed relinking trajectory.</summary>
    public int MaximumPathSteps { get; set; } = int.MaxValue;

    /// <summary>Forward, backward, back-and-forward or mixed endpoint policy.</summary>
    public PathRelinkingDirectionStrategy PathDirection { get; set; } =
        PathRelinkingDirectionStrategy.Forward;

    /// <summary>Greedy or greedy-randomized adaptive move selection.</summary>
    public PathRelinkingMoveSelectionStrategy PathMoveSelection { get; set; } =
        PathRelinkingMoveSelectionStrategy.Greedy;

    /// <summary>
    /// Fraction of the initial path distance to eliminate. One explores the full path;
    /// values below one implement truncated path relinking.
    /// </summary>
    public double PathFraction { get; set; } = 1.0;

    /// <summary>
    /// RCL alpha for greedy-randomized adaptive path relinking. Zero keeps only best
    /// target-directed moves; one admits the full target-directed move set.
    /// </summary>
    public double PathRelinkingAlpha { get; set; } = 0.2;

    /// <summary>Builds the execution policy consumed by an advanced relinking engine.</summary>
    public PathRelinkingExecutionOptions CreatePathRelinkingExecutionOptions() =>
        new(
            PathDirection,
            PathMoveSelection,
            PathFraction,
            PathRelinkingAlpha);

    /// <inheritdoc />
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

        CreatePathRelinkingExecutionOptions().Validate();
    }
}