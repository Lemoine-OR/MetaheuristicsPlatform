using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.Constructive;

/// <summary>Parameters for canonical GRASP.</summary>
public sealed class GraspParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Maximum number of complete construction + local-search GRASP iterations.
    /// </summary>
    public int MaximumIterations { get; set; } = 100;

    /// <summary>
    /// Threshold-RCL greediness/randomization parameter in [0,1].
    /// Alpha=0 keeps only greedily best ties; alpha=1 admits the complete candidate list.
    /// </summary>
    public double Alpha { get; set; } = 0.2;

    /// <summary>
    /// Safety limit for the number of components accepted during one construction phase.
    /// </summary>
    public int MaximumConstructionSteps { get; set; } = int.MaxValue;

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
    }
}
