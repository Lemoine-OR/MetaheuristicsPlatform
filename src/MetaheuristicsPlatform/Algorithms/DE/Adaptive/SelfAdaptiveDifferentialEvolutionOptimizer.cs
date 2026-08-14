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
/// Canonical self-adaptive Differential Evolution (jDE) of Brest et al. (2006).
/// </summary>
/// <remarks>
/// Implements DE/rand/1/bin with individual F_i and CR_i values.
/// Proposed control parameters are inherited only when their trial vector strictly
/// improves the corresponding parent.
///
/// Reference:
/// J. Brest, S. Greiner, B. Boskovic, M. Mernik, V. Zumer,
/// "Self-Adapting Control Parameters in Differential Evolution:
/// A Comparative Study on Numerical Benchmark Problems",
/// IEEE Transactions on Evolutionary Computation 10(6), 646-657, 2006.
/// DOI: 10.1109/TEVC.2006.872133.
/// </remarks>
public sealed class SelfAdaptiveDifferentialEvolutionOptimizer :
    IMetaheuristic<double[], JdeParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = "jde-brest-2006",
            Name = "Self-Adaptive Differential Evolution",
            Acronym = "jDE",
            SolutionModel =
                MetaheuristicSolutionModel.Population,
            Families =
                MetaheuristicFamily.Evolutionary,
            Mechanisms =
                MetaheuristicMechanism.EvolutionaryOperators |
                MetaheuristicMechanism.Adaptive,
            SearchSpaces =
                SearchSpaceKind.Continuous,
            IsStochastic = true,
            References =
                new[]
                {
                    DeAdaptiveReferences.BrestEtAl2006
                }
        };

    public JdeParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        JdeParameters parameters,
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
                "The high-performance continuous jDE implementation requires " +
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

        JdeParameterAdaptationPolicy adaptation =
            parameters.CreateAdaptationPolicy();

        adaptation.Initialize(
            parameterBuffers,
            populationSize);

        var randomStreams =
            new DeTargetRandomStreams(
                populationSize,
                runtimeOptions.Seed,
                runtimeOptions.RandomSourceFactory);

        var selectionFeedback =
            GC.AllocateUninitializedArray<DeSelectionFeedback>(
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
            CreateIterationState(
                populationSize,
                dimension,
                acceptedTrials: 0,
                parameterBuffers);

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

            int accepted =
                SelectGeneration(
                    population,
                    parameterBuffers,
                    selectionFeedback,
                    problem.Sense,
                    parameters.VariationExecution,
                    cancellationToken);

            adaptation.CompleteGeneration(
                in adaptationContext,
                parameterBuffers,
                selectionFeedback);

            state =
                CreateIterationState(
                    populationSize,
                    dimension,
                    accepted,
                    parameterBuffers);

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

    private static JdeIterationState CreateIterationState(
        int populationSize,
        int dimension,
        int acceptedTrials,
        DeParameterBuffers parameters)
    {
        double sumF = 0.0;
        double sumCr = 0.0;

        for (int target = 0;
             target < populationSize;
             target++)
        {
            DeControlParameters values =
                parameters.GetParent(
                    target);

            sumF +=
                values.DifferentialWeight;

            sumCr +=
                values.CrossoverProbability;
        }

        return new JdeIterationState(
            populationSize,
            dimension,
            acceptedTrials,
            sumF / populationSize,
            sumCr / populationSize);
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
        IBoundedContinuousSearchSpace searchSpace,
        JdeParameters parameters,
        DeTargetRandomStreams randomStreams,
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

                    DeDistinctIndexSampler.Sample3(
                        random,
                        buffers.PopulationSize,
                        target,
                        out int r1,
                        out int r2,
                        out int r3);

                    DeControlParameters control =
                        parameterBuffers.GetTrial(
                            target);

                    Span<double> trial =
                        buffers.GetTrialVector(
                            target);

                    ReadOnlySpan<double> current =
                        buffers.GetVectorReadOnly(
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

                        double value =
                            fromMutant
                                ? buffers
                                        .GetVectorReadOnly(r1)[d] +
                                    control.DifferentialWeight *
                                    (buffers
                                            .GetVectorReadOnly(r2)[d] -
                                     buffers
                                            .GetVectorReadOnly(r3)[d])
                                : current[d];

                        trial[d] =
                            HandleBoundary(
                                value,
                                lower[d],
                                upper[d],
                                parameters.BoundaryHandling);
                    }
                }
            },
            cancellationToken);
    }

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
        DeParameterBuffers parameterBuffers,
        DeSelectionFeedback[] feedback,
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

                    double parentFitness =
                        buffers.GetFitness(
                            target);

                    bool accept =
                        sense.IsBetter(
                            trialFitness,
                            parentFitness);

                    double improvement =
                        accept
                            ? Math.Abs(
                                parentFitness -
                                trialFitness)
                            : 0.0;

                    feedback[target] =
                        new DeSelectionFeedback(
                            target,
                            accept,
                            parentFitness,
                            trialFitness,
                            improvement);

                    if (!accept)
                    {
                        continue;
                    }

                    buffers
                        .GetTrialVectorReadOnly(target)
                        .CopyTo(
                            buffers.GetVector(target));

                    buffers.SetFitness(
                        target,
                        trialFitness);

                    localAccepted++;
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