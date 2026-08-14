using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Parameters of the Brest et al. self-adaptive Differential Evolution algorithm.
/// </summary>
public sealed class JdeParameters :
    IMetaheuristicParameters
{
    public int PopulationSize { get; init; } =
        100;

    public double InitialDifferentialWeight { get; init; } =
        0.5;

    public double InitialCrossoverProbability { get; init; } =
        0.9;

    /// <summary>F_l in the canonical update.</summary>
    public double DifferentialWeightLowerBound { get; init; } =
        0.1;

    /// <summary>
    /// Width multiplied by U(0,1) in the canonical update.
    /// The Brest et al. defaults 0.1 + U * 0.9 generate F in [0.1, 1.0).
    /// </summary>
    public double DifferentialWeightRange { get; init; } =
        0.9;

    /// <summary>tau1.</summary>
    public double DifferentialWeightAdaptationProbability { get; init; } =
        0.1;

    /// <summary>tau2.</summary>
    public double CrossoverAdaptationProbability { get; init; } =
        0.1;

    public DeBoundaryHandling BoundaryHandling { get; init; } =
        DeBoundaryHandling.Clamp;

    public DeExecutionOptions VariationExecution { get; init; } =
        new();

    public EvaluationExecutionOptions EvaluationExecution { get; init; } =
        new();

    public void Validate()
    {
        if (PopulationSize < 4)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PopulationSize),
                "Canonical jDE uses DE/rand/1/bin and therefore requires at least four individuals.");
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

    internal JdeParameterAdaptationPolicy CreateAdaptationPolicy() =>
        new(
            InitialDifferentialWeight,
            InitialCrossoverProbability,
            DifferentialWeightLowerBound,
            DifferentialWeightRange,
            DifferentialWeightAdaptationProbability,
            CrossoverAdaptationProbability);
}