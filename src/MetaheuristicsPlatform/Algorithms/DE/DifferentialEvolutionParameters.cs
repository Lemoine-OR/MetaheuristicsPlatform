using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Parameters;

namespace MetaheuristicsPlatform.Algorithms.DE;

/// <summary>
/// Fixed-parameter classical Differential Evolution configuration.
/// </summary>
public sealed class DifferentialEvolutionParameters :
    IMetaheuristicParameters
{
    public int PopulationSize { get; init; } =
        50;

    /// <summary>Differential weight F.</summary>
    public double DifferentialWeight { get; init; } =
        0.5;

    /// <summary>Crossover probability CR.</summary>
    public double CrossoverProbability { get; init; } =
        0.9;

    public DeMutationStrategy MutationStrategy { get; init; } =
        DeMutationStrategy.Rand1;

    public DeCrossoverStrategy CrossoverStrategy { get; init; } =
        DeCrossoverStrategy.Binomial;

    public DeBoundaryHandling BoundaryHandling { get; init; } =
        DeBoundaryHandling.Clamp;

    public DeExecutionOptions VariationExecution { get; init; } =
        new();

    public EvaluationExecutionOptions EvaluationExecution { get; init; } =
        new();

    public void Validate()
    {
        if (PopulationSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PopulationSize));
        }

        if (!double.IsFinite(DifferentialWeight) ||
            DifferentialWeight <= 0.0 ||
            DifferentialWeight > 2.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(DifferentialWeight),
                "F must be finite and in (0, 2].");
        }

        if (!double.IsFinite(CrossoverProbability) ||
            CrossoverProbability < 0.0 ||
            CrossoverProbability > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(CrossoverProbability),
                "CR must be finite and in [0, 1].");
        }

        int minimum =
            MinimumPopulationSizeFor(
                MutationStrategy);

        if (PopulationSize < minimum)
        {
            throw new ArgumentOutOfRangeException(
                nameof(PopulationSize),
                $"Mutation strategy {MutationStrategy} requires at least {minimum} individuals.");
        }

        ArgumentNullException.ThrowIfNull(
            VariationExecution);

        ArgumentNullException.ThrowIfNull(
            EvaluationExecution);

        VariationExecution.Validate();
        EvaluationExecution.Validate();
    }

    public static int MinimumPopulationSizeFor(
        DeMutationStrategy strategy) =>
        strategy switch
        {
            DeMutationStrategy.Rand1 => 4,
            DeMutationStrategy.Best1 => 4,
            DeMutationStrategy.CurrentToBest1 => 4,
            DeMutationStrategy.Rand2 => 6,
            _ => throw new ArgumentOutOfRangeException(
                nameof(strategy))
        };
}