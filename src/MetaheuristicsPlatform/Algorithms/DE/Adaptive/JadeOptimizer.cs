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
/// JADE adaptive Differential Evolution with optional external archive.
/// </summary>
/// <remarks>
/// Implements DE/current-to-pbest/1/bin with adaptive F/CR means and the optional
/// external archive of Zhang and Sanderson (2009).
/// DOI: 10.1109/TEVC.2009.2014613.
/// </remarks>
public sealed class JadeOptimizer :
    IMetaheuristic<double[], JadeParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = "jade-2009",
            Name =
                "JADE: Adaptive Differential Evolution With Optional External Archive",
            Acronym = "JADE",
            SolutionModel =
                MetaheuristicSolutionModel.Population,
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
                    DeAdaptiveReferences.ZhangSanderson2009
                }
        };

    public JadeParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        JadeParameters parameters,
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
                "The high-performance continuous JADE implementation requires " +
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

        var population =
            new DePopulationBuffers(
                populationSize,
                dimension);

        var parameterBuffers =
            new DeParameterBuffers(
                populationSize);

        JadeParameterAdaptationPolicy adaptation =
            parameters.CreateAdaptationPolicy();

        adaptation.Initialize(
            parameterBuffers,
            populationSize);

        var randomStreams =
            new DeTargetRandomStreams(
                populationSize,
                runtimeOptions.Seed,
                runtimeOptions.RandomSourceFactory);

        DeExternalArchive? archive =
            parameters.UseExternalArchive
                ? new DeExternalArchive(
                    populationSize,
                    dimension)
                : null;

        var ranking =
            new int[populationSize];

        for (int i = 0;
             i < ranking.Length;
             i++)
        {
            ranking[i] = i;
        }

        var rankingComparer =
            new DeFitnessIndexComparer(
                population,
                problem.Sense);

        var feedback =
            GC.AllocateUninitializedArray<DeSelectionFeedback>(
                populationSize);

        var selected =
            GC.AllocateUninitializedArray<bool>(
                populationSize);

        var successful =
            GC.AllocateUninitializedArray<bool>(
                populationSize);

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                runtimeOptions,
                callback,
                cancellationToken);

        long functionEvaluations = 0;

        var state =
            new JadeIterationState(
                populationSize,
                dimension,
                SuccessfulTrials: 0,
                ArchiveCount: 0,
                adaptation.MeanDifferentialWeight,
                adaptation.MeanCrossoverProbability);

        context.Start(state);

        InitializePopulation(
            population,
            continuousProblem.SearchSpace,
            parameters.VariationExecution,
            randomStreams,
            cancellationToken);

        EvaluatePopulation(
            continuousProblem,
            parameters.EvaluationExecution,
            population,
            useTrialPopulation: false,
            cancellationToken);

        functionEvaluations +=
            populationSize;

        CommitPopulationEvaluations(
            context,
            population,
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

        int generation = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            generation++;

            Array.Sort(
                ranking,
                0,
                populationSize,
                rankingComparer);

            var adaptationContext =
                new DeGenerationAdaptationContext(
                    Generation: generation,
                    ActivePopulationSize: populationSize,
                    FunctionEvaluations: functionEvaluations,
                    MaximumFunctionEvaluations: null);

            adaptation.PrepareGeneration(
                in adaptationContext,
                parameterBuffers,
                randomStreams);

            BuildTrialPopulation(
                population,
                parameterBuffers,
                archive,
                ranking,
                continuousProblem.SearchSpace,
                parameters,
                randomStreams,
                cancellationToken);

            EvaluatePopulation(
                continuousProblem,
                parameters.EvaluationExecution,
                population,
                useTrialPopulation: true,
                cancellationToken);

            functionEvaluations +=
                populationSize;

            CommitPopulationEvaluations(
                context,
                population,
                useTrialPopulation: true);

            int successes =
                CompareGeneration(
                    population,
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
                    archive,
                    successful,
                    randomStreams);
            }

            CommitSelectedTrials(
                population,
                selected,
                parameters.VariationExecution,
                cancellationToken);

            adaptation.CompleteGeneration(
                in adaptationContext,
                parameterBuffers,
                feedback);

            state =
                new JadeIterationState(
                    populationSize,
                    dimension,
                    successes,
                    archive?.Count ?? 0,
                    adaptation.MeanDifferentialWeight,
                    adaptation.MeanCrossoverProbability);

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
                        problem.Evaluate(vector);

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
        for (int target = 0;
             target < buffers.PopulationSize;
             target++)
        {
            double value =
                useTrialPopulation
                    ? buffers.GetTrialFitness(target)
                    : buffers.GetFitness(target);

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

    private static void BuildTrialPopulation(
        DePopulationBuffers buffers,
        DeParameterBuffers parameterBuffers,
        DeExternalArchive? archive,
        int[] ranking,
        IBoundedContinuousSearchSpace searchSpace,
        JadeParameters parameters,
        DeTargetRandomStreams randomStreams,
        CancellationToken cancellationToken)
    {
        int pBestCount =
            Math.Clamp(
                (int)Math.Ceiling(
                    parameters.PBestFraction *
                    buffers.PopulationSize),
                1,
                buffers.PopulationSize);

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
                                buffers.PopulationSize);
                    }
                    while (r1 == target);

                    bool r2FromArchive;
                    int r2Index;

                    SelectR2(
                        random,
                        buffers.PopulationSize,
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

                    int jRandom =
                        random.NextInt32(
                            buffers.Dimension);

                    for (int d = 0;
                         d < buffers.Dimension;
                         d++)
                    {
                        bool fromMutant =
                            d == jRandom ||
                            random.NextDouble() <
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
        int populationSize,
        int archiveCount,
        int target,
        int r1,
        out bool fromArchive,
        out int index)
    {
        int unionCount =
            populationSize +
            archiveCount;

        while (true)
        {
            int selected =
                random.NextInt32(
                    unionCount);

            if (selected < populationSize)
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
                populationSize;
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
                    ? 0.5 * (lower + targetValue)
                    : 0.5 * (upper + targetValue),

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
            : upper - (modulo - width);
    }

    private static int CompareGeneration(
        DePopulationBuffers buffers,
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
            buffers.PopulationSize);

        Array.Clear(
            successful,
            0,
            buffers.PopulationSize);

        int successCount = 0;

        DeRangeExecutor.ForTargets(
            buffers.PopulationSize,
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

                    double improvement =
                        strictlyBetter
                            ? Math.Abs(
                                parentFitness -
                                trialFitness)
                            : 0.0;

                    feedback[target] =
                        new DeSelectionFeedback(
                            target,
                            strictlyBetter,
                            parentFitness,
                            trialFitness,
                            improvement);

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
        DeExternalArchive archive,
        bool[] successful,
        DeTargetRandomStreams randomStreams)
    {
        // Deterministic target order is intentional.
        // Parents are archived before selected trials overwrite them.
        for (int target = 0;
             target < buffers.PopulationSize;
             target++)
        {
            if (!successful[target])
            {
                continue;
            }

            archive.Add(
                buffers.GetVectorReadOnly(target),
                randomStreams.Get(target));
        }
    }

    private static void CommitSelectedTrials(
        DePopulationBuffers buffers,
        bool[] selected,
        DeExecutionOptions execution,
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
                    if (!selected[target])
                    {
                        continue;
                    }

                    buffers
                        .GetTrialVectorReadOnly(target)
                        .CopyTo(
                            buffers.GetVector(target));

                    buffers.SetFitness(
                        target,
                        buffers.GetTrialFitness(target));
                }
            },
            cancellationToken);
    }
}