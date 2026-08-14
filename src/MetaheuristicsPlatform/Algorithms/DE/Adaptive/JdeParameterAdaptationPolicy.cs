using MetaheuristicsPlatform.Algorithms.DE.Random;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Brest et al. self-adaptive control-parameter policy (jDE).
/// </summary>
/// <remarks>
/// Each individual carries its own F and CR values.
///
/// Before mutation:
/// F_trial = F_lower + rand1 * F_range when rand2 &lt; tau1,
/// otherwise F_parent.
///
/// CR_trial = rand3 when rand4 &lt; tau2,
/// otherwise CR_parent.
///
/// Trial F/CR are inherited only when the corresponding trial solution is accepted.
///
/// Reference:
/// J. Brest, S. Greiner, B. Boskovic, M. Mernik, V. Zumer,
/// IEEE Transactions on Evolutionary Computation 10(6), 646-657, 2006.
/// DOI: 10.1109/TEVC.2006.872133.
/// </remarks>
public sealed class JdeParameterAdaptationPolicy :
    IDeParameterAdaptationPolicy
{
    private readonly DeControlParameters _initialParameters;

    public JdeParameterAdaptationPolicy(
        double initialDifferentialWeight = 0.5,
        double initialCrossoverProbability = 0.9,
        double differentialWeightLowerBound = 0.1,
        double differentialWeightRange = 0.9,
        double differentialWeightAdaptationProbability = 0.1,
        double crossoverAdaptationProbability = 0.1)
    {
        _initialParameters =
            new DeControlParameters(
                initialDifferentialWeight,
                initialCrossoverProbability);

        _initialParameters.Validate();

        if (!double.IsFinite(
                differentialWeightLowerBound) ||
            differentialWeightLowerBound <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(differentialWeightLowerBound));
        }

        if (!double.IsFinite(
                differentialWeightRange) ||
            differentialWeightRange <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(differentialWeightRange));
        }

        double maximumDifferentialWeight =
            differentialWeightLowerBound +
            differentialWeightRange;

        if (maximumDifferentialWeight > 2.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(differentialWeightRange),
                "The maximum generated differential weight must not exceed 2.");
        }

        ValidateProbability(
            differentialWeightAdaptationProbability,
            nameof(differentialWeightAdaptationProbability));

        ValidateProbability(
            crossoverAdaptationProbability,
            nameof(crossoverAdaptationProbability));

        DifferentialWeightLowerBound =
            differentialWeightLowerBound;

        DifferentialWeightRange =
            differentialWeightRange;

        DifferentialWeightAdaptationProbability =
            differentialWeightAdaptationProbability;

        CrossoverAdaptationProbability =
            crossoverAdaptationProbability;
    }

    public string Id =>
        "jde-brest-2006";

    public DeParameterAdaptationKind Kind =>
        DeParameterAdaptationKind.SelfAdaptive;

    public double DifferentialWeightLowerBound { get; }

    public double DifferentialWeightRange { get; }

    public double DifferentialWeightAdaptationProbability { get; }

    public double CrossoverAdaptationProbability { get; }

    public DeControlParameters InitialParameters =>
        _initialParameters;

    public void Initialize(
        DeParameterBuffers buffers,
        int activePopulationSize)
    {
        ArgumentNullException.ThrowIfNull(buffers);

        ValidateActivePopulation(
            buffers,
            activePopulationSize);

        for (int target = 0;
             target < activePopulationSize;
             target++)
        {
            buffers.SetParent(
                target,
                in _initialParameters);

            buffers.SetTrial(
                target,
                in _initialParameters);
        }
    }

    public void PrepareGeneration(
        in DeGenerationAdaptationContext context,
        DeParameterBuffers buffers,
        DeTargetRandomStreams randomStreams)
    {
        ArgumentNullException.ThrowIfNull(buffers);
        ArgumentNullException.ThrowIfNull(randomStreams);

        ValidateActivePopulation(
            buffers,
            context.ActivePopulationSize);

        for (int target = 0;
             target < context.ActivePopulationSize;
             target++)
        {
            DeControlParameters parent =
                buffers.GetParent(
                    target);

            var random =
                randomStreams.Get(
                    target);

            // Consume the four paper variables in stable order.
            double rand1 =
                random.NextDouble();

            double rand2 =
                random.NextDouble();

            double rand3 =
                random.NextDouble();

            double rand4 =
                random.NextDouble();

            double trialF =
                rand2 <
                    DifferentialWeightAdaptationProbability
                    ? DifferentialWeightLowerBound +
                        rand1 *
                        DifferentialWeightRange
                    : parent.DifferentialWeight;

            double trialCr =
                rand4 <
                    CrossoverAdaptationProbability
                    ? rand3
                    : parent.CrossoverProbability;

            var trial =
                new DeControlParameters(
                    trialF,
                    trialCr);

            buffers.SetTrial(
                target,
                in trial);
        }
    }

    public void CompleteGeneration(
        in DeGenerationAdaptationContext context,
        DeParameterBuffers buffers,
        ReadOnlySpan<DeSelectionFeedback> feedback)
    {
        ArgumentNullException.ThrowIfNull(buffers);

        ValidateActivePopulation(
            buffers,
            context.ActivePopulationSize);

        if (feedback.Length !=
            context.ActivePopulationSize)
        {
            throw new ArgumentException(
                "Feedback length must match the active population size.",
                nameof(feedback));
        }

        for (int target = 0;
             target < feedback.Length;
             target++)
        {
            ref readonly DeSelectionFeedback item =
                ref feedback[target];

            if (item.TargetIndex != target)
            {
                throw new ArgumentException(
                    "Selection feedback must be ordered by target index.",
                    nameof(feedback));
            }

            if (item.Accepted)
            {
                buffers.AcceptTrial(
                    target);
            }
        }
    }

    private static void ValidateProbability(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value) ||
            value < 0.0 ||
            value > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName);
        }
    }

    private static void ValidateActivePopulation(
        DeParameterBuffers buffers,
        int activePopulationSize)
    {
        if (activePopulationSize <= 0 ||
            activePopulationSize > buffers.Capacity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(activePopulationSize));
        }
    }
}