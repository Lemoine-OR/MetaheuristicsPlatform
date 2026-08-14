using MetaheuristicsPlatform.Algorithms.DE.Random;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// Success-history based F/CR adaptation of Tanabe and Fukunaga (2013).
/// </summary>
/// <remarks>
/// A historical-memory index is sampled uniformly for each target.
///
/// CR is sampled from Normal(M_CR[r], sigma) and clipped to [0,1].
/// F is sampled from Cauchy(M_F[r], sigma), resampled while non-positive,
/// and capped at one.
///
/// Strictly successful trials update one circular memory entry:
/// - M_CR uses the improvement-weighted arithmetic mean;
/// - M_F uses the improvement-weighted Lehmer mean.
///
/// DOI: 10.1109/CEC.2013.6557555.
/// </remarks>
public sealed class ShadeParameterAdaptationPolicy :
    IDeParameterAdaptationPolicy
{
    private readonly ShadeSuccessHistoryMemory _memory;

    public ShadeParameterAdaptationPolicy(
        int memorySize = 100,
        double initialMemoryValue = 0.5,
        double distributionScale = 0.1)
    {
        if (!double.IsFinite(distributionScale) ||
            distributionScale <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(distributionScale));
        }

        _memory =
            new ShadeSuccessHistoryMemory(
                memorySize,
                initialMemoryValue);

        DistributionScale =
            distributionScale;
    }

    public string Id =>
        "shade-tanabe-fukunaga-2013";

    public DeParameterAdaptationKind Kind =>
        DeParameterAdaptationKind.SuccessHistory;

    public int MemorySize =>
        _memory.Capacity;

    public int MemoryPosition =>
        _memory.Position;

    public double DistributionScale { get; }

    public ShadeSuccessHistoryMemory Memory =>
        _memory;

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
                _memory.GetDifferentialWeight(0),
                _memory.GetCrossoverProbability(0));

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

            int memoryIndex =
                random.NextInt32(
                    _memory.Capacity);

            double memoryCr =
                _memory.GetCrossoverProbability(
                    memoryIndex);

            double memoryF =
                _memory.GetDifferentialWeight(
                    memoryIndex);

            double cr =
                Math.Clamp(
                    DeRandomDistributions.SampleNormal(
                        random,
                        memoryCr,
                        DistributionScale),
                    0.0,
                    1.0);

            double f;

            do
            {
                f =
                    DeRandomDistributions.SampleCauchy(
                        random,
                        memoryF,
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

        double totalImprovement = 0.0;
        int successCount = 0;

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

            if (!double.IsFinite(item.Improvement) ||
                item.Improvement <= 0.0)
            {
                throw new ArgumentException(
                    "A strict successful SHADE trial must have a positive finite improvement.",
                    nameof(feedback));
            }

            totalImprovement +=
                item.Improvement;

            successCount++;
        }

        if (successCount == 0)
        {
            return;
        }

        if (!double.IsFinite(totalImprovement) ||
            totalImprovement <= 0.0)
        {
            throw new InvalidOperationException(
                "SHADE success weights require positive finite total improvement.");
        }

        double weightedCr = 0.0;
        double weightedFSquared = 0.0;
        double weightedF = 0.0;

        for (int target = 0;
             target < feedback.Length;
             target++)
        {
            ref readonly DeSelectionFeedback item =
                ref feedback[target];

            if (!item.Accepted)
            {
                continue;
            }

            double weight =
                item.Improvement /
                totalImprovement;

            DeControlParameters successful =
                buffers.GetTrial(
                    target);

            double f =
                successful.DifferentialWeight;

            double cr =
                successful.CrossoverProbability;

            weightedCr +=
                weight * cr;

            weightedFSquared +=
                weight * f * f;

            weightedF +=
                weight * f;
        }

        double weightedLehmerF =
            weightedFSquared /
            weightedF;

        _memory.Update(
            Math.Clamp(
                weightedCr,
                0.0,
                1.0),
            Math.Clamp(
                weightedLehmerF,
                double.Epsilon,
                1.0));
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