using MetaheuristicsPlatform.Algorithms.PSO.Dynamics;
using MetaheuristicsPlatform.Algorithms.PSO.Execution;
using MetaheuristicsPlatform.Algorithms.PSO.Social;
using MetaheuristicsPlatform.Algorithms.PSO.State;
using MetaheuristicsPlatform.Algorithms.PSO.Topologies;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Execution;
using MetaheuristicsPlatform.Parameters;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.SearchSpaces.Continuous;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.PSO;

/// <summary>
/// High-performance synchronous continuous Particle Swarm Optimizer.
/// </summary>
public sealed class ParticleSwarmOptimizer :
    IMetaheuristic<double[], PsoParameters>
{
    public MetaheuristicDescriptor Descriptor { get; } =
        new()
        {
            Id = "pso-continuous",
            Name = "Particle Swarm Optimization",
            Acronym = "PSO",
            SolutionModel =
                MetaheuristicSolutionModel.Population,
            Families =
                MetaheuristicFamily.SwarmIntelligence,
            Mechanisms =
                MetaheuristicMechanism.Swarm |
                MetaheuristicMechanism.Adaptive,
            SearchSpaces =
                SearchSpaceKind.Continuous,
            IsStochastic = true,
            References = new[]
            {
                PsoSocialReferences.KennedyEberhart1995,
                PsoSocialReferences.ClercKennedy2002,
                PsoSocialReferences.MendesKennedyNeves2004
            }
        };

    public PsoParameters CreateDefaultParameters() =>
        new();

    public OptimizationResult<double[]> Optimize(
        IOptimizationProblem<double[]> problem,
        PsoParameters parameters,
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
                "The high-performance continuous PSO requires " +
                "ISpanContinuousOptimizationProblem.");
        }

        OptimizationOptions runtimeOptions =
            options ?? new OptimizationOptions();

        runtimeOptions.Validate();

        int swarmSize =
            parameters.SwarmSize;

        int dimension =
            continuousProblem.SearchSpace.Dimension;

        var buffers =
            new PsoSwarmBuffers(
                swarmSize,
                dimension);

        var randomStreams =
            new PsoParticleRandomStreams(
                swarmSize,
                runtimeOptions.Seed,
                runtimeOptions.RandomSourceFactory);

        double[] attractionScratch =
            GC.AllocateUninitializedArray<double>(
                checked(swarmSize * dimension));

        int[] neighborhoodBestGuides =
            GC.AllocateUninitializedArray<int>(
                swarmSize);

        IRandomSource topologyRandom =
            runtimeOptions.RandomSourceFactory.Create(
                RandomStreamSeed.Derive(
                    runtimeOptions.Seed,
                    0x50534F5F544F504FUL)) ??
            throw new InvalidOperationException(
                "Random-source factory returned null for the topology stream.");

        double[] velocityLimits =
            CreateVelocityLimits(
                continuousProblem.SearchSpace,
                parameters.VelocityLimitRangeFraction);

        var bounds =
            new PsoBoundsCache(
                continuousProblem.SearchSpace);

        var context =
            new OptimizationContext<double[]>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                runtimeOptions,
                callback,
                cancellationToken);

        PsoIterationState algorithmState =
            CreateIterationState(
                parameters,
                dimension);

        context.Start(algorithmState);

        InitializeSwarm(
            buffers,
            continuousProblem.SearchSpace,
            parameters,
            randomStreams,
            cancellationToken);

        EvaluateAndInitializePersonalBest(
            continuousProblem,
            parameters,
            buffers,
            cancellationToken);

        CommitEvaluations(
            context,
            buffers);

        StoppingDecision stop =
            context.EvaluateStopping(
                algorithmState);

        if (stop.ShouldStop)
        {
            return context.Complete(
                stop,
                algorithmState);
        }

        NeighborhoodGraphHolder graphHolder =
            new();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool implicitFullyConnectedCanonical =
                parameters.Topology is FullyConnectedTopology &&
                parameters.InfluencePolicy is CanonicalBestInfluencePolicy;

            PsoSocialContext socialContext;

            if (implicitFullyConnectedCanonical)
            {
                socialContext =
                    new PsoSocialContext(
                        buffers.Positions,
                        buffers.PersonalBestPositions,
                        buffers.PersonalBestFitness,
                        swarmSize,
                        dimension,
                        problem.Sense);
            }
            else
            {
                EnsureGraph(
                    graphHolder,
                    parameters.Topology,
                    buffers,
                    problem.Sense,
                    context.State.Iteration,
                    topologyRandom);

                socialContext =
                    new PsoSocialContext(
                        buffers.Positions,
                        buffers.PersonalBestPositions,
                        buffers.PersonalBestFitness,
                        swarmSize,
                        dimension,
                        graphHolder.Graph!,
                        problem.Sense);
            }

            PsoVelocityCoefficients dynamics =
                parameters.VelocityDynamics
                    .GetCoefficients(
                        context.State.Iteration);

            if (parameters.InfluencePolicy is
                CanonicalBestInfluencePolicy)
            {
                PsoNeighborhoodGuideCache.Fill(
                    neighborhoodBestGuides,
                    socialContext,
                    parameters.Topology,
                    parameters.MovementExecution,
                    cancellationToken);
            }

            MovePopulation(
                buffers,
                socialContext,
                parameters,
                dynamics,
                randomStreams,
                neighborhoodBestGuides,
                bounds,
                velocityLimits,
                attractionScratch,
                cancellationToken);

            EvaluateAndUpdatePersonalBest(
                continuousProblem,
                parameters,
                buffers,
                cancellationToken);

            CommitEvaluations(
                context,
                buffers);

            algorithmState =
                CreateIterationState(
                    parameters,
                    dimension);

            context.CompleteIteration(
                context.State.BestFitness,
                algorithmState);

            stop =
                context.EvaluateStopping(
                    algorithmState);

            if (stop.ShouldStop)
            {
                return context.Complete(
                    stop,
                    algorithmState);
            }

            if (parameters.Topology.Descriptor.Dynamics is
                PsoTopologyDynamics.FitnessDynamic or
                PsoTopologyDynamics.SpatialDynamic or
                PsoTopologyDynamics.AdaptiveDynamic or
                PsoTopologyDynamics.SelfOrganizing or
                PsoTopologyDynamics.DynamicRandom)
            {
                graphHolder.Graph = null;
            }
        }
    }

    private static void InitializeSwarm(
        PsoSwarmBuffers buffers,
        IBoundedContinuousSearchSpace searchSpace,
        PsoParameters parameters,
        PsoParticleRandomStreams randomStreams,
        CancellationToken cancellationToken)
    {
        double[] lower =
            searchSpace.LowerBounds.ToArray();

        double[] upper =
            searchSpace.UpperBounds.ToArray();

        PsoRangeExecutor.ForParticles(
            buffers.SwarmSize,
            buffers.Dimension,
            parameters.MovementExecution,
            (start, end) =>
            {
                for (int particle = start;
                     particle < end;
                     particle++)
                {
                    IRandomSource random =
                        randomStreams.Get(particle);

                    Span<double> position =
                        buffers.GetPosition(particle);

                    Span<double> velocity =
                        buffers.GetVelocity(particle);

                    searchSpace.Sample(
                        random,
                        position);

                    for (int d = 0;
                         d < buffers.Dimension;
                         d++)
                    {
                        double maxInitialVelocity =
                            (upper[d] - lower[d]) *
                            parameters.InitialVelocityRangeFraction;

                        velocity[d] =
                            ((2.0 *
                              random.NextDouble()) -
                             1.0) *
                            maxInitialVelocity;
                    }
                }
            },
            cancellationToken);
    }

    private static void EvaluateAndInitializePersonalBest(
        ISpanContinuousOptimizationProblem problem,
        PsoParameters parameters,
        PsoSwarmBuffers buffers,
        CancellationToken cancellationToken)
    {
        EvaluationExecutionOptions execution =
            ResolveEvaluationExecution(
                parameters);

        EvaluationCharacteristics characteristics =
            ResolveEvaluationCharacteristics(
                problem,
                parameters);

        EvaluationExecutor.ForCandidates(
            buffers.SwarmSize,
            buffers.Dimension,
            characteristics,
            execution,
            (start, end) =>
            {
                for (int particle = start;
                     particle < end;
                     particle++)
                {
                    ReadOnlySpan<double> position =
                        buffers.GetPositionReadOnly(
                            particle);

                    double fitness =
                        problem.Evaluate(
                            position);

                    buffers.CurrentFitness[particle] =
                        fitness;

                    position.CopyTo(
                        buffers.GetPersonalBestPosition(
                            particle));

                    buffers.PersonalBestFitness[particle] =
                        fitness;
                }
            },
            cancellationToken);
    }

    private static void EvaluateAndUpdatePersonalBest(
        ISpanContinuousOptimizationProblem problem,
        PsoParameters parameters,
        PsoSwarmBuffers buffers,
        CancellationToken cancellationToken)
    {
        EvaluationExecutionOptions execution =
            ResolveEvaluationExecution(
                parameters);

        EvaluationCharacteristics characteristics =
            ResolveEvaluationCharacteristics(
                problem,
                parameters);

        OptimizationSense sense =
            problem.Sense;

        EvaluationExecutor.ForCandidates(
            buffers.SwarmSize,
            buffers.Dimension,
            characteristics,
            execution,
            (start, end) =>
            {
                for (int particle = start;
                     particle < end;
                     particle++)
                {
                    ReadOnlySpan<double> position =
                        buffers.GetPositionReadOnly(
                            particle);

                    double fitness =
                        problem.Evaluate(
                            position);

                    buffers.CurrentFitness[particle] =
                        fitness;

                    double personalBest =
                        buffers.PersonalBestFitness[
                            particle];

                    if (double.IsNaN(personalBest) ||
                        sense.IsBetter(
                            fitness,
                            personalBest))
                    {
                        position.CopyTo(
                            buffers.GetPersonalBestPosition(
                                particle));

                        buffers.PersonalBestFitness[
                            particle] =
                            fitness;
                    }
                }
            },
            cancellationToken);
    }

    private static EvaluationExecutionOptions
        ResolveEvaluationExecution(
            PsoParameters parameters)
    {
        if (parameters.EnableParallelObjectiveEvaluation)
        {
            return parameters.EvaluationExecution;
        }

        return new EvaluationExecutionOptions
        {
            Mode =
                MetaheuristicsPlatform.Execution.EvaluationExecutionMode.Sequential
        };
    }

    private static EvaluationCharacteristics
        ResolveEvaluationCharacteristics(
            ISpanContinuousOptimizationProblem problem,
            PsoParameters parameters)
    {
        if (parameters.EnableParallelObjectiveEvaluation)
        {
            return problem.EvaluationCharacteristics;
        }

        EvaluationCharacteristics source =
            problem.EvaluationCharacteristics;

        return source with
        {
            SupportsParallelEvaluation = false
        };
    }
    private static void CommitEvaluations(
        OptimizationContext<double[]> context,
        PsoSwarmBuffers buffers)
    {
        for (int particle = 0;
             particle < buffers.SwarmSize;
             particle++)
        {
            double fitness =
                buffers.CurrentFitness[particle];

            if (context.WouldImprove(fitness))
            {
                double[] snapshot =
                    buffers.GetPositionReadOnly(
                            particle)
                        .ToArray();

                context.RegisterOwnedExternalEvaluationSnapshot(
                    snapshot,
                    fitness);
            }
            else
            {
                context.RegisterExternalEvaluation(
                    fitness);
            }
        }
    }

    private static void EnsureGraph(
        NeighborhoodGraphHolder holder,
        IPsoTopology topology,
        PsoSwarmBuffers buffers,
        OptimizationSense sense,
        long iteration,
        IRandomSource topologyRandom)
    {
        if (holder.Graph is not null)
        {
            return;
        }

        var topologyContext =
            new PsoTopologyContext(
                buffers.SwarmSize,
                iteration,
                sense,
                currentFitness:
                    buffers.CurrentFitness,
                personalBestFitness:
                    buffers.PersonalBestFitness);

        holder.Graph =
            topology.CreateGraph(
                topologyContext,
                topologyRandom);
    }

    private static void MovePopulation(
        PsoSwarmBuffers buffers,
        PsoSocialContext socialContext,
        PsoParameters parameters,
        PsoVelocityCoefficients dynamics,
        PsoParticleRandomStreams randomStreams,
        int[] neighborhoodBestGuides,
        PsoBoundsCache bounds,
        double[] velocityLimits,
        double[] attractionScratch,
        CancellationToken cancellationToken)
    {
        PsoRangeExecutor.ForParticles(
            buffers.SwarmSize,
            buffers.Dimension,
            parameters.MovementExecution,
            (start, end) =>
            {
                PsoMovementKernel.UpdateRange(
                    start,
                    end,
                    buffers,
                    socialContext,
                    parameters.InfluencePolicy,
                    dynamics,
                    randomStreams,
                    neighborhoodBestGuides,
                    bounds.Lower,
                    bounds.Upper,
                    velocityLimits,
                    parameters.BoundaryHandling,
                    attractionScratch);
            },
            cancellationToken);
    }

    private static double[] CreateVelocityLimits(
        IBoundedContinuousSearchSpace searchSpace,
        double? rangeFraction)
    {
        if (!rangeFraction.HasValue)
        {
            return Array.Empty<double>();
        }

        double[] limits =
            new double[searchSpace.Dimension];

        double[] lower =
            searchSpace.LowerBounds.ToArray();

        double[] upper =
            searchSpace.UpperBounds.ToArray();

        for (int d = 0;
             d < limits.Length;
             d++)
        {
            limits[d] =
                (upper[d] - lower[d]) *
                rangeFraction.Value;
        }

        return limits;
    }

    private static PsoIterationState CreateIterationState(
        PsoParameters parameters,
        int dimension) =>
        new(
            parameters.SwarmSize,
            dimension,
            parameters.Topology.Descriptor.Id,
            parameters.InfluencePolicy.Descriptor.Id,
            parameters.VelocityDynamics.Id);

    private sealed class NeighborhoodGraphHolder
    {
        public MetaheuristicsPlatform.Graphs.NeighborhoodGraph?
            Graph { get; set; }
    }
}