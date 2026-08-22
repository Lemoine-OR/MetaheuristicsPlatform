using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.CMAES;

/// <summary>Parameters shared by IPOP-CMA-ES and BIPOP-CMA-ES.</summary>
public sealed class RestartCmaEsParameters : IMetaheuristicParameters
{
    /// <summary>
    /// Population size of the initial run. Zero selects
    /// 4 + floor(3 ln(n)).
    /// </summary>
    public int InitialPopulationSize { get; init; }

    /// <summary>
    /// Number of restarts after the initial run.
    /// Total run count is MaximumRestarts + 1.
    /// </summary>
    public int MaximumRestarts { get; init; } = 5;

    /// <summary>
    /// Local generation limit of one CMA-ES run before a restart is requested.
    /// The global stopping criterion remains authoritative and may stop earlier.
    /// </summary>
    public int MaximumGenerationsPerRestart { get; init; } = 200;

    /// <summary>
    /// IPOP population multiplier. The canonical setting is 2.
    /// </summary>
    public double PopulationMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Optional mean of the first run. Null selects the center of the box.
    /// Restart means are sampled uniformly in the bounded search space.
    /// </summary>
    public double[]? InitialMean { get; init; }

    /// <summary>
    /// Optional default initial step size. Null selects 0.3 times
    /// the root-mean-square box width.
    /// </summary>
    public double? InitialStepSize { get; init; }

    /// <summary>
    /// Numerical covariance eigenvalue floor.
    /// </summary>
    public double MinimumCovarianceEigenvalue { get; init; } = 1e-30;

    /// <summary>
    /// A completed generation requests a restart when the estimated covariance
    /// condition number reaches this threshold.
    /// </summary>
    public double RestartConditionNumberThreshold { get; init; } = 1e14;

    public void Validate()
    {
        if (InitialPopulationSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(InitialPopulationSize));
        }

        if (InitialPopulationSize == 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(InitialPopulationSize),
                "Restart CMA-ES requires population size zero or at least two.");
        }

        if (MaximumRestarts < 0 || MaximumRestarts > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumRestarts));
        }

        if (MaximumGenerationsPerRestart <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumGenerationsPerRestart));
        }

        if (!double.IsFinite(PopulationMultiplier) ||
            PopulationMultiplier <= 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(PopulationMultiplier));
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

        if (!double.IsFinite(RestartConditionNumberThreshold) ||
            RestartConditionNumberThreshold <= 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RestartConditionNumberThreshold));
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
