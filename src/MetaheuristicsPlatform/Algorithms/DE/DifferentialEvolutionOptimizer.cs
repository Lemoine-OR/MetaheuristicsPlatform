using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Algorithms.DE.Random;
using MetaheuristicsPlatform.Algorithms.DE.State;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Evaluation;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.DE;

/// <summary>
/// High-performance synchronous/generational continuous Differential Evolution.
/// </summary>
public sealed class DifferentialEvolutionOptimizer :
    IMetaheuristic<double[], DifferentialEvolutionParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = "de-continuous",
            Name = "Differential Evolution",
            Acronym = "DE",
            SolutionModel =
                MetaheuristicSolutionModel.Population,
            Families =
                MetaheuristicFamily.Evolutionary,
            Mechanisms =
                MetaheuristicMechanism.EvolutionaryOperators,
            SearchSpaces =
                SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    DifferentialEvolutionReferences.StornPrice1997
                }
        };

    public DifferentialEvolutionParameters
        CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        DifferentialEvolutionParameters parameters,
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

        if (problem is not
            ISpanContinuousOptimizationProblem continuousProblem)
        {
            throw new NotSupportedException(
                "The high-performance continuous DE requires " +
                "ISpanContinuousOptimizationProblem.");
        }

        OptimizationOptions runtimeOptions =
            options ??
            new OptimizationOptions();

        runtimeOptions.Validate();

        int populationSize =
            parameters.PopulationSize;

        int dimension =
            continuousProblem.SearchSpace.Dimension;

        var buffers =
            new DePopulationBuffers(
                populationSize,
                dimension);

        var randomStreams =
            new DeTargetRandomStreams(
                populationSize,
                runtimeOptions.Seed,
                runtimeOptions.RandomSourceFactory);

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                runtimeOptions,
                callback,
                cancellationToken);

        var state =
            new DeIterationState(
                populationSize,
                dimension,
                parameters.MutationStrategy,
                parameters.CrossoverStrategy,
                AcceptedTrials: 0);

        context.Start(state);

        InitializePopulation(
            buffers,
            continuousProblem.SearchSpace,
            parameters.VariationExecution,
            randomStreams,
            cancellationToken);

        EvaluatePopulation(
            continuousProblem,
            parameters.EvaluationExecution,
            buffers,
            useTrialPopulation: false,
            cancellationToken);

        CommitPopulationEvaluations(
            context,
            buffers,
            useTrialPopulation: false);

        StoppingDecision stop =
            context.EvaluateStopping(
                state);

        if (stop.ShouldStop)
        {
            return context.Complete(
                stop,
                state);
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int bestIndex =
                FindBestIndex(
                    buffers.FitnessReadOnly,
                    problem.Sense);

            BuildTrialPopulation(
                buffers,
                continuousProblem.SearchSpace,
                parameters,
                randomStreams,
                bestIndex,
                cancellationToken);

            EvaluatePopulation(
                continuousProblem,
                parameters.EvaluationExecution,
                buffers,
                useTrialPopulation: true,
                cancellationToken);

            CommitPopulationEvaluations(
                context,
                buffers,
                useTrialPopulation: true);

            int accepted =
                SelectGeneration(
                    buffers,
                    problem.Sense,
                    parameters.VariationExecution,
                    cancellationToken);

            state =
                new DeIterationState(
                    populationSize,
                    dimension,
                    parameters.MutationStrategy,
                    parameters.CrossoverStrategy,
                    accepted);

            context.CompleteIteration(
                context.State.BestFitness,
                state);

            stop =
                context.EvaluateStopping(
                    state);

            if (stop.ShouldStop)
            {
                return context.Complete(
                    stop,
                    state);
            }
        }
    }

    private static void InitializePopulation(
        DePopulationBuffers buffers,
        IBoundedContinuousSearchSpace searchSpace,
        DeExecutionOptions execution,
        DeTargetRandomStreams randomStreams,
        CancellationToken cancellationToken)
    {
        DeRangeExecutor.ForTargets(
            buffers.PopulationSize,
            buffers.Dimension,
            execution,
            (start, end) =>
            {
                for (int target = start;
                     target < end;
                     target++)
                {
                    searchSpace.Sample(
                        randomStreams.Get(target),
                        buffers.GetVector(target));
                }
            },
            cancellationToken);
    }

    private static void EvaluatePopulation(
        ISpanContinuousOptimizationProblem problem,
        EvaluationExecutionOptions execution,
        DePopulationBuffers buffers,
        bool useTrialPopulation,
        CancellationToken cancellationToken)
    {
        EvaluationExecutor.ForCandidates(
            buffers.PopulationSize,
            buffers.Dimension,
            problem.EvaluationCharacteristics,
            execution,
            (start, end) =>
            {
                for (int target = start;
                     target < end;
                     target++)
                {
                    ReadOnlySpan<double> vector =
                        useTrialPopulation
                            ? buffers.GetTrialVectorReadOnly(target)
                            : buffers.GetVectorReadOnly(target);

                    double fitness =
                        problem.Evaluate(
                            vector);

                    if (useTrialPopulation)
                    {
                        buffers.SetTrialFitness(
                            target,
                            fitness);
                    }
                    else
                    {
                        buffers.SetFitness(
                            target,
                            fitness);
                    }
                }
            },
            cancellationToken);
    }

    private static void CommitPopulationEvaluations(
        OptimizationContext<double[]> context,
        DePopulationBuffers buffers,
        bool useTrialPopulation)
    {
        ReadOnlySpan<double> fitness =
            useTrialPopulation
                ? buffers.TrialFitness
                : buffers.Fitness;

        for (int target = 0;
             target < buffers.PopulationSize;
             target++)
        {
            double value =
                fitness[target];

            if (context.WouldImprove(value))
            {
                double[] snapshot =
                    (useTrialPopulation
                        ? buffers.GetTrialVectorReadOnly(target)
                        : buffers.GetVectorReadOnly(target))
                    .ToArray();

                context.RegisterOwnedExternalEvaluationSnapshot(
                    snapshot,
                    value);
            }
            else
            {
                context.RegisterExternalEvaluation(
                    value);
            }
        }
    }

    private static int FindBestIndex(
        ReadOnlySpan<double> fitness,
        OptimizationSense sense)
    {
        if (fitness.Length == 0)
        {
            throw new ArgumentException(
                "Population fitness cannot be empty.",
                nameof(fitness));
        }

        int bestIndex = 0;
        double best = fitness[0];

        for (int i = 1;
             i < fitness.Length;
             i++)
        {
            double candidate =
                fitness[i];

            if (sense.IsBetter(
                    candidate,
                    best))
            {
                best =
                    candidate;

                bestIndex =
                    i;
            }
        }

        return bestIndex;
    }

    private static void BuildTrialPopulation(
        DePopulationBuffers buffers,
        IBoundedContinuousSearchSpace searchSpace,
        DifferentialEvolutionParameters parameters,
        DeTargetRandomStreams randomStreams,
        int bestIndex,
        CancellationToken cancellationToken)
    {
        DeRangeExecutor.ForTargets(
            buffers.PopulationSize,
            buffers.Dimension,
            parameters.VariationExecution,
            (start, end) =>
            {
                ReadOnlySpan<double> lower =
                    searchSpace.LowerBounds;

                ReadOnlySpan<double> upper =
                    searchSpace.UpperBounds;

                for (int target = start;
                     target < end;
                     target++)
                {
                    IRandomSource random =
                        randomStreams.Get(
                            target);

                    Span<double> trial =
                        buffers.GetTrialVector(
                            target);

                    ReadOnlySpan<double> current =
                        buffers.GetVectorReadOnly(
                            target);

                    switch (parameters.CrossoverStrategy)
                    {
                        case DeCrossoverStrategy.Binomial:
                            BuildBinomialTrial(
                                buffers,
                                target,
                                bestIndex,
                                current,
                                trial,
                                lower,
                                upper,
                                parameters,
                                random);
                            break;

                        case DeCrossoverStrategy.Exponential:
                            BuildExponentialTrial(
                                buffers,
                                target,
                                bestIndex,
                                current,
                                trial,
                                lower,
                                upper,
                                parameters,
                                random);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            },
            cancellationToken);
    }

    private static void BuildBinomialTrial(
        DePopulationBuffers buffers,
        int target,
        int bestIndex,
        ReadOnlySpan<double> current,
        Span<double> trial,
        ReadOnlySpan<double> lower,
        ReadOnlySpan<double> upper,
        DifferentialEvolutionParameters parameters,
        IRandomSource random)
    {
        int jRandom =
            random.NextInt32(
                buffers.Dimension);

        GetDonorIndices(
            buffers.PopulationSize,
            target,
            parameters.MutationStrategy,
            random,
            out int r1,
            out int r2,
            out int r3,
            out int r4,
            out int r5);

        for (int d = 0;
             d < buffers.Dimension;
             d++)
        {
            bool fromMutant =
                d == jRandom ||
                random.NextDouble() <
                    parameters.CrossoverProbability;

            double value =
                fromMutant
                    ? MutantComponent(
                        buffers,
                        target,
                        bestIndex,
                        d,
                        r1,
                        r2,
                        r3,
                        r4,
                        r5,
                        parameters)
                    : current[d];

            trial[d] =
                HandleBoundary(
                    value,
                    lower[d],
                    upper[d],
                    parameters.BoundaryHandling);
        }
    }

    private static void BuildExponentialTrial(
        DePopulationBuffers buffers,
        int target,
        int bestIndex,
        ReadOnlySpan<double> current,
        Span<double> trial,
        ReadOnlySpan<double> lower,
        ReadOnlySpan<double> upper,
        DifferentialEvolutionParameters parameters,
        IRandomSource random)
    {
        current.CopyTo(
            trial);

        GetDonorIndices(
            buffers.PopulationSize,
            target,
            parameters.MutationStrategy,
            random,
            out int r1,
            out int r2,
            out int r3,
            out int r4,
            out int r5);

        int startDimension =
            random.NextInt32(
                buffers.Dimension);

        int length = 1;

        while (length <
                    buffers.Dimension &&
               random.NextDouble() <
                    parameters.CrossoverProbability)
        {
            length++;
        }

        for (int offset = 0;
             offset < length;
             offset++)
        {
            int d =
                startDimension + offset;

            if (d >= buffers.Dimension)
            {
                d -= buffers.Dimension;
            }

            double value =
                MutantComponent(
                    buffers,
                    target,
                    bestIndex,
                    d,
                    r1,
                    r2,
                    r3,
                    r4,
                    r5,
                    parameters);

            trial[d] =
                HandleBoundary(
                    value,
                    lower[d],
                    upper[d],
                    parameters.BoundaryHandling);
        }
    }

    private static void GetDonorIndices(
        int populationSize,
        int target,
        DeMutationStrategy strategy,
        IRandomSource random,
        out int r1,
        out int r2,
        out int r3,
        out int r4,
        out int r5)
    {
        r4 = -1;
        r5 = -1;

        if (strategy ==
            DeMutationStrategy.Rand2)
        {
            DeDistinctIndexSampler.Sample5(
                random,
                populationSize,
                target,
                out r1,
                out r2,
                out r3,
                out r4,
                out r5);

            return;
        }

        DeDistinctIndexSampler.Sample3(
            random,
            populationSize,
            target,
            out r1,
            out r2,
            out r3);
    }

    private static double MutantComponent(
        DePopulationBuffers buffers,
        int target,
        int bestIndex,
        int dimension,
        int r1,
        int r2,
        int r3,
        int r4,
        int r5,
        DifferentialEvolutionParameters parameters)
    {
        double f =
            parameters.DifferentialWeight;

        return parameters.MutationStrategy switch
        {
            DeMutationStrategy.Rand1 =>
                Component(buffers, r1, dimension) +
                f *
                (Component(buffers, r2, dimension) -
                 Component(buffers, r3, dimension)),

            DeMutationStrategy.Best1 =>
                Component(buffers, bestIndex, dimension) +
                f *
                (Component(buffers, r1, dimension) -
                 Component(buffers, r2, dimension)),

            DeMutationStrategy.CurrentToBest1 =>
                Component(buffers, target, dimension) +
                f *
                (Component(buffers, bestIndex, dimension) -
                 Component(buffers, target, dimension)) +
                f *
                (Component(buffers, r1, dimension) -
                 Component(buffers, r2, dimension)),

            DeMutationStrategy.Rand2 =>
                Component(buffers, r1, dimension) +
                f *
                (Component(buffers, r2, dimension) -
                 Component(buffers, r3, dimension)) +
                f *
                (Component(buffers, r4, dimension) -
                 Component(buffers, r5, dimension)),

            _ =>
                throw new ArgumentOutOfRangeException()
        };
    }

    private static double Component(
        DePopulationBuffers buffers,
        int target,
        int dimension) =>
        buffers.GetVectorReadOnly(
            target)[dimension];

    private static double HandleBoundary(
        double value,
        double lower,
        double upper,
        DeBoundaryHandling handling)
    {
        if (value >= lower &&
            value <= upper)
        {
            return value;
        }

        return handling switch
        {
            DeBoundaryHandling.Clamp =>
                Math.Clamp(
                    value,
                    lower,
                    upper),

            DeBoundaryHandling.Reflect =>
                Reflect(
                    value,
                    lower,
                    upper),

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(handling))
        };
    }

    private static double Reflect(
        double value,
        double lower,
        double upper)
    {
        double width =
            upper - lower;

        double period =
            2.0 * width;

        double shifted =
            value - lower;

        double modulo =
            shifted % period;

        if (modulo < 0.0)
        {
            modulo += period;
        }

        return modulo <= width
            ? lower + modulo
            : upper - (modulo - width);
    }

    private static int SelectGeneration(
        DePopulationBuffers buffers,
        OptimizationSense sense,
        DeExecutionOptions execution,
        CancellationToken cancellationToken)
    {
        int accepted = 0;

        DeRangeExecutor.ForTargets(
            buffers.PopulationSize,
            buffers.Dimension,
            execution,
            (start, end) =>
            {
                int localAccepted = 0;

                for (int target = start;
                     target < end;
                     target++)
                {
                    double trialFitness =
                        buffers.GetTrialFitness(
                            target);

                    double currentFitness =
                        buffers.GetFitness(
                            target);

                    if (sense.IsBetter(
                            trialFitness,
                            currentFitness) ||
                        trialFitness ==
                            currentFitness)
                    {
                        buffers
                            .GetTrialVectorReadOnly(target)
                            .CopyTo(
                                buffers.GetVector(target));

                        buffers.SetFitness(
                            target,
                            trialFitness);

                        localAccepted++;
                    }
                }

                if (localAccepted != 0)
                {
                    Interlocked.Add(
                        ref accepted,
                        localAccepted);
                }
            },
            cancellationToken);

        return accepted;
    }
}