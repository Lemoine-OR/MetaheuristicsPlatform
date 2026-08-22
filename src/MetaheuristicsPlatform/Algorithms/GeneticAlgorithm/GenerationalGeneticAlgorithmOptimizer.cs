using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;

/// <summary>
/// Generic fixed-size generational Genetic Algorithm.
/// Representation-specific initialization, crossover and mutation are injected;
/// tournament selection is provided as the canonical foundation selector.
/// </summary>
public sealed class GenerationalGeneticAlgorithmOptimizer<TSolution> :
    IMetaheuristic<TSolution,GeneticAlgorithmParameters>
{
    private readonly IGeneticPopulationInitializer<TSolution> _initializer;
    private readonly IGeneticParentSelectionMethod<TSolution> _parentSelection;
    private readonly IGeneticCrossoverMethod<TSolution> _crossover;
    private readonly IGeneticMutationMethod<TSolution> _mutation;

    public GenerationalGeneticAlgorithmOptimizer(
        IGeneticPopulationInitializer<TSolution> initializer,
        IGeneticCrossoverMethod<TSolution> crossover,
        IGeneticMutationMethod<TSolution> mutation,
        int tournamentSize = 2)
        : this(
            initializer,
            new TournamentGeneticParentSelectionMethod<TSolution>(
                tournamentSize),
            crossover,
            mutation)
    {
    }

    public GenerationalGeneticAlgorithmOptimizer(
        IGeneticPopulationInitializer<TSolution> initializer,
        IGeneticParentSelectionMethod<TSolution> parentSelection,
        IGeneticCrossoverMethod<TSolution> crossover,
        IGeneticMutationMethod<TSolution> mutation)
    {
        _initializer =
            initializer ??
            throw new ArgumentNullException(nameof(initializer));

        _parentSelection =
            parentSelection ??
            throw new ArgumentNullException(nameof(parentSelection));

        _crossover =
            crossover ??
            throw new ArgumentNullException(nameof(crossover));

        _mutation =
            mutation ??
            throw new ArgumentNullException(nameof(mutation));
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.GeneticAlgorithm,
        Name = "Generational Genetic Algorithm",
        Acronym = "GA",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families = MetaheuristicFamily.Evolutionary,
        Mechanisms =
            MetaheuristicMechanism.EvolutionaryOperators,
        SearchSpaces =
            SearchSpaceKind.Continuous |
            SearchSpaceKind.Binary |
            SearchSpaceKind.Integer |
            SearchSpaceKind.Permutation |
            SearchSpaceKind.Combinatorial |
            SearchSpaceKind.Mixed,
        IsStochastic = true,
        References =
        [
            GeneticAlgorithmReferences.EibenSmith2003,
            GeneticAlgorithmReferences.Whitley1994,
            GeneticAlgorithmReferences.BlickleThiele1996
        ]
    };

    public GeneticAlgorithmParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        GeneticAlgorithmParameters parameters,
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

        long offspringEvaluated = 0;
        long parentSelections = 0;
        long crossoverEvents = 0;
        long mutationEvents = 0;

        var state =
            new GeneticAlgorithmState(
                0,
                0,
                offspringEvaluated,
                parentSelections,
                crossoverEvents,
                mutationEvents,
                parameters.EliteCount);

        context.Start(state);

        var population =
            new List<GeneticPopulationMember<TSolution>>(
                parameters.PopulationSize);

        for (int index = 0;
             index < parameters.PopulationSize;
             index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TSolution generated =
                _initializer.Create(
                    problem,
                    context.Random);

            TSolution owned =
                solutionCloner.Clone(generated);

            double objective =
                context.Evaluate(
                    owned,
                    state);

            population.Add(
                new GeneticPopulationMember<TSolution>(
                    owned,
                    objective));

            state =
                new GeneticAlgorithmState(
                    0,
                    population.Count,
                    offspringEvaluated,
                    parentSelections,
                    crossoverEvents,
                    mutationEvents,
                    parameters.EliteCount);

            StoppingDecision initializationStop =
                context.EvaluateStopping(state);

            if (initializationStop.ShouldStop)
                return context.Complete(initializationStop, state);
        }

        StoppingDecision stop =
            context.EvaluateStopping(state);

        if (stop.ShouldStop)
            return context.Complete(stop, state);

        for (int generation = 1;
             generation <= parameters.MaximumGenerations;
             generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nextPopulation =
                new List<GeneticPopulationMember<TSolution>>(
                    parameters.PopulationSize);

            CopyElites(
                population,
                nextPopulation,
                parameters.EliteCount,
                problem.Sense,
                solutionCloner);

            while (nextPopulation.Count < parameters.PopulationSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int firstParentIndex =
                    SelectParentIndex(
                        population,
                        problem.Sense,
                        context.Random);

                int secondParentIndex =
                    SelectParentIndex(
                        population,
                        problem.Sense,
                        context.Random);

                parentSelections += 2;

                TSolution firstParent =
                    solutionCloner.Clone(
                        population[firstParentIndex].Solution);

                TSolution secondParent =
                    solutionCloner.Clone(
                        population[secondParentIndex].Solution);

                GeneticOffspringPair<TSolution> rawOffspring;

                if (ShouldApply(
                        parameters.CrossoverProbability,
                        context.Random))
                {
                    rawOffspring =
                        _crossover.Crossover(
                            firstParent,
                            secondParent,
                            problem,
                            context.Random);

                    crossoverEvents++;
                }
                else
                {
                    rawOffspring =
                        new GeneticOffspringPair<TSolution>(
                            firstParent,
                            secondParent);
                }

                GeneticPopulationMember<TSolution> firstChild =
                    EvaluateOffspring(
                        rawOffspring.First,
                        problem,
                        parameters,
                        solutionCloner,
                        context,
                        generation,
                        nextPopulation.Count,
                        offspringEvaluated,
                        parentSelections,
                        crossoverEvents,
                        ref mutationEvents,
                        out state,
                        out stop);

                offspringEvaluated++;
                nextPopulation.Add(firstChild);

                if (stop.ShouldStop)
                    return context.Complete(stop, state);

                if (nextPopulation.Count >= parameters.PopulationSize)
                    break;

                GeneticPopulationMember<TSolution> secondChild =
                    EvaluateOffspring(
                        rawOffspring.Second,
                        problem,
                        parameters,
                        solutionCloner,
                        context,
                        generation,
                        nextPopulation.Count,
                        offspringEvaluated,
                        parentSelections,
                        crossoverEvents,
                        ref mutationEvents,
                        out state,
                        out stop);

                offspringEvaluated++;
                nextPopulation.Add(secondChild);

                if (stop.ShouldStop)
                    return context.Complete(stop, state);
            }

            population =
                nextPopulation;

            state =
                new GeneticAlgorithmState(
                    generation,
                    population.Count,
                    offspringEvaluated,
                    parentSelections,
                    crossoverEvents,
                    mutationEvents,
                    parameters.EliteCount);

            double generationBest =
                BestObjective(
                    population,
                    problem.Sense);

            context.CompleteIteration(
                generationBest,
                state);

            stop =
                context.EvaluateStopping(state);

            if (stop.ShouldStop)
                return context.Complete(stop, state);

            if (generation == parameters.MaximumGenerations)
            {
                return context.Complete(
                    StoppingDecision.Stop(
                        "MaximumGenerations",
                        $"The configured maximum of {parameters.MaximumGenerations} GA generations was reached."),
                    state);
            }
        }

        throw new InvalidOperationException(
            "The GA generation loop terminated unexpectedly.");
    }

    private int SelectParentIndex(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense,
        IRandomSource random)
    {
        int selected =
            _parentSelection.SelectParent(
                population,
                sense,
                random);

        if ((uint)selected >= (uint)population.Count)
        {
            throw new InvalidOperationException(
                "The parent-selection method returned an index outside the current population.");
        }

        return selected;
    }

    private GeneticPopulationMember<TSolution> EvaluateOffspring(
        TSolution rawOffspring,
        IOptimizationProblem<TSolution> problem,
        GeneticAlgorithmParameters parameters,
        ISolutionCloner<TSolution> solutionCloner,
        OptimizationContext<TSolution> context,
        int generation,
        int nextPopulationCount,
        long offspringEvaluated,
        long parentSelections,
        long crossoverEvents,
        ref long mutationEvents,
        out GeneticAlgorithmState state,
        out StoppingDecision stop)
    {
        TSolution candidate =
            solutionCloner.Clone(rawOffspring);

        if (ShouldApply(
                parameters.MutationProbability,
                context.Random))
        {
            candidate =
                _mutation.Mutate(
                    candidate,
                    problem,
                    context.Random);

            mutationEvents++;
        }

        TSolution owned =
            solutionCloner.Clone(candidate);

        var evaluationState =
            new GeneticAlgorithmState(
                generation,
                nextPopulationCount,
                offspringEvaluated,
                parentSelections,
                crossoverEvents,
                mutationEvents,
                parameters.EliteCount);

        double objective =
            context.Evaluate(
                owned,
                evaluationState);

        state =
            new GeneticAlgorithmState(
                generation,
                nextPopulationCount + 1,
                offspringEvaluated + 1,
                parentSelections,
                crossoverEvents,
                mutationEvents,
                parameters.EliteCount);

        stop =
            context.EvaluateStopping(state);

        return new GeneticPopulationMember<TSolution>(
            owned,
            objective);
    }

    private static bool ShouldApply(
        double probability,
        IRandomSource random)
    {
        if (probability <= 0.0)
            return false;

        if (probability >= 1.0)
            return true;

        return random.NextDouble() < probability;
    }

    private static void CopyElites(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        ICollection<GeneticPopulationMember<TSolution>> destination,
        int eliteCount,
        OptimizationSense sense,
        ISolutionCloner<TSolution> solutionCloner)
    {
        if (eliteCount == 0)
            return;

        int[] indices =
            Enumerable
                .Range(0, population.Count)
                .ToArray();

        Array.Sort(
            indices,
            (left, right) =>
                CompareObjectives(
                    population[left].Objective,
                    population[right].Objective,
                    sense));

        for (int eliteIndex = 0;
             eliteIndex < eliteCount;
             eliteIndex++)
        {
            GeneticPopulationMember<TSolution> member =
                population[indices[eliteIndex]];

            destination.Add(
                new GeneticPopulationMember<TSolution>(
                    solutionCloner.Clone(member.Solution),
                    member.Objective));
        }
    }

    private static double BestObjective(
        IReadOnlyList<GeneticPopulationMember<TSolution>> population,
        OptimizationSense sense)
    {
        double best =
            population[0].Objective;

        for (int index = 1;
             index < population.Count;
             index++)
        {
            if (sense.IsBetter(
                    population[index].Objective,
                    best))
            {
                best =
                    population[index].Objective;
            }
        }

        return best;
    }

    private static int CompareObjectives(
        double left,
        double right,
        OptimizationSense sense)
    {
        if (sense.IsBetter(left, right))
            return -1;

        if (sense.IsBetter(right, left))
            return 1;

        return 0;
    }
}
