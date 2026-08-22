using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>
/// Generic MAX-MIN Ant System of Stutzle and Hoos (2000).
/// </summary>
public sealed class MaxMinAntSystemOptimizer<
    TSolution,
    TComponent,
    TPheromoneKey,
    TEnumerator> :
    IMetaheuristic<TSolution, MaxMinAntSystemParameters>
    where TPheromoneKey : notnull
    where TEnumerator : struct, IAntColonyCandidateEnumerator<TComponent>
{
    private readonly AntSystemConstructionEngine<
        TSolution,TComponent,TPheromoneKey,TEnumerator> _construction;
    private readonly IAntSystemDepositPolicy<TSolution> _depositPolicy;
    private readonly IEqualityComparer<TPheromoneKey>? _pheromoneKeyComparer;

    public MaxMinAntSystemOptimizer(
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
        Id = MetaheuristicAlgorithmIds.MaxMinAntSystem,
        Name = "MAX-MIN Ant System - Stutzle-Hoos",
        Acronym = "MMAS",
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
        References = [ AntSystemReferences.StutzleHoos2000 ]
    };

    public MaxMinAntSystemParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        MaxMinAntSystemParameters parameters,
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

        var pheromones =
            new AntSystemPheromoneMemory<TPheromoneKey>(
                parameters.InitialPheromone,
                parameters.EvaporationRate,
                _pheromoneKeyComparer,
                parameters.MinimumPheromone,
                parameters.MaximumPheromone);

        AdvancedAntColonyState state = default;
        context.Start(state);

        TSolution? globalBestSolution = default;
        IReadOnlyList<TPheromoneKey>? globalBestPath = null;
        double globalBestObjective = problem.Sense.WorstValue();
        bool hasGlobalBest = false;

        long antsConstructed = 0;
        long constructionSteps = 0;
        long transitionEvaluations = 0;
        long globalUpdates = 0;
        int restarts = 0;
        int nonImproving = 0;

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TSolution? iterationBestSolution = default;
            IReadOnlyList<TPheromoneKey>? iterationBestPath = null;
            double iterationBestObjective = problem.Sense.WorstValue();
            bool hasIterationBest = false;
            bool improvedThisIteration = false;

            for (int ant = 0; ant < parameters.AntCount; ant++)
            {
                AntColonyConstructionResult<TSolution,TPheromoneKey> construction =
                    _construction.Construct(
                        problem,
                        context.Random,
                        pheromones,
                        parameters.Alpha,
                        parameters.Beta,
                        parameters.MaximumConstructionSteps,
                        cancellationToken);

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
                    0,
                    restarts,
                    nonImproving,
                    hasIterationBest ? iterationBestObjective : null);

                double objective =
                    context.Evaluate(construction.Solution, state);

                if (!hasIterationBest ||
                    problem.Sense.IsBetter(objective, iterationBestObjective))
                {
                    iterationBestObjective = objective;
                    iterationBestSolution = solutionCloner.Clone(construction.Solution);
                    iterationBestPath = construction.PheromoneKeys.ToArray();
                    hasIterationBest = true;
                }

                if (!hasGlobalBest ||
                    problem.Sense.IsBetter(objective, globalBestObjective))
                {
                    globalBestObjective = objective;
                    globalBestSolution = solutionCloner.Clone(construction.Solution);
                    globalBestPath = construction.PheromoneKeys.ToArray();
                    hasGlobalBest = true;
                    improvedThisIteration = true;
                }

                StoppingDecision stop =
                    context.EvaluateStopping(state);

                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }
            }

            if (!hasIterationBest ||
                iterationBestSolution is null ||
                iterationBestPath is null ||
                !hasGlobalBest ||
                globalBestSolution is null ||
                globalBestPath is null)
            {
                throw new InvalidOperationException(
                    "MMAS completed a colony without a valid best solution.");
            }

            pheromones.Evaporate();

            TSolution reinforcementSolution;
            IReadOnlyList<TPheromoneKey> reinforcementPath;
            double reinforcementObjective;

            if (parameters.BestSource ==
                MaxMinAntSystemBestSource.IterationBest)
            {
                reinforcementSolution = iterationBestSolution;
                reinforcementPath = iterationBestPath;
                reinforcementObjective = iterationBestObjective;
            }
            else
            {
                reinforcementSolution = globalBestSolution;
                reinforcementPath = globalBestPath;
                reinforcementObjective = globalBestObjective;
            }

            double deposit =
                _depositPolicy.GetDeposit(
                    in reinforcementSolution,
                    reinforcementObjective,
                    0,
                    1,
                    problem);

            if (!double.IsFinite(deposit) || deposit < 0.0)
            {
                throw new InvalidOperationException(
                    "MMAS deposit policy returned an invalid deposit.");
            }

            foreach (TPheromoneKey key in reinforcementPath)
            {
                pheromones.Deposit(key, deposit);
                globalUpdates++;
            }

            nonImproving =
                improvedThisIteration
                    ? 0
                    : nonImproving + 1;

            if (parameters.RestartAfterNonImprovingIterations > 0 &&
                nonImproving >= parameters.RestartAfterNonImprovingIterations)
            {
                pheromones.Reset();
                restarts++;
                nonImproving = 0;
            }

            state = new AdvancedAntColonyState(
                iteration,
                antsConstructed,
                constructionSteps,
                transitionEvaluations,
                pheromones.Count,
                globalUpdates,
                0,
                restarts,
                nonImproving,
                iterationBestObjective);

            context.CompleteIteration(
                iterationBestObjective,
                state);

            StoppingDecision iterationStop =
                context.EvaluateStopping(state);

            if (iterationStop.ShouldStop)
            {
                return context.Complete(iterationStop, state);
            }
        }

        return context.Complete(
            StoppingDecision.Stop(
                "MaximumMaxMinAntSystemIterations",
                "The configured MAX-MIN Ant System iteration limit was reached."),
            state);
    }
}
