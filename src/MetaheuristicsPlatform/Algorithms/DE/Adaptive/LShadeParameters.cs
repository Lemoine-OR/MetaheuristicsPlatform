using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// L-SHADE configuration using the tuned settings reported by
/// Tanabe and Fukunaga (CEC 2014).
/// </summary>
public sealed class LShadeParameters :
    IMetaheuristicParameters
{
    /// <summary>
    /// Explicit N_init. Zero resolves to round(D * InitialPopulationSizeMultiplier).
    /// </summary>
    public int InitialPopulationSize { get; init; }

    public double InitialPopulationSizeMultiplier { get; init; } =
        18.0;

    public int MinimumPopulationSize { get; init; } =
        4;

    public double ArchiveSizeRatio { get; init; } =
        2.6;

    public double PBestFraction { get; init; } =
        0.11;

    public int MemorySize { get; init; } =
        6;

    public double InitialMemoryValue { get; init; } =
        0.5;

    public double DistributionScale { get; init; } =
        0.1;

    /// <summary>
    /// Explicit MAX_NFE. Zero resolves to
    /// FunctionEvaluationsPerDimension * D.
    /// </summary>
    public long MaximumFunctionEvaluations { get; init; }

    public long FunctionEvaluationsPerDimension { get; init; } =
        10_000;

    public bool UseExternalArchive { get; init; } =
        true;

    public JadeBoundaryHandling BoundaryHandling { get; init; } =
        JadeBoundaryHandling.MidpointToTarget;

    public DeExecutionOptions VariationExecution { get; init; } =
        new();

    public EvaluationExecutionOptions EvaluationExecution { get; init; } =
        new();

    public void Validate()
    {
        if (InitialPopulationSize < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(InitialPopulationSize));
        }

        if (!double.IsFinite(
                InitialPopulationSizeMultiplier) ||
            InitialPopulationSizeMultiplier <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(InitialPopulationSizeMultiplier));
        }

        if (MinimumPopulationSize < 4)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MinimumPopulationSize),
                "current-to-pbest/1 requires at least four individuals.");
        }

        if (!double.IsFinite(ArchiveSizeRatio) ||
            ArchiveSizeRatio <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(ArchiveSizeRatio));
        }

        if (!double.IsFinite(PBestFraction) ||
            PBestFraction <= 0.0 ||
            PBestFraction > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PBestFraction));
        }

        if (MemorySize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MemorySize));
        }

        if (MaximumFunctionEvaluations < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MaximumFunctionEvaluations));
        }

        if (FunctionEvaluationsPerDimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(FunctionEvaluationsPerDimension));
        }

        ArgumentNullException.ThrowIfNull(
            VariationExecution);

        ArgumentNullException.ThrowIfNull(
            EvaluationExecution);

        VariationExecution.Validate();
        EvaluationExecution.Validate();

        _ =
            new LShadeParameterAdaptationPolicy(
                MemorySize,
                InitialMemoryValue,
                DistributionScale);
    }

    public int ResolveInitialPopulationSize(
        int dimension)
    {
        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        int resolved =
            InitialPopulationSize > 0
                ? InitialPopulationSize
                : checked(
                    (int)Math.Round(
                        dimension *
                        InitialPopulationSizeMultiplier,
                        MidpointRounding.AwayFromZero));

        if (resolved < MinimumPopulationSize)
        {
            throw new InvalidOperationException(
                "Resolved initial population is smaller than the minimum population.");
        }

        return resolved;
    }

    public long ResolveMaximumFunctionEvaluations(
        int dimension)
    {
        if (dimension <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(dimension));
        }

        return MaximumFunctionEvaluations > 0
            ? MaximumFunctionEvaluations
            : checked(
                FunctionEvaluationsPerDimension *
                dimension);
    }

    public int ResolveArchiveCapacity(
        int populationSize)
    {
        if (populationSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(populationSize));
        }

        return Math.Max(
            1,
            checked(
                (int)Math.Round(
                    populationSize *
                    ArchiveSizeRatio,
                    MidpointRounding.AwayFromZero)));
    }
}