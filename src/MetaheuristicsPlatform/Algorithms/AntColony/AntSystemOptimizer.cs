using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.AntColony;

/// <summary>
/// Generic canonical Ant System foundation of Dorigo, Maniezzo and Colorni.
/// Multiple ants construct complete solutions using proportional pheromone/heuristic
/// transitions. After every complete colony iteration, pheromone evaporates and every
/// ant contributes through the configured deposit policy.
/// </summary>
public sealed class AntSystemOptimizer<
    TSolution,
    TComponent,
    TPheromoneKey,
    TEnumerator> :
    IMetaheuristic<TSolution, AntSystemParameters>
    where TPheromoneKey : notnull
    where TEnumerator : struct, IAntColonyCandidateEnumerator<TComponent>
{
    private readonly AntSystemConstructionEngine<
        TSolution,
        TComponent,
        TPheromoneKey,
        TEnumerator> _construction;

    private readonly IAntSystemDepositPolicy<TSolution> _depositPolicy;
    private readonly IEqualityComparer<TPheromoneKey>? _pheromoneKeyComparer;

    public AntSystemOptimizer(
        IAntColonyConstructionModel<
            TSolution,
            TComponent,
            TPheromoneKey,
            TEnumerator> constructionModel,
        IAntSystemDepositPolicy<TSolution> depositPolicy,
        IEqualityComparer<TPheromoneKey>? pheromoneKeyComparer = null)
    {
        _construction =
            new AntSystemConstructionEngine<
                TSolution,
                TComponent,
                TPheromoneKey,
                TEnumerator>(
                constructionModel ??
                throw new ArgumentNullException(nameof(constructionModel)));

        _depositPolicy =
            depositPolicy ??
            throw new ArgumentNullException(nameof(depositPolicy));

        _pheromoneKeyComparer = pheromoneKeyComparer;
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.AntSystem,
        Name = "Ant System - Dorigo-Maniezzo-Colorni",
        Acronym = "AS",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families =
            MetaheuristicFamily.SwarmIntelligence |
            MetaheuristicFamily.Constructive,
        Mechanisms =
            MetaheuristicMechanism.Swarm |
            MetaheuristicMechanism.Constructive |
            MetaheuristicMechanism.MemoryBased,
        SearchSpaces =
            SearchSpaceKind.Binary |
            SearchSpaceKind.Integer |
            SearchSpaceKind.Permutation |
            SearchSpaceKind.Combinatorial |
            SearchSpaceKind.Mixed,
        IsStochastic = true,
        References =
        [
            AntSystemReferences.DorigoManiezzoColorni1996
        ]
    };

    public AntSystemParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        AntSystemParameters parameters,
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
                _pheromoneKeyComparer);

        var state =
            new AntSystemState(
                IterationsCompleted: 0,
                AntsConstructed: 0,
                ConstructionSteps: 0,
                TransitionEvaluations: 0,
                PheromoneEntries: 0,
                EvaporationRounds: 0,
                PheromoneDepositApplications: 0,
                LastIterationBestObjective: null);

        context.Start(state);

        long antsConstructed = 0;
        long constructionSteps = 0;
        long transitionEvaluations = 0;
        long depositApplications = 0;

        for (int iteration = 1;
             iteration <= parameters.MaximumIterations;
             iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var colony =
                new List<CompletedAnt>(
                    parameters.AntCount);

            double? iterationBest = null;

            for (int ant = 0;
                 ant < parameters.AntCount;
                 ant++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                AntColonyConstructionResult<TSolution, TPheromoneKey> construction =
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

                state =
                    new AntSystemState(
                        iteration - 1,
                        antsConstructed,
                        constructionSteps,
                        transitionEvaluations,
                        pheromones.Count,
                        pheromones.EvaporationRounds,
                        depositApplications,
                        iterationBest);

                double objective =
                    context.Evaluate(
                        construction.Solution,
                        state);

                if (!iterationBest.HasValue ||
                    problem.Sense.IsBetter(
                        objective,
                        iterationBest.Value))
                {
                    iterationBest = objective;
                }

                state =
                    new AntSystemState(
                        iteration - 1,
                        antsConstructed,
                        constructionSteps,
                        transitionEvaluations,
                        pheromones.Count,
                        pheromones.EvaporationRounds,
                        depositApplications,
                        iterationBest);

                colony.Add(
                    new CompletedAnt(
                        construction.Solution,
                        construction.PheromoneKeys,
                        objective));

                StoppingDecision stop =
                    context.EvaluateStopping(state);

                if (stop.ShouldStop)
                {
                    return context.Complete(stop, state);
                }
            }

            pheromones.Evaporate();

            for (int ant = 0;
                 ant < colony.Count;
                 ant++)
            {
                CompletedAnt completed =
                    colony[ant];

                double deposit =
                    _depositPolicy.GetDeposit(
                        completed.Solution,
                        completed.Objective,
                        ant,
                        parameters.AntCount,
                        problem);

                if (!double.IsFinite(deposit) ||
                    deposit < 0.0)
                {
                    throw new InvalidOperationException(
                        $"Ant System deposit policy '{_depositPolicy.Id}' returned an invalid deposit.");
                }

                foreach (TPheromoneKey key in completed.PheromoneKeys)
                {
                    pheromones.Deposit(key, deposit);
                    depositApplications++;
                }
            }

            state =
                new AntSystemState(
                    iteration,
                    antsConstructed,
                    constructionSteps,
                    transitionEvaluations,
                    pheromones.Count,
                    pheromones.EvaporationRounds,
                    depositApplications,
                    iterationBest);

            context.CompleteIteration(
                iterationBest,
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
                "MaximumAntSystemIterations",
                "The configured Ant System iteration limit was reached."),
            state);
    }

    private sealed record CompletedAnt(
        TSolution Solution,
        IReadOnlyList<TPheromoneKey> PheromoneKeys,
        double Objective);
}
