using BenchmarkDotNet.Attributes;
using MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;
using MetaheuristicsPlatform.Algorithms.Memetic;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Benchmarks;

[MemoryDiagnoser]
public class MemeticAlgorithmBenchmarks
{
    private readonly RandomInitializer _initializer = new();
    private readonly TournamentGeneticParentSelectionMethod<int> _selection = new(2);
    private readonly IdentityCrossover _crossover = new();
    private readonly NoOpMutation _mutation = new();
    private readonly OneStepLocalSearch _localSearch = new();
    private readonly LinearProblem _problem = new();
    private readonly ImmutableSolutionCloner<int> _cloner = new();

    private readonly GeneticAlgorithmParameters _gaParameters =
        new()
        {
            PopulationSize = 32,
            MaximumGenerations = 5,
            CrossoverProbability = 0.9,
            MutationProbability = 0.0,
            EliteCount = 1
        };

    private readonly MemeticAlgorithmParameters _memeticParameters =
        new()
        {
            GeneticAlgorithm =
                new GeneticAlgorithmParameters
                {
                    PopulationSize = 32,
                    MaximumGenerations = 5,
                    CrossoverProbability = 0.9,
                    MutationProbability = 0.0,
                    EliteCount = 1
                }
        };

    [Benchmark(Baseline = true)]
    public double GenerationalGa()
    {
        var optimizer =
            new GenerationalGeneticAlgorithmOptimizer<int>(
                _initializer,
                _selection,
                _crossover,
                _mutation);

        return optimizer.Optimize(
            _problem,
            _gaParameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(10000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;
    }

    [Benchmark]
    public double MemeticLayerDisabled()
    {
        var optimizer =
            new MemeticAlgorithmOptimizer<int>(
                _initializer,
                _selection,
                _crossover,
                _mutation,
                _localSearch,
                new ProbabilisticMemeticLocalSearchPolicy(0.0),
                new LamarckianMemeticLearningPolicy());

        return optimizer.Optimize(
            _problem,
            _memeticParameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(10000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;
    }

    [Benchmark]
    public double MemeticEveryOffspring()
    {
        var optimizer =
            new MemeticAlgorithmOptimizer<int>(
                _initializer,
                _selection,
                _crossover,
                _mutation,
                _localSearch,
                new EveryOffspringMemeticLocalSearchPolicy(),
                new LamarckianMemeticLearningPolicy());

        return optimizer.Optimize(
            _problem,
            _memeticParameters,
            _cloner,
            new MaxEvaluationsStoppingCriterion(10000),
            new OptimizationOptions { Seed = 123456UL }).BestFitness;
    }

    private sealed class LinearProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;
        public double Evaluate(int solution) => solution;
    }

    private sealed class RandomInitializer : IGeneticPopulationInitializer<int>
    {
        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            random.NextInt32(0, 10000);
    }

    private sealed class IdentityCrossover : IGeneticCrossoverMethod<int>
    {
        public GeneticOffspringPair<int> Crossover(
            int firstParent,
            int secondParent,
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            new(firstParent, secondParent);
    }

    private sealed class NoOpMutation : IGeneticMutationMethod<int>
    {
        public int Mutate(
            int solution,
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            solution;
    }

    private sealed class OneStepLocalSearch : ILocalSearchProcedure<int>
    {
        public LocalSearchProcedureResult Improve(
            ref int solution,
            double currentFitness,
            OptimizationContext<int> context,
            ISolutionCloner<int> solutionCloner,
            CancellationToken cancellationToken)
        {
            solution--;

            double objective =
                context.Evaluate(solution);

            return new LocalSearchProcedureResult(
                objective,
                acceptedMoves: 1,
                localOptimum: true,
                context.EvaluateStopping());
        }
    }
}
