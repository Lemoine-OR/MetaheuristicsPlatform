using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Canonical JADE configuration.
/// </summary>
public sealed class JadeParameters :
    IMetaheuristicParameters
{
    public int PopulationSize { get; init; } =
        100;

    /// <summary>
    /// Fraction p of the population eligible for random p-best selection.
    /// </summary>
    public double PBestFraction { get; init; } =
        0.05;

    /// <summary>c in the JADE mean-parameter updates.</summary>
    public double AdaptationRate { get; init; } =
        0.1;

    public double InitialMeanDifferentialWeight { get; init; } =
        0.5;

    public double InitialMeanCrossoverProbability { get; init; } =
        0.5;

    public double DistributionScale { get; init; } =
        0.1;

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
        if (PopulationSize < 4)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PopulationSize));
        }

        if (!double.IsFinite(PBestFraction) ||
            PBestFraction <= 0.0 ||
            PBestFraction > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PBestFraction));
        }

        if (PopulationSize * PBestFraction < 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PBestFraction),
                "p must identify at least one p-best candidate.");
        }

        ArgumentNullException.ThrowIfNull(
            VariationExecution);

        ArgumentNullException.ThrowIfNull(
            EvaluationExecution);

        VariationExecution.Validate();
        EvaluationExecution.Validate();

        _ = CreateAdaptationPolicy();
    }

    internal JadeParameterAdaptationPolicy CreateAdaptationPolicy() =>
        new(
            InitialMeanDifferentialWeight,
            InitialMeanCrossoverProbability,
            AdaptationRate,
            DistributionScale);
}