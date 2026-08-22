using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>Parameters of the canonical full-covariance CMA-ES implementation.</summary>
public sealed class CmaEsParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Offspring population size lambda. Zero selects the canonical
    /// dimension-dependent default 4 + floor(3 ln(n)).
    /// </summary>
    public int PopulationSize { get; init; }

    /// <summary>
    /// Number of selected parents mu. Zero selects floor(lambda / 2).
    /// </summary>
    public int ParentCount { get; init; }

    /// <summary>Maximum number of complete generations.</summary>
    public int MaximumGenerations { get; init; } = 1000;

    /// <summary>
    /// Optional initial mean. Null selects the center of the bounded box.
    /// </summary>
    public double[]? InitialMean { get; init; }

    /// <summary>
    /// Optional initial global step size sigma. Null selects 0.3 times
    /// the root-mean-square box width.
    /// </summary>
    public double? InitialStepSize { get; init; }

    /// <summary>
    /// Smallest eigenvalue retained by the numerical covariance
    /// eigendecomposition.
    /// </summary>
    public double MinimumCovarianceEigenvalue { get; init; } = 1e-30;

    public void Validate()
    {
        if (PopulationSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(PopulationSize));
        }

        if (ParentCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ParentCount));
        }

        if (PopulationSize > 0 &&
            ParentCount > PopulationSize)
        {
            throw new ArgumentOutOfRangeException(nameof(ParentCount));
        }

        if (MaximumGenerations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumGenerations));
        }

        if (InitialStepSize.HasValue &&
            (!double.IsFinite(InitialStepSize.Value) ||
             InitialStepSize.Value <= 0.0))
        {
            throw new ArgumentOutOfRangeException(nameof(InitialStepSize));
        }

        if (!double.IsFinite(MinimumCovarianceEigenvalue) ||
            MinimumCovarianceEigenvalue <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumCovarianceEigenvalue));
        }

        if (InitialMean is not null)
        {
            for (int i = 0; i < InitialMean.Length; i++)
            {
                if (!double.IsFinite(InitialMean[i]))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(InitialMean),
                        $"InitialMean[{i}] must be finite.");
                }
            }
        }
    }
}
