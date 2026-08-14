using MetaheuristicsPlatform.Algorithms.DE.Random;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// SHADE 1.1 success-history parameter adaptation used by L-SHADE.
/// </summary>
/// <remarks>
/// Both successful CR and F values use improvement-weighted Lehmer means.
/// M_CR also supports the terminal value described in SHADE 1.1.
///
/// DOI: 10.1109/CEC.2014.6900380.
/// </remarks>
public sealed class LShadeParameterAdaptationPolicy :
    IDeParameterAdaptationPolicy
{
    private readonly LShadeSuccessHistoryMemory _memory;

    public LShadeParameterAdaptationPolicy(
        int memorySize = 6,
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
            new LShadeSuccessHistoryMemory(
                memorySize,
                initialMemoryValue);

        DistributionScale =
            distributionScale;
    }

    public string Id =>
        "lshade-success-history-2014";

    public DeParameterAdaptationKind Kind =>
        DeParameterAdaptationKind.SuccessHistory;

    public int MemorySize =>
        _memory.Capacity;

    public int MemoryPosition =>
        _memory.Position;

    public double DistributionScale { get; }

    public LShadeSuccessHistoryMemory Memory =>
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

            double cr;

            if (_memory.IsCrossoverTerminal(
                    memoryIndex))
            {
                cr = 0.0;
            }
            else
            {
                cr =
                    Math.Clamp(
                        DeRandomDistributions.SampleNormal(
                            random,
                            _memory.GetCrossoverProbability(
                                memoryIndex),
                            DistributionScale),
                        0.0,
                        1.0);
            }

            double f;

            do
            {
                f =
                    DeRandomDistributions.SampleCauchy(
                        random,
                        _memory.GetDifferentialWeight(
                            memoryIndex),
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

        if (feedback.Length <
            context.ActivePopulationSize)
        {
            throw new ArgumentException(
                "Feedback must cover the active population.",
                nameof(feedback));
        }

        int successCount = 0;
        double totalImprovement = 0.0;
        double maximumSuccessfulCr = 0.0;

        for (int target = 0;
             target < context.ActivePopulationSize;
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
                    "Strict successful trials require positive finite improvement.",
                    nameof(feedback));
            }

            DeControlParameters successful =
                buffers.GetTrial(
                    target);

            totalImprovement +=
                item.Improvement;

            maximumSuccessfulCr =
                Math.Max(
                    maximumSuccessfulCr,
                    successful.CrossoverProbability);

            successCount++;
        }

        if (successCount == 0)
        {
            return;
        }

        if (_memory.IsCrossoverTerminal(
                _memory.Position) ||
            maximumSuccessfulCr == 0.0)
        {
            _memory.UpdateTerminalCrossover();
            return;
        }

        double weightedCrSquared = 0.0;
        double weightedCr = 0.0;
        double weightedFSquared = 0.0;
        double weightedF = 0.0;

        for (int target = 0;
             target < context.ActivePopulationSize;
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

            double cr =
                successful.CrossoverProbability;

            double f =
                successful.DifferentialWeight;

            weightedCrSquared +=
                weight * cr * cr;

            weightedCr +=
                weight * cr;

            weightedFSquared +=
                weight * f * f;

            weightedF +=
                weight * f;
        }

        double weightedLehmerCr =
            weightedCrSquared /
            weightedCr;

        double weightedLehmerF =
            weightedFSquared /
            weightedF;

        _memory.Update(
            Math.Clamp(
                weightedLehmerCr,
                double.Epsilon,
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