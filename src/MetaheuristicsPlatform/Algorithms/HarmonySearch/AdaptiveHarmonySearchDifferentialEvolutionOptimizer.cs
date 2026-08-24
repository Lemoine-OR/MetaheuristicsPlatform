using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.HarmonySearch;

public sealed class AdaptiveHarmonySearchDifferentialEvolutionOptimizer :
    IMetaheuristic<double[], AdaptiveHarmonySearchDifferentialEvolutionParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = MetaheuristicAlgorithmIds.AdaptiveHarmonySearchDifferentialEvolution,
            Name = "Adaptive Harmony Search with Differential Evolution",
            Acronym = "aHSDE",
            SolutionModel = MetaheuristicSolutionModel.Population,
            Families = MetaheuristicFamily.Other,
            Mechanisms =
                MetaheuristicMechanism.MemoryBased |
                MetaheuristicMechanism.Adaptive |
                MetaheuristicMechanism.Hybrid,
            SearchSpaces = SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = [ HarmonySearchReferences.ZhaoLiHaoLiuYuan2020 ]
        };

    public AdaptiveHarmonySearchDifferentialEvolutionParameters
        CreateDefaultParameters() => new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        AdaptiveHarmonySearchDifferentialEvolutionParameters parameters,
        ISolutionCloner<double[]> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<double[]>? callback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(solutionCloner);
        ArgumentNullException.ThrowIfNull(stoppingCriterion);
        parameters.Validate();

        if (problem.Sense != OptimizationSense.Minimize)
        {
            throw new NotSupportedException(
                "Published aHSDE learning/improvement equations are implemented for minimization.");
        }

        if (problem is not ISpanContinuousOptimizationProblem continuousProblem)
        {
            throw new NotSupportedException(
                "aHSDE requires ISpanContinuousOptimizationProblem.");
        }

        IBoundedContinuousSearchSpace searchSpace = continuousProblem.SearchSpace;
        int dimension = searchSpace.Dimension;
        int maximumHms =
            Math.Max(
                parameters.MinimumHarmonyMemorySize,
                parameters.MaximumHarmonyMemorySizePerDimension * dimension);

        int initialHms = Math.Max(parameters.HarmonyMemorySize, maximumHms);
        double[][] memory = new double[initialHms][];
        double[] fitness = new double[initialHms];
        for (int i = 0; i < initialHms; i++)
        {
            memory[i] = new double[dimension];
        }

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        HarmonySearchState state =
            new(
                Iteration: 0,
                Phase: HarmonySearchPhase.Initialization,
                HarmonyMemorySize: initialHms,
                TotalImprovisations: 0,
                ReplacedWorstHarmony: false,
                HarmonyMemoryConsiderationRate: parameters.HarmonyMemoryConsiderationRate,
                PitchAdjustmentRate: parameters.InitialPitchAdjustmentRateMean,
                PitchAdjustmentBandwidth: parameters.PitchAdjustmentBandwidth,
                MemoryBestFitness: null,
                MemoryWorstFitness: null);
        context.Start(state);

        int nfe = 0;
        for (int i = 0; i < initialHms; i++)
        {
            searchSpace.Sample(context.Random, memory[i]);
            fitness[i] = context.Evaluate(memory[i], state);
            RequireFiniteObjective(fitness[i]);
            nfe++;

            StoppingDecision initStop = context.EvaluateStopping(state);
            if (initStop.ShouldStop)
            {
                return context.Complete(initStop, state);
            }
        }

        int activeHms = initialHms;
        int maxNfe =
            parameters.MaximumFunctionEvaluationsPerDimension * dimension;
        double parMean = parameters.InitialPitchAdjustmentRateMean;
        double fMean = parameters.InitialScaleFactorMean;

        var successfulPar = new List<double>();
        var successfulF = new List<double>();
        var successfulDelta = new List<double>();
        double[] candidate = new double[dimension];

        for (int improvisation = 1;
             improvisation <= parameters.MaximumImprovisations &&
             nfe < maxNfe;
             improvisation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double par = ClampAdaptiveSample(
                NextGaussian(context.Random, parMean, 0.1));
            double scale = ClampAdaptiveSample(
                NextGaussian(context.Random, fMean, 0.1));

            int best = FindBestIndex(fitness, activeHms);
            int worst = FindWorstIndex(fitness, activeHms);

            state =
                new HarmonySearchState(
                    Iteration: improvisation - 1,
                    Phase: HarmonySearchPhase.Improvisation,
                    HarmonyMemorySize: activeHms,
                    TotalImprovisations: improvisation - 1,
                    ReplacedWorstHarmony: false,
                    HarmonyMemoryConsiderationRate: parameters.HarmonyMemoryConsiderationRate,
                    PitchAdjustmentRate: par,
                    PitchAdjustmentBandwidth: parameters.PitchAdjustmentBandwidth,
                    MemoryBestFitness: fitness[best],
                    MemoryWorstFitness: fitness[worst]);

            Improvise(
                memory,
                activeHms,
                candidate,
                best,
                parameters.HarmonyMemoryConsiderationRate,
                par,
                scale,
                parameters.PitchAdjustmentBandwidth,
                searchSpace,
                context.Random);

            double candidateFitness = context.Evaluate(candidate, state);
            RequireFiniteObjective(candidateFitness);
            nfe++;

            worst = FindWorstIndex(fitness, activeHms);
            double oldWorst = fitness[worst];
            bool replaced = candidateFitness < oldWorst;
            if (replaced)
            {
                candidate.AsSpan().CopyTo(memory[worst]);
                fitness[worst] = candidateFitness;
                successfulPar.Add(par);
                successfulF.Add(scale);
                successfulDelta.Add(oldWorst - candidateFitness);
            }

            if (improvisation % parameters.LearningPeriod == 0 &&
                successfulDelta.Count > 0)
            {
                parMean = WeightedLehmerMean(successfulPar, successfulDelta);
                fMean = WeightedLehmerMean(successfulF, successfulDelta);
                successfulPar.Clear();
                successfulF.Clear();
                successfulDelta.Clear();
            }

            int targetHms =
                (int)Math.Round(
                    maximumHms -
                    ((maximumHms - parameters.MinimumHarmonyMemorySize) *
                     ((double)nfe / maxNfe)),
                    MidpointRounding.AwayFromZero);

            targetHms =
                Math.Clamp(
                    targetHms,
                    parameters.MinimumHarmonyMemorySize,
                    maximumHms);

            while (activeHms > targetHms)
            {
                int remove = FindWorstIndex(fitness, activeHms);
                int last = activeHms - 1;
                if (remove != last)
                {
                    memory[remove] = memory[last];
                    fitness[remove] = fitness[last];
                }
                activeHms--;
            }

            best = FindBestIndex(fitness, activeHms);
            worst = FindWorstIndex(fitness, activeHms);

            state =
                new HarmonySearchState(
                    Iteration: improvisation,
                    Phase: HarmonySearchPhase.CompletedImprovisation,
                    HarmonyMemorySize: activeHms,
                    TotalImprovisations: improvisation,
                    ReplacedWorstHarmony: replaced,
                    HarmonyMemoryConsiderationRate: parameters.HarmonyMemoryConsiderationRate,
                    PitchAdjustmentRate: par,
                    PitchAdjustmentBandwidth: parameters.PitchAdjustmentBandwidth,
                    MemoryBestFitness: fitness[best],
                    MemoryWorstFitness: fitness[worst]);

            context.CompleteIteration(fitness[best], state);
            StoppingDecision stop = context.EvaluateStopping(state);
            if (stop.ShouldStop)
            {
                return context.Complete(stop, state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumAdaptiveHarmonySearchDifferentialEvolutionBudget",
                "The configured aHSDE scientific NFE/improvisation budget was reached."),
            state);
    }

    private static void Improvise(
        double[][] memory,
        int activeHms,
        Span<double> candidate,
        int bestIndex,
        double hmcr,
        double par,
        double scale,
        double bandwidth,
        IBoundedContinuousSearchSpace searchSpace,
        IRandomSource random)
    {
        ReadOnlySpan<double> lower = searchSpace.LowerBounds;
        ReadOnlySpan<double> upper = searchSpace.UpperBounds;

        int[] r = SelectFourDistinct(activeHms, random);

        for (int i = 0; i < candidate.Length; i++)
        {
            if (random.NextDouble() < hmcr)
            {
                candidate[i] =
                    memory[random.NextInt32(activeHms)][i];

                if (random.NextDouble() < par)
                {
                    // Zhao et al. (2020), DE/best/2-based pitch adjustment.
                    candidate[i] =
                        memory[bestIndex][i] +
                        scale *
                        ((memory[r[0]][i] - memory[r[1]][i]) +
                         (memory[r[2]][i] - memory[r[3]][i])) +
                        (((2.0 * random.NextDouble()) - 1.0) * bandwidth);
                }
            }
            else
            {
                candidate[i] =
                    lower[i] +
                    random.NextDouble() * (upper[i] - lower[i]);
            }
        }

        searchSpace.Clamp(candidate);
    }

    private static int[] SelectFourDistinct(
        int count,
        IRandomSource random)
    {
        if (count < 5)
        {
            throw new InvalidOperationException(
                "aHSDE requires at least five active harmonies.");
        }

        int[] result = new int[4];
        for (int i = 0; i < result.Length; i++)
        {
            int value;
            bool duplicate;
            do
            {
                value = random.NextInt32(count);
                duplicate = false;
                for (int j = 0; j < i; j++)
                {
                    if (result[j] == value)
                    {
                        duplicate = true;
                        break;
                    }
                }
            }
            while (duplicate);
            result[i] = value;
        }
        return result;
    }

    private static double NextGaussian(
        IRandomSource random,
        double mean,
        double standardDeviation)
    {
        double u1 = Math.Max(random.NextDouble(), double.Epsilon);
        double u2 = random.NextDouble();
        double z =
            Math.Sqrt(-2.0 * Math.Log(u1)) *
            Math.Cos(2.0 * Math.PI * u2);
        return mean + (standardDeviation * z);
    }

    private static double ClampAdaptiveSample(double value)
    {
        if (value > 1.0)
        {
            return 1.0;
        }
        if (value <= 0.0)
        {
            return 0.001;
        }
        return value;
    }

    private static double WeightedLehmerMean(
        IReadOnlyList<double> samples,
        IReadOnlyList<double> improvements)
    {
        double improvementTotal = improvements.Sum();
        if (!(improvementTotal > 0.0))
        {
            return samples.Average();
        }

        double numerator = 0.0;
        double denominator = 0.0;
        for (int i = 0; i < samples.Count; i++)
        {
            double weight = improvements[i] / improvementTotal;
            numerator += weight * samples[i] * samples[i];
            denominator += weight * samples[i];
        }

        return denominator > 0.0
            ? numerator / denominator
            : samples.Average();
    }

    private static int FindBestIndex(double[] values, int count)
    {
        int index = 0;
        for (int i = 1; i < count; i++)
        {
            if (values[i] < values[index])
            {
                index = i;
            }
        }
        return index;
    }

    private static int FindWorstIndex(double[] values, int count)
    {
        int index = 0;
        for (int i = 1; i < count; i++)
        {
            if (values[i] > values[index])
            {
                index = i;
            }
        }
        return index;
    }

    private static void RequireFiniteObjective(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new InvalidOperationException(
                "aHSDE requires finite objective values.");
        }
    }
}
