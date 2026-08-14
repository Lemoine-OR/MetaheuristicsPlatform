using MetaheuristicsPlatform.Algorithms.DE.Random;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// JADE parameter adaptation of Zhang and Sanderson (2009).
/// </summary>
/// <remarks>
/// For each target:
/// CR_i ~ Normal(mu_CR, sigma), clipped to [0,1].
/// F_i  ~ Cauchy(mu_F, sigma), resampled while &lt;= 0 and capped at 1.
///
/// After selection:
/// mu_CR = (1-c) mu_CR + c mean_A(S_CR)
/// mu_F  = (1-c) mu_F  + c mean_L(S_F)
///
/// DOI: 10.1109/TEVC.2009.2014613.
/// </remarks>
public sealed class JadeParameterAdaptationPolicy :
    IDeParameterAdaptationPolicy
{
    public JadeParameterAdaptationPolicy(
        double initialMeanDifferentialWeight = 0.5,
        double initialMeanCrossoverProbability = 0.5,
        double adaptationRate = 0.1,
        double distributionScale = 0.1)
    {
        ValidateUnitInterval(
            initialMeanDifferentialWeight,
            nameof(initialMeanDifferentialWeight));

        ValidateUnitInterval(
            initialMeanCrossoverProbability,
            nameof(initialMeanCrossoverProbability));

        if (initialMeanDifferentialWeight <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialMeanDifferentialWeight));
        }

        if (!double.IsFinite(adaptationRate) ||
            adaptationRate <= 0.0 ||
            adaptationRate > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(adaptationRate));
        }

        if (!double.IsFinite(distributionScale) ||
            distributionScale <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(distributionScale));
        }

        MeanDifferentialWeight =
            initialMeanDifferentialWeight;

        MeanCrossoverProbability =
            initialMeanCrossoverProbability;

        AdaptationRate =
            adaptationRate;

        DistributionScale =
            distributionScale;
    }

    public string Id =>
        "jade-zhang-sanderson-2009";

    public DeParameterAdaptationKind Kind =>
        DeParameterAdaptationKind.CurrentSuccessMean;

    public double MeanDifferentialWeight { get; private set; }

    public double MeanCrossoverProbability { get; private set; }

    public double AdaptationRate { get; }

    public double DistributionScale { get; }

    public void Initialize(
        DeParameterBuffers buffers,
        int activePopulationSize)
    {
        ArgumentNullException.ThrowIfNull(buffers);

        ValidateActivePopulation(
            buffers,
            activePopulationSize);

        var initial =
            new DeControlParameters(
                MeanDifferentialWeight,
                MeanCrossoverProbability);

        for (int target = 0;
             target < activePopulationSize;
             target++)
        {
            buffers.SetParent(
                target,
                in initial);

            buffers.SetTrial(
                target,
                in initial);
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
            var random =
                randomStreams.Get(target);

            double cr =
                Math.Clamp(
                    DeRandomDistributions.SampleNormal(
                        random,
                        MeanCrossoverProbability,
                        DistributionScale),
                    0.0,
                    1.0);

            double f;

            do
            {
                f =
                    DeRandomDistributions.SampleCauchy(
                        random,
                        MeanDifferentialWeight,
                        DistributionScale);
            }
            while (f <= 0.0 ||
                   double.IsNaN(f));

            if (f > 1.0 ||
                double.IsPositiveInfinity(f))
            {
                f = 1.0;
            }

            var trial =
                new DeControlParameters(
                    f,
                    cr);

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

        int successes = 0;
        double sumCr = 0.0;
        double sumF = 0.0;
        double sumFSquared = 0.0;

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

            if (!item.Accepted)
            {
                continue;
            }

            DeControlParameters successful =
                buffers.GetTrial(
                    target);

            successes++;

            sumCr +=
                successful.CrossoverProbability;

            sumF +=
                successful.DifferentialWeight;

            sumFSquared +=
                successful.DifferentialWeight *
                successful.DifferentialWeight;
        }

        if (successes == 0)
        {
            return;
        }

        double arithmeticMeanCr =
            sumCr / successes;

        double lehmerMeanF =
            sumFSquared / sumF;

        MeanCrossoverProbability =
            (1.0 - AdaptationRate) *
                MeanCrossoverProbability +
            AdaptationRate *
                arithmeticMeanCr;

        MeanDifferentialWeight =
            (1.0 - AdaptationRate) *
                MeanDifferentialWeight +
            AdaptationRate *
                lehmerMeanF;
    }

    private static void ValidateUnitInterval(
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