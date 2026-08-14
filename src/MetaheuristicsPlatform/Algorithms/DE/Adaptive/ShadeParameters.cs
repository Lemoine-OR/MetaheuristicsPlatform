using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Canonical SHADE configuration.
/// </summary>
public sealed class ShadeParameters :
    IMetaheuristicParameters
{
    public int PopulationSize { get; init; } =
        100;

    /// <summary>
    /// Canonical SHADE p-best fraction.
    /// </summary>
    public double PBestFraction { get; init; } =
        0.2;

    /// <summary>
    /// Number H of historical F/CR memory entries.
    /// </summary>
    public int MemorySize { get; init; } =
        100;

    public double InitialMemoryValue { get; init; } =
        0.5;

    public double DistributionScale { get; init; } =
        0.1;

    /// <summary>
    /// SHADE uses the external archive in its canonical configuration.
    /// This switch exists for controlled ablation experiments.
    /// </summary>
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

        if (MemorySize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(MemorySize));
        }

        ArgumentNullException.ThrowIfNull(
            VariationExecution);

        ArgumentNullException.ThrowIfNull(
            EvaluationExecution);

        VariationExecution.Validate();
        EvaluationExecution.Validate();

        _ =
            CreateAdaptationPolicy();
    }

    internal ShadeParameterAdaptationPolicy
        CreateAdaptationPolicy() =>
        new(
            MemorySize,
            InitialMemoryValue,
            DistributionScale);
}