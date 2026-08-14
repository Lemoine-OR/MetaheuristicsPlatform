using MetaheuristicsPlatform.Algorithms.DE.Execution;
using MetaheuristicsPlatform.Algorithms.DE.Random;
using MetaheuristicsPlatform.Algorithms.DE.State;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.DE.Adaptive;

/// <summary>
/// L-SHADE: SHADE 1.1 with Linear Population Size Reduction.
/// </summary>
/// <remarks>
/// Reference:
/// R. Tanabe, A. S. Fukunaga,
/// "Improving the Search Performance of SHADE Using Linear Population Size Reduction",
/// IEEE CEC 2014, 1658-1665.
/// DOI: 10.1109/CEC.2014.6900380.
/// </remarks>
public sealed class LShadeOptimizer :
    IMetaheuristic<double[], LShadeParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = "lshade-2014",
            Name =
                "L-SHADE: SHADE with Linear Population Size Reduction",
            Acronym = "L-SHADE",
            SolutionModel =
                MetaheuristicSolutionModel.VariablePopulation,
            Families =
                MetaheuristicFamily.Evolutionary,
            Mechanisms =
                MetaheuristicMechanism.EvolutionaryOperators |
                MetaheuristicMechanism.Adaptive |
                MetaheuristicMechanism.MemoryBased,
            SearchSpaces =
                SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    DeAdaptiveReferences.TanabeFukunaga2014
                }
        };

    public LShadeParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        LShadeParameters parameters,
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
                "The high-performance continuous L-SHADE implementation requires " +
                "ISpanContinuousOptimizationProblem.");
        }

        OptimizationOptions runtimeOptions =
            options ??
            new OptimizationOptions();

        runtimeOptions.Validate();

        int dimension =
            continuousProblem.SearchSpace.Dimension;

        int initialPopulationSize =
            parameters.ResolveInitialPopulationSize(
                dimension);

        long maximumFunctionEvaluations =
            parameters.ResolveMaximumFunctionEvaluations(
                dimension);

        if (maximumFunctionEvaluations <
            initialPopulationSize)
        {
            throw new ArgumentException(
                "The L-SHADE evaluation budget must cover the initial population.",
                nameof(parameters));
        }

        int maximumArchiveCapacity =
            parameters.ResolveArchiveCapacity(
                initialPopulationSize);

        var population =
            new DePopulationBuffers(
                initialPopulationSize,
                dimension);

        var parameterBuffers =
            new DeParameterBuffers(
                initialPopulationSize);

        var adaptation =
            new LShadeParameterAdaptationPolicy(
                parameters.MemorySize,
                parameters.InitialMemoryValue,
                parameters.DistributionScale);

        adaptation.Initialize(
            parameterBuffers,
            initialPopulationSize);

        var randomStreams =
            new DeTargetRandomStreams(
                initialPopulationSize,
                runtimeOptions.Seed,
                runtimeOptions.RandomSourceFactory);

        IRandomSource archiveRandom =
            runtimeOptions.RandomSourceFactory.Create(
                runtimeOptions.Seed ^
                0xD1B54A32D192ED03UL);

        DeExternalArchive? archive =
            parameters.UseExternalArchive
                ? new DeExternalArchive(
                    maximumArchiveCapacity,
                    dimension)
                : null;

        var ranking =
            new int[
                initialPopulationSize];

        var survivorFlags =
            new bool[
                initialPopulationSize];

        var feedback =
            GC.AllocateUninitializedArray<DeSelectionFeedback>(
                initialPopulationSize);

        var selected =
            GC.AllocateUninitializedArray<bool>(
                initialPopulationSize);

        var successful =
            GC.AllocateUninitializedArray<bool>(
                initialPopulationSize);

        var rankingComparer =
            new DeFitnessIndexComparer(
                population,
                problem.Sense);

        var schedule =
            new LShadePopulationSchedule();

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                runtimeOptions,
                callback,
                cancellationToken);

        int activePopulationSize =
            initialPopulationSize;

        int archiveLimit =
            parameters.ResolveArchiveCapacity(
                activePopulationSize);

        long functionEvaluations = 0;

        var state =
            CreateState(
                initialPopulationSize,
                activePopulationSize,
                parameters.MinimumPopulationSize,
                dimension,
                successfulTrials: 0,
                archive?.Count ?? 0,
                archiveLimit,
                adaptation.MemoryPosition,
                functionEvaluations,
                maximumFunctionEvaluations);

        context.Start(state);

        InitializePopulation(
            population,
            activePopulationSize,
            continuousProblem.SearchSpace,
            parameters.VariationExecution,
            randomStreams,
            cancellationToken);

        EvaluatePopulation(
            continuousProblem,
            parameters.EvaluationExecution,
            population,
            activePopulationSize,
            useTrialPopulation: false,
            cancellationToken);

        functionEvaluations +=
            activePopulationSize;

        CommitPopulationEvaluations(
            context,
            population,
            activePopulationSize,
            useTrialPopulation: false);

        state =
            state with
            {
                FunctionEvaluations =
                    functionEvaluations
            };

        StoppingDecision stop =
            context.EvaluateStopping(
                state);

        if (stop.ShouldStop)
        {
            return context.Complete(
                stop,
                state);
        }

        int generation = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            generation++;

            PrepareRanking(
                ranking,
                activePopulationSize);

            Array.Sort(
                ranking,
                0,
                activePopulationSize,
                rankingComparer);

            var adaptationContext =
                new DeGenerationAdaptationContext(
                    Generation: generation,
                    ActivePopulationSize:
                        activePopulationSize,
                    FunctionEvaluations:
                        functionEvaluations,
                    MaximumFunctionEvaluations:
                        maximumFunctionEvaluations);

            adaptation.PrepareGeneration(
                in adaptationContext,
                parameterBuffers,
                randomStreams);

            BuildTrialPopulation(
                population,
                parameterBuffers,
                archive,
                ranking,
                activePopulationSize,
                continuousProblem.SearchSpace,
                parameters,
                randomStreams,
                cancellationToken);

            EvaluatePopulation(
                continuousProblem,
                parameters.EvaluationExecution,
                population,
                activePopulationSize,
                useTrialPopulation: true,
                cancellationToken);

            functionEvaluations +=
                activePopulationSize;

            CommitPopulationEvaluations(
                context,
                population,
                activePopulationSize,
                useTrialPopulation: true);

            int successes =
                CompareGeneration(
                    population,
                    activePopulationSize,
                    feedback,
                    selected,
                    successful,
                    problem.Sense,
                    parameters.VariationExecution,
                    cancellationToken);

            if (archive is not null)
            {
                ArchiveSuccessfulParents(
                    population,
                    activePopulationSize,
                    archive,
                    successful,
                    archiveRandom);

                archive.TrimToCount(
                    archiveLimit,
                    archiveRandom);
            }

            CommitSelectedTrials(
                population,
                activePopulationSize,
                selected,
                parameters.VariationExecution,
                cancellationToken);

            adaptation.CompleteGeneration(
                in adaptationContext,
                parameterBuffers,
                feedback);

            int targetPopulationSize =
                schedule.GetTargetPopulationSize(
                    initialPopulationSize,
                    activePopulationSize,
                    parameters.MinimumPopulationSize,
                    functionEvaluations,
                    maximumFunctionEvaluations);

            if (targetPopulationSize <
                activePopulationSize)
            {
                activePopulationSize =
                    ReducePopulation(
                        population,
                        parameterBuffers,
                        ranking,
                        survivorFlags,
                        rankingComparer,
                        activePopulationSize,
                        targetPopulationSize);
            }

            archiveLimit =
                parameters.ResolveArchiveCapacity(
                    activePopulationSize);

            if (archive is not null)
            {
                archive.TrimToCount(
                    archiveLimit,
                    archiveRandom);
            }

            state =
                CreateState(
                    initialPopulationSize,
                    activePopulationSize,
                    parameters.MinimumPopulationSize,
                    dimension,
                    successes,
                    archive?.Count ?? 0,
                    archiveLimit,
                    adaptation.MemoryPosition,
                    functionEvaluations,
                    maximumFunctionEvaluations);

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

    private static LShadeIterationState CreateState(
        int initialPopulationSize,
        int activePopulationSize,
        int minimumPopulationSize,
        int dimension,
        int successfulTrials,
        int archiveCount,
        int archiveLimit,
        int memoryPosition,
        long functionEvaluations,
        long maximumFunctionEvaluations) =>
        new(
            initialPopulationSize,
            activePopulationSize,
            minimumPopulationSize,
            dimension,
            successfulTrials,
            archiveCount,
            archiveLimit,
            memoryPosition,
            functionEvaluations,
            maximumFunctionEvaluations);

    private static void PrepareRanking(
        int[] ranking,
        int activePopulationSize)
    {
        for (int i = 0;
             i < activePopulationSize;
             i++)
        {
            ranking[i] = i;
        }
    }

    private static void InitializePopulation(
        DePopulationBuffers buffers,
        int activePopulationSize,
        IBoundedContinuousSearchSpace searchSpace,
        DeExecutionOptions execution,
        DeTargetRandomStreams randomStreams,
        CancellationToken cancellationToken)
    {
        DeRangeExecutor.ForTargets(
            activePopulationSize,
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
        int activePopulationSize,
        bool useTrialPopulation,
        CancellationToken cancellationToken)
    {
        EvaluationExecutor.ForCandidates(
            activePopulationSize,
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
                            ? buffers.GetTrialVectorReadOnly(
                                target)
                            : buffers.GetVectorReadOnly(
                                target);

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
        int activePopulationSize,
        bool useTrialPopulation)
    {
        for (int target = 0;
             target < activePopulationSize;
             target++)
        {
            double value =
                useTrialPopulation
                    ? buffers.GetTrialFitness(
                        target)
                    : buffers.GetFitness(
                        target);

            if (context.WouldImprove(value))
            {
                double[] snapshot =
                    (useTrialPopulation
                        ? buffers.GetTrialVectorReadOnly(
                            target)
                        : buffers.GetVectorReadOnly(
                            target))
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

    private static void BuildTrialPopulation(
        DePopulationBuffers buffers,
        DeParameterBuffers parameterBuffers,
        DeExternalArchive? archive,
        int[] ranking,
        int activePopulationSize,
        IBoundedContinuousSearchSpace searchSpace,
        LShadeParameters parameters,
        DeTargetRandomStreams randomStreams,
        CancellationToken cancellationToken)
    {
        int pBestCount =
            Math.Clamp(
                Math.Max(
                    2,
                    (int)Math.Round(
                        parameters.PBestFraction *
                        activePopulationSize,
                        MidpointRounding.AwayFromZero)),
                1,
                activePopulationSize);

        DeRangeExecutor.ForTargets(
            activePopulationSize,
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
                        randomStreams.Get(target);

                    int pBest =
                        ranking[
                            random.NextInt32(
                                pBestCount)];

                    int r1;

                    do
                    {
                        r1 =
                            random.NextInt32(
                                activePopulationSize);
                    }
                    while (r1 == target);

                    bool r2FromArchive;
                    int r2Index;

                    SelectR2(
                        random,
                        activePopulationSize,
                        archive?.Count ?? 0,
                        target,
                        r1,
                        out r2FromArchive,
                        out r2Index);

                    DeControlParameters control =
                        parameterBuffers.GetTrial(
                            target);

                    ReadOnlySpan<double> current =
                        buffers.GetVectorReadOnly(
                            target);

                    ReadOnlySpan<double> pBestVector =
                        buffers.GetVectorReadOnly(
                            pBest);

                    ReadOnlySpan<double> r1Vector =
                        buffers.GetVectorReadOnly(
                            r1);

                    ReadOnlySpan<double> r2Vector =
                        r2FromArchive
                            ? archive!.GetVectorReadOnly(
                                r2Index)
                            : buffers.GetVectorReadOnly(
                                r2Index);

                    Span<double> trial =
                        buffers.GetTrialVector(
                            target);

                    int forcedDimension =
                        random.NextInt32(
                            buffers.Dimension);

                    for (int d = 0;
                         d < buffers.Dimension;
                         d++)
                    {
                        bool fromMutant =
                            d == forcedDimension ||
                            random.NextDouble() <=
                                control.CrossoverProbability;

                        if (!fromMutant)
                        {
                            trial[d] =
                                current[d];

                            continue;
                        }

                        double mutant =
                            current[d] +
                            control.DifferentialWeight *
                                (pBestVector[d] -
                                 current[d]) +
                            control.DifferentialWeight *
                                (r1Vector[d] -
                                 r2Vector[d]);

                        trial[d] =
                            HandleBoundary(
                                mutant,
                                current[d],
                                lower[d],
                                upper[d],
                                parameters.BoundaryHandling);
                    }
                }
            },
            cancellationToken);
    }

    private static void SelectR2(
        IRandomSource random,
        int activePopulationSize,
        int archiveCount,
        int target,
        int r1,
        out bool fromArchive,
        out int index)
    {
        int unionCount =
            activePopulationSize +
            archiveCount;

        while (true)
        {
            int selected =
                random.NextInt32(
                    unionCount);

            if (selected <
                activePopulationSize)
            {
                if (selected == target ||
                    selected == r1)
                {
                    continue;
                }

                fromArchive = false;
                index = selected;
                return;
            }

            fromArchive = true;
            index =
                selected -
                activePopulationSize;

            return;
        }
    }

    private static double HandleBoundary(
        double value,
        double targetValue,
        double lower,
        double upper,
        JadeBoundaryHandling handling)
    {
        if (value >= lower &&
            value <= upper)
        {
            return value;
        }

        return handling switch
        {
            JadeBoundaryHandling.MidpointToTarget =>
                value < lower
                    ? 0.5 *
                        (lower + targetValue)
                    : 0.5 *
                        (upper + targetValue),

            JadeBoundaryHandling.Clamp =>
                Math.Clamp(
                    value,
                    lower,
                    upper),

            JadeBoundaryHandling.Reflect =>
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
            : upper -
                (modulo - width);
    }

    private static int CompareGeneration(
        DePopulationBuffers buffers,
        int activePopulationSize,
        DeSelectionFeedback[] feedback,
        bool[] selected,
        bool[] successful,
        OptimizationSense sense,
        DeExecutionOptions execution,
        CancellationToken cancellationToken)
    {
        Array.Clear(
            selected,
            0,
            activePopulationSize);

        Array.Clear(
            successful,
            0,
            activePopulationSize);

        int successCount = 0;

        DeRangeExecutor.ForTargets(
            activePopulationSize,
            buffers.Dimension,
            execution,
            (start, end) =>
            {
                int localSuccesses = 0;

                for (int target = start;
                     target < end;
                     target++)
                {
                    double parentFitness =
                        buffers.GetFitness(
                            target);

                    double trialFitness =
                        buffers.GetTrialFitness(
                            target);

                    bool strictlyBetter =
                        sense.IsBetter(
                            trialFitness,
                            parentFitness);

                    bool equal =
                        trialFitness ==
                        parentFitness;

                    selected[target] =
                        strictlyBetter ||
                        equal;

                    successful[target] =
                        strictlyBetter;

                    feedback[target] =
                        new DeSelectionFeedback(
                            target,
                            strictlyBetter,
                            parentFitness,
                            trialFitness,
                            strictlyBetter
                                ? Math.Abs(
                                    parentFitness -
                                    trialFitness)
                                : 0.0);

                    if (strictlyBetter)
                    {
                        localSuccesses++;
                    }
                }

                if (localSuccesses != 0)
                {
                    Interlocked.Add(
                        ref successCount,
                        localSuccesses);
                }
            },
            cancellationToken);

        return successCount;
    }

    private static void ArchiveSuccessfulParents(
        DePopulationBuffers buffers,
        int activePopulationSize,
        DeExternalArchive archive,
        bool[] successful,
        IRandomSource archiveRandom)
    {
        for (int target = 0;
             target < activePopulationSize;
             target++)
        {
            if (!successful[target])
            {
                continue;
            }

            archive.Add(
                buffers.GetVectorReadOnly(
                    target),
                archiveRandom);
        }
    }

    private static void CommitSelectedTrials(
        DePopulationBuffers buffers,
        int activePopulationSize,
        bool[] selected,
        DeExecutionOptions execution,
        CancellationToken cancellationToken)
    {
        DeRangeExecutor.ForTargets(
            activePopulationSize,
            buffers.Dimension,
            execution,
            (start, end) =>
            {
                for (int target = start;
                     target < end;
                     target++)
                {
                    if (!selected[target])
                    {
                        continue;
                    }

                    buffers
                        .GetTrialVectorReadOnly(
                            target)
                        .CopyTo(
                            buffers.GetVector(
                                target));

                    buffers.SetFitness(
                        target,
                        buffers.GetTrialFitness(
                            target));
                }
            },
            cancellationToken);
    }

    private static int ReducePopulation(
        DePopulationBuffers population,
        DeParameterBuffers parameterBuffers,
        int[] ranking,
        bool[] survivorFlags,
        DeFitnessIndexComparer rankingComparer,
        int activePopulationSize,
        int targetPopulationSize)
    {
        if (targetPopulationSize >=
            activePopulationSize)
        {
            return activePopulationSize;
        }

        PrepareRanking(
            ranking,
            activePopulationSize);

        Array.Sort(
            ranking,
            0,
            activePopulationSize,
            rankingComparer);

        Array.Clear(
            survivorFlags,
            0,
            activePopulationSize);

        for (int rank = 0;
             rank < targetPopulationSize;
             rank++)
        {
            survivorFlags[
                ranking[rank]] =
                true;
        }

        int write = 0;

        for (int read = 0;
             read < activePopulationSize;
             read++)
        {
            if (!survivorFlags[read])
            {
                continue;
            }

            if (write != read)
            {
                population
                    .GetVectorReadOnly(read)
                    .CopyTo(
                        population.GetVector(write));

                population.SetFitness(
                    write,
                    population.GetFitness(read));

                DeControlParameters parentParameters =
                    parameterBuffers.GetParent(read);

                DeControlParameters trialParameters =
                    parameterBuffers.GetTrial(read);

                parameterBuffers.SetParent(
                    write,
                    in parentParameters);

                parameterBuffers.SetTrial(
                    write,
                    in trialParameters);
            }

            write++;
        }

        if (write != targetPopulationSize)
        {
            throw new InvalidOperationException(
                "L-SHADE population compaction did not produce the requested size.");
        }

        return targetPopulationSize;
    }
}