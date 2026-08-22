using MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Callbacks;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Algorithms.Memetic;

/// <summary>
/// Canonical GA-backed memetic algorithm: shared generational evolutionary search plus
/// configurable local improvement and Lamarckian/Baldwinian learning.
/// </summary>
public sealed class MemeticAlgorithmOptimizer<TSolution> :
    IMetaheuristic<TSolution,MemeticAlgorithmParameters>
{
    private readonly MemeticGeneticExecutionExtension<TSolution> _memeticExtension;
    private readonly GenerationalGeneticAlgorithmOptimizer<TSolution> _engine;

    public MemeticAlgorithmOptimizer(
        IGeneticPopulationInitializer<TSolution> initializer,
        IGeneticCrossoverMethod<TSolution> crossover,
        IGeneticMutationMethod<TSolution> mutation,
        ILocalSearchProcedure<TSolution> localSearch,
        IMemeticLocalSearchPolicy? localSearchPolicy = null,
        IMemeticLearningPolicy? learningPolicy = null,
        int tournamentSize = 2)
        : this(
            initializer,
            new TournamentGeneticParentSelectionMethod<TSolution>(
                tournamentSize),
            crossover,
            mutation,
            localSearch,
            localSearchPolicy,
            learningPolicy)
    {
    }

    public MemeticAlgorithmOptimizer(
        IGeneticPopulationInitializer<TSolution> initializer,
        IGeneticParentSelectionMethod<TSolution> parentSelection,
        IGeneticCrossoverMethod<TSolution> crossover,
        IGeneticMutationMethod<TSolution> mutation,
        ILocalSearchProcedure<TSolution> localSearch,
        IMemeticLocalSearchPolicy? localSearchPolicy = null,
        IMemeticLearningPolicy? learningPolicy = null)
    {
        _memeticExtension =
            new MemeticGeneticExecutionExtension<TSolution>(
                localSearch,
                localSearchPolicy ??
                    new EveryOffspringMemeticLocalSearchPolicy(),
                learningPolicy ??
                    new LamarckianMemeticLearningPolicy());

        _engine =
            new GenerationalGeneticAlgorithmOptimizer<TSolution>(
                initializer,
                parentSelection,
                crossover,
                mutation,
                Descriptor,
                _memeticExtension);
    }

    public MetaheuristicDescriptor Descriptor { get; } = new()
    {
        Id = MetaheuristicAlgorithmIds.MemeticAlgorithm,
        Name = "Memetic Algorithm - Moscato",
        Acronym = "MA",
        SolutionModel = MetaheuristicSolutionModel.Population,
        Families =
            MetaheuristicFamily.Evolutionary |
            MetaheuristicFamily.LocalSearch |
            MetaheuristicFamily.Hybrid,
        Mechanisms =
            MetaheuristicMechanism.EvolutionaryOperators |
            MetaheuristicMechanism.Hybrid |
            MetaheuristicMechanism.Adaptive,
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
            MemeticAlgorithmReferences.Moscato1989,
            MemeticAlgorithmReferences.KrasnogorSmith2005
        ]
    };

    public MemeticAlgorithmParameters CreateDefaultParameters() => new();

    public OptimizationResult<TSolution> Optimize(
        IOptimizationProblem<TSolution> problem,
        MemeticAlgorithmParameters parameters,
        ISolutionCloner<TSolution> solutionCloner,
        IStoppingCriterion stoppingCriterion,
        OptimizationOptions? options = null,
        IOptimizationCallback<TSolution>? callback = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        parameters.Validate();

        _memeticExtension.Reset();

        return _engine.Optimize(
            problem,
            parameters.GeneticAlgorithm,
            solutionCloner,
            stoppingCriterion,
            options,
            callback,
            cancellationToken);
    }
}
