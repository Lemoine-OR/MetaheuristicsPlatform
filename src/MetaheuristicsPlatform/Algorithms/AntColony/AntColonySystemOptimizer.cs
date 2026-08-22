using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>
/// Generic Ant Colony System of Dorigo and Gambardella (1997).
/// </summary>
public sealed class AntColonySystemOptimizer<
    TSolution,
    TComponent,
    TPheromoneKey,
    TEnumerator> :
    IMetaheuristic<TSolution, AntColonySystemParameters>
    where TPheromoneKey : notnull
    where TEnumerator : struct, IAntColonyCandidateEnumerator<TComponent>
{
    private readonly AntSystemConstructionEngine<
        TSolution,TComponent,TPheromoneKey,TEnumerator> _construction;
    private readonly IAntSystemDepositPolicy<TSolution> _depositPolicy;
    private readonly IEqualityComparer<TPheromoneKey>? _pheromoneKeyComparer;

    public AntColonySystemOptimizer(
        IAntColonyConstructionModel<
            TSolution,TComponent,TPheromoneKey,TEnumerator> constructionModel,
        IAntSystemDepositPolicy<TSolution> depositPolicy,
        IEqualityComparer<TPheromoneKey>? pheromoneKeyComparer = null)
    {
        _construction =
            new AntSystemConstructionEngine<
                TSolution,TComponent,TPheromoneKey,TEnumerator>(
                constructionModel ??
                throw new ArgumentNullException(nameof(constructionModel)));

        _depositPolicy =
            depositPolicy ??
            throw new ArgumentNullException(nameof(depositPolicy));

        _pheromoneKeyComparer = pheromoneKeyComparer;
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.AntColonySystem,
        Name = "Ant Colony System - Dorigo-Gambardella",
        Acronym = "ACS",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families =
            MetaheuristicFamily.SwarmIntelligence |
            MetaheuristicFamily.Constructive,
        Mechanisms =
            MetaheuristicMechanism.Swarm |
            MetaheuristicMechanism.Constructive |
            MetaheuristicMechanism.MemoryBased |
            MetaheuristicMechanism.Adaptive,
        SearchSpaces =
            SearchSpaceKind.Binary |
            SearchSpaceKind.Integer |
            SearchSpaceKind.Permutation |
            SearchSpaceKind.Combinatorial |
            SearchSpaceKind.Mixed,
        IsStochastic = true,
        References = [ AntSystemReferences.DorigoGambardella1997 ]
    };

    public AntColonySystemParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        AntColonySystemParameters parameters,
        ISolutionCloner<TSolution> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<TSolution>? callback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(solutionCloner);
        ArgumentNullException.ThrowIfNull(stoppingCriterion);

        parameters.Validate();

        var context =
            new OptimizationContext<TSolution>(
                Descriptor,
                problem,
                solutionCloner,
                stoppingCriterion,
                options,
                callback,
                cancellationToken);

        // ACS does not globally evaporate every trail at every iteration:
        // local updates affect traversed keys and global update affects the
        // best-so-far path only.
        var pheromones =
            new AntSystemPheromoneMemory<TPheromoneKey>(
                parameters.InitialPheromone,
                evaporationRate: 0.0,
                _pheromoneKeyComparer);

        AdvancedAntColonyState state = default;
        context.Start(state);

        TSolution? bestSolution = default;
        IReadOnlyList<TPheromoneKey>? bestPath = null;
        double bestObjective = problem.Sense.WorstValue();
        bool hasBest = false;

        long antsConstructed = 0;
        long constructionSteps = 0;
        long transitionEvaluations = 0;
        long globalUpdates = 0;
        long localUpdates = 0;
        int nonImproving = 0;

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            double? iterationBest = null;
            bool improvedThisIteration = false;

            for (int ant = 0; ant < parameters.AntCount; ant++)
            {
                AntColonyConstructionResult<TSolution,TPheromoneKey> construction =
                    _construction.Construct(
                        problem,
                        context.Random,
                        pheromones,
                        alpha: 1.0,
                        beta: parameters.Beta,
                        maximumConstructionSteps: parameters.MaximumConstructionSteps,
                        cancellationToken,
                        exploitationProbability: parameters.ExploitationProbability,
                        selectedKeyUpdate:
                            key =>
                            {
                                double current = pheromones.Get(key);
                                double updated =
                                    (1.0 - parameters.LocalUpdateRate) * current +
                                    parameters.LocalUpdateRate * parameters.InitialPheromone;
                                pheromones.Set(key, updated);
                                localUpdates++;
                            });

                antsConstructed++;
                constructionSteps += construction.ConstructionSteps;
                transitionEvaluations += construction.TransitionEvaluations;

                state = new AdvancedAntColonyState(
                    iteration - 1,
                    antsConstructed,
                    constructionSteps,
                    transitionEvaluations,
                    pheromones.Count,
                    globalUpdates,
                    localUpdates,
                    0,
                    nonImproving,
                    iterationBest);

                double objective =
                    context.Evaluate(construction.Solution, state);

                if (!iterationBest.HasValue ||
                    problem.Sense.IsBetter(objective, iterationBest.Value))
                {
                    iterationBest = objective;
                }

                if (!hasBest ||
                    problem.Sense.IsBetter(objective, bestObjective))
                {
                    bestObjective = objective;
                    bestSolution = solutionCloner.Clone(construction.Solution);
                    bestPath = construction.PheromoneKeys.ToArray();
                    hasBest = true;
                    improvedThisIteration = true;
                }

                StoppingDecision stop =
                    context.EvaluateStopping(state);

                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }
            }

            if (!hasBest || bestSolution is null || bestPath is null)
            {
                throw new InvalidOperationException(
                    "ACS completed a colony without a valid best solution.");
            }

            double delta =
                _depositPolicy.GetDeposit(
                    in bestSolution,
                    bestObjective,
                    0,
                    1,
                    problem);

            if (!double.IsFinite(delta) || delta < 0.0)
            {
                throw new InvalidOperationException(
                    "ACS deposit policy returned an invalid global deposit.");
            }

            foreach (TPheromoneKey key in bestPath)
            {
                double current = pheromones.Get(key);
                double updated =
                    (1.0 - parameters.GlobalEvaporationRate) * current +
                    parameters.GlobalEvaporationRate * delta;
                pheromones.Set(key, updated);
                globalUpdates++;
            }

            nonImproving =
                improvedThisIteration
                    ? 0
                    : nonImproving + 1;

            state = new AdvancedAntColonyState(
                iteration,
                antsConstructed,
                constructionSteps,
                transitionEvaluations,
                pheromones.Count,
                globalUpdates,
                localUpdates,
                0,
                nonImproving,
                iterationBest);

            context.CompleteIteration(iterationBest, state);

            StoppingDecision iterationStop =
                context.EvaluateStopping(state);

            if (iterationStop.ShouldStop)
            {
                return context.Complete(iterationStop, state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumAntColonySystemIterations",
                "The configured Ant Colony System iteration limit was reached."),
            state);
    }
}
