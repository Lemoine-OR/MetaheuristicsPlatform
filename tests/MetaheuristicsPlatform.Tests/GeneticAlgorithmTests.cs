using MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class GeneticAlgorithmTests
{
    [Fact]
    public void DescriptorUsesStableIdAndEvolutionaryClassification()
    {
        GenerationalGeneticAlgorithmOptimizer<int> optimizer =
            CreateIntOptimizer();

        Assert.Equal(
            MetaheuristicAlgorithmIds.GeneticAlgorithm,
            optimizer.Descriptor.Id);

        Assert.Equal(
            MetaheuristicSolutionModel.Population,
            optimizer.Descriptor.SolutionModel);

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Evolutionary));

        Assert.True(
            optimizer.Descriptor.Mechanisms.HasFlag(
                MetaheuristicMechanism.EvolutionaryOperators));

        Assert.True(optimizer.Descriptor.IsStochastic);
        Assert.Equal(3, optimizer.Descriptor.References.Count);
        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1007/978-3-662-05094-1_3");
    }

    [Fact]
    public void DefaultParametersAreValid()
    {
        new GeneticAlgorithmParameters().Validate();
    }

    [Fact]
    public void ParametersRejectInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeneticAlgorithmParameters
            {
                PopulationSize = 1
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeneticAlgorithmParameters
            {
                MaximumGenerations = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeneticAlgorithmParameters
            {
                CrossoverProbability = double.NaN
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeneticAlgorithmParameters
            {
                MutationProbability = 1.01
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeneticAlgorithmParameters
            {
                PopulationSize = 4,
                EliteCount = 4
            }.Validate());
    }

    [Fact]
    public void TournamentSelectionHonorsMinimization()
    {
        var selector =
            new TournamentGeneticParentSelectionMethod<int>(
                tournamentSize: 2);

        var population =
            new[]
            {
                new GeneticPopulationMember<int>(0, 10.0),
                new GeneticPopulationMember<int>(1, 5.0)
            };

        int selected =
            selector.SelectParent(
                population,
                OptimizationSense.Minimize,
                new SequenceRandomSource(0, 1));

        Assert.Equal(1, selected);
    }

    [Fact]
    public void TournamentSelectionHonorsMaximization()
    {
        var selector =
            new TournamentGeneticParentSelectionMethod<int>(
                tournamentSize: 2);

        var population =
            new[]
            {
                new GeneticPopulationMember<int>(0, 10.0),
                new GeneticPopulationMember<int>(1, 5.0)
            };

        int selected =
            selector.SelectParent(
                population,
                OptimizationSense.Maximize,
                new SequenceRandomSource(0, 1));

        Assert.Equal(0, selected);
    }

    [Fact]
    public void InitializationStopsAtEvaluationBudgetAndReturnsBest()
    {
        var initializer =
            new CountingInitializer(
                5, 3, 8, 1, 7);

        GenerationalGeneticAlgorithmOptimizer<int> optimizer =
            CreateIntOptimizer(initializer);

        OptimizationResult<int> result =
            optimizer.Optimize(
                new LinearProblem(OptimizationSense.Minimize),
                new GeneticAlgorithmParameters
                {
                    PopulationSize = 5,
                    MaximumGenerations = 10
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(3),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(3L, result.Statistics.Evaluations);
        Assert.Equal(0L, result.Statistics.Iterations);
        Assert.Equal(3.0, result.BestFitness);
        Assert.Equal(3, result.BestSolution);
    }

    [Fact]
    public void OneGenerationWithoutElitismEvaluatesFullOffspringPopulation()
    {
        GenerationalGeneticAlgorithmOptimizer<int> optimizer =
            CreateIntOptimizer(
                new CountingInitializer(0, 1, 2, 3));

        OptimizationResult<int> result =
            RunOneGeneration(
                optimizer,
                populationSize: 4,
                eliteCount: 0);

        Assert.Equal(8L, result.Statistics.Evaluations);
        Assert.Equal(1L, result.Statistics.Iterations);
    }

    [Fact]
    public void ElitismCopiesMembersWithoutReevaluation()
    {
        GenerationalGeneticAlgorithmOptimizer<int> optimizer =
            CreateIntOptimizer(
                new CountingInitializer(0, 1, 2, 3));

        OptimizationResult<int> result =
            RunOneGeneration(
                optimizer,
                populationSize: 4,
                eliteCount: 1);

        Assert.Equal(7L, result.Statistics.Evaluations);
        Assert.Equal(1L, result.Statistics.Iterations);
    }

    [Fact]
    public void OddPopulationSizeIsFilledExactly()
    {
        GenerationalGeneticAlgorithmOptimizer<int> optimizer =
            CreateIntOptimizer(
                new CountingInitializer(0, 1, 2, 3, 4));

        OptimizationResult<int> result =
            RunOneGeneration(
                optimizer,
                populationSize: 5,
                eliteCount: 0);

        Assert.Equal(10L, result.Statistics.Evaluations);
        Assert.Equal(1L, result.Statistics.Iterations);
    }

    [Fact]
    public void ZeroCrossoverProbabilitySkipsCrossover()
    {
        var optimizer =
            new GenerationalGeneticAlgorithmOptimizer<int>(
                new CountingInitializer(0, 1, 2, 3),
                new FixedParentSelection(),
                new ThrowingCrossover(),
                new NoOpMutation());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new LinearProblem(OptimizationSense.Minimize),
                new GeneticAlgorithmParameters
                {
                    PopulationSize = 4,
                    MaximumGenerations = 1,
                    CrossoverProbability = 0.0,
                    MutationProbability = 0.0
                },
                new ImmutableSolutionCloner<int>(),
                new MaxIterationsStoppingCriterion(1),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(8L, result.Statistics.Evaluations);
    }

    [Fact]
    public void ZeroMutationProbabilitySkipsMutation()
    {
        var optimizer =
            new GenerationalGeneticAlgorithmOptimizer<int>(
                new CountingInitializer(0, 1, 2, 3),
                new FixedParentSelection(),
                new CountingCrossover(),
                new ThrowingMutation());

        OptimizationResult<int> result =
            optimizer.Optimize(
                new LinearProblem(OptimizationSense.Minimize),
                new GeneticAlgorithmParameters
                {
                    PopulationSize = 4,
                    MaximumGenerations = 1,
                    CrossoverProbability = 1.0,
                    MutationProbability = 0.0
                },
                new ImmutableSolutionCloner<int>(),
                new MaxIterationsStoppingCriterion(1),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(8L, result.Statistics.Evaluations);
    }

    [Fact]
    public void UnitProbabilitiesInvokeExpectedVariationCounts()
    {
        var crossover =
            new CountingCrossover();

        var mutation =
            new CountingMutation(
                static value => value - 1);

        var selection =
            new FixedParentSelection();

        var optimizer =
            new GenerationalGeneticAlgorithmOptimizer<int>(
                new CountingInitializer(0, 1, 2, 3),
                selection,
                crossover,
                mutation);

        OptimizationResult<int> result =
            optimizer.Optimize(
                new LinearProblem(OptimizationSense.Minimize),
                new GeneticAlgorithmParameters
                {
                    PopulationSize = 4,
                    MaximumGenerations = 1,
                    CrossoverProbability = 1.0,
                    MutationProbability = 1.0
                },
                new ImmutableSolutionCloner<int>(),
                new MaxIterationsStoppingCriterion(1),
                cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(4, selection.Calls);
        Assert.Equal(2, crossover.Calls);
        Assert.Equal(4, mutation.Calls);
        Assert.Equal(8L, result.Statistics.Evaluations);
    }

    [Fact]
    public void SameSeedProducesSameResult()
    {
        OptimizationResult<int> first =
            RunSeeded(seed: 123456789UL);

        OptimizationResult<int> second =
            RunSeeded(seed: 123456789UL);

        Assert.Equal(first.BestFitness, second.BestFitness);
        Assert.Equal(first.BestSolution, second.BestSolution);
        Assert.Equal(
            first.Statistics.Evaluations,
            second.Statistics.Evaluations);
        Assert.Equal(
            first.Statistics.Iterations,
            second.Statistics.Iterations);
    }

    [Fact]
    public void PopulationOwnsInitializerSnapshots()
    {
        var initializer =
            new ReusingArrayInitializer();

        var selection =
            new FixedArrayParentSelection();

        var crossover =
            new RecordingArrayCrossover();

        var optimizer =
            new GenerationalGeneticAlgorithmOptimizer<int[]>(
                initializer,
                selection,
                crossover,
                new NoOpArrayMutation());

        _ = optimizer.Optimize(
            new ArrayLinearProblem(),
            new GeneticAlgorithmParameters
            {
                PopulationSize = 4,
                MaximumGenerations = 1,
                CrossoverProbability = 1.0,
                MutationProbability = 0.0
            },
            new ArraySolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(1),
            cancellationToken: TestContext.Current.CancellationToken);

        int[] observedParents =
            Assert.IsType<int[]>(
                crossover.FirstObservedParents);

        Assert.Equal(
            new[] { 1, 2 },
            observedParents);
    }

    [Fact]
    public void StableIdSupportsTypedFactoryRegistration()
    {
        GenerationalGeneticAlgorithmOptimizer<int> optimizer =
            CreateIntOptimizer();

        MetaheuristicFactory.Register(
            MetaheuristicAlgorithmIds.GeneticAlgorithm,
            () => optimizer,
            replace: true);

        GenerationalGeneticAlgorithmOptimizer<int> created =
            MetaheuristicFactory.Create<
                GenerationalGeneticAlgorithmOptimizer<int>>(
                MetaheuristicAlgorithmIds.GeneticAlgorithm);

        Assert.Same(optimizer, created);
    }

    private static OptimizationResult<int> RunOneGeneration(
        GenerationalGeneticAlgorithmOptimizer<int> optimizer,
        int populationSize,
        int eliteCount) =>
        optimizer.Optimize(
            new LinearProblem(OptimizationSense.Minimize),
            new GeneticAlgorithmParameters
            {
                PopulationSize = populationSize,
                MaximumGenerations = 1,
                CrossoverProbability = 1.0,
                MutationProbability = 0.0,
                EliteCount = eliteCount
            },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(1),
            cancellationToken: TestContext.Current.CancellationToken);

    private static OptimizationResult<int> RunSeeded(
        ulong seed)
    {
        var optimizer =
            new GenerationalGeneticAlgorithmOptimizer<int>(
                new RandomInitializer(),
                new TournamentGeneticParentSelectionMethod<int>(3),
                new AveragingCrossover(),
                new RandomStepMutation());

        return optimizer.Optimize(
            new LinearProblem(OptimizationSense.Minimize),
            new GeneticAlgorithmParameters
            {
                PopulationSize = 6,
                MaximumGenerations = 3,
                CrossoverProbability = 0.8,
                MutationProbability = 0.7,
                EliteCount = 1
            },
            new ImmutableSolutionCloner<int>(),
            new MaxIterationsStoppingCriterion(3),
            new OptimizationOptions
            {
                Seed = seed
            },
            cancellationToken: TestContext.Current.CancellationToken);
    }

    private static GenerationalGeneticAlgorithmOptimizer<int> CreateIntOptimizer(
        IGeneticPopulationInitializer<int>? initializer = null) =>
        new(
            initializer ??
            new CountingInitializer(0, 1, 2, 3),
            new FixedParentSelection(),
            new CountingCrossover(),
            new NoOpMutation());

    private sealed class LinearProblem :
        IOptimizationProblem<int>
    {
        public LinearProblem(
            OptimizationSense sense) =>
            Sense = sense;

        public OptimizationSense Sense { get; }

        public double Evaluate(
            int solution) =>
            solution;
    }

    private sealed class CountingInitializer :
        IGeneticPopulationInitializer<int>
    {
        private readonly int[] _values;
        private int _index;

        public CountingInitializer(
            params int[] values)
        {
            _values = values;
        }

        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            int value =
                _values[_index % _values.Length];

            _index++;

            return value;
        }
    }

    private sealed class RandomInitializer :
        IGeneticPopulationInitializer<int>
    {
        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            random.NextInt32(-100, 101);
    }

    private sealed class FixedParentSelection :
        IGeneticParentSelectionMethod<int>
    {
        private int _next;

        public int Calls { get; private set; }

        public int SelectParent(
            IReadOnlyList<GeneticPopulationMember<int>> population,
            OptimizationSense sense,
            IRandomSource random)
        {
            int selected =
                _next % Math.Min(2, population.Count);

            _next++;
            Calls++;

            return selected;
        }
    }

    private sealed class CountingCrossover :
        IGeneticCrossoverMethod<int>
    {
        public int Calls { get; private set; }

        public GeneticOffspringPair<int> Crossover(
            int firstParent,
            int secondParent,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            Calls++;

            return new GeneticOffspringPair<int>(
                firstParent,
                secondParent);
        }
    }

    private sealed class AveragingCrossover :
        IGeneticCrossoverMethod<int>
    {
        public GeneticOffspringPair<int> Crossover(
            int firstParent,
            int secondParent,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            int midpoint =
                (firstParent + secondParent) / 2;

            return new GeneticOffspringPair<int>(
                midpoint,
                firstParent + secondParent - midpoint);
        }
    }

    private sealed class ThrowingCrossover :
        IGeneticCrossoverMethod<int>
    {
        public GeneticOffspringPair<int> Crossover(
            int firstParent,
            int secondParent,
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            throw new InvalidOperationException(
                "Crossover must not be invoked.");
    }

    private sealed class NoOpMutation :
        IGeneticMutationMethod<int>
    {
        public int Mutate(
            int solution,
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            solution;
    }

    private sealed class CountingMutation :
        IGeneticMutationMethod<int>
    {
        private readonly Func<int,int> _mutation;

        public CountingMutation(
            Func<int,int> mutation) =>
            _mutation = mutation;

        public int Calls { get; private set; }

        public int Mutate(
            int solution,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            Calls++;

            return _mutation(solution);
        }
    }

    private sealed class RandomStepMutation :
        IGeneticMutationMethod<int>
    {
        public int Mutate(
            int solution,
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            solution +
            random.NextInt32(-2, 3);
    }

    private sealed class ThrowingMutation :
        IGeneticMutationMethod<int>
    {
        public int Mutate(
            int solution,
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            throw new InvalidOperationException(
                "Mutation must not be invoked.");
    }

    private sealed class ReusingArrayInitializer :
        IGeneticPopulationInitializer<int[]>
    {
        private readonly int[] _shared = new int[1];
        private int _next = 1;

        public int[] Create(
            IOptimizationProblem<int[]> problem,
            IRandomSource random)
        {
            _shared[0] = _next;
            _next++;

            return _shared;
        }
    }

    private sealed class FixedArrayParentSelection :
        IGeneticParentSelectionMethod<int[]>
    {
        private int _next;

        public int SelectParent(
            IReadOnlyList<GeneticPopulationMember<int[]>> population,
            OptimizationSense sense,
            IRandomSource random)
        {
            int index =
                _next % 2;

            _next++;

            return index;
        }
    }

    private sealed class RecordingArrayCrossover :
        IGeneticCrossoverMethod<int[]>
    {
        public int[]? FirstObservedParents { get; private set; }

        public GeneticOffspringPair<int[]> Crossover(
            int[] firstParent,
            int[] secondParent,
            IOptimizationProblem<int[]> problem,
            IRandomSource random)
        {
            FirstObservedParents ??=
            [
                firstParent[0],
                secondParent[0]
            ];

            return new GeneticOffspringPair<int[]>(
                firstParent,
                secondParent);
        }
    }

    private sealed class NoOpArrayMutation :
        IGeneticMutationMethod<int[]>
    {
        public int[] Mutate(
            int[] solution,
            IOptimizationProblem<int[]> problem,
            IRandomSource random) =>
            solution;
    }

    private sealed class ArrayLinearProblem :
        IOptimizationProblem<int[]>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            int[] solution) =>
            solution[0];
    }

    private sealed class SequenceRandomSource :
        IRandomSource
    {
        private readonly int[] _values;
        private int _index;

        public SequenceRandomSource(
            params int[] values)
        {
            _values = values;
        }

        public ulong Seed => 0UL;

        public ulong NextUInt64() =>
            (ulong)NextInt32(int.MaxValue);

        public double NextDouble() =>
            0.0;

        public int NextInt32(
            int exclusiveMax)
        {
            if (exclusiveMax <= 0)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

            int raw =
                _values[_index % _values.Length];

            _index++;

            return Math.Abs(raw % exclusiveMax);
        }

        public int NextInt32(
            int inclusiveMin,
            int exclusiveMax)
        {
            if (exclusiveMax <= inclusiveMin)
                throw new ArgumentOutOfRangeException(nameof(exclusiveMax));

            return inclusiveMin +
                NextInt32(exclusiveMax - inclusiveMin);
        }

        public void Fill(
            Span<byte> buffer) =>
            buffer.Clear();
    }
}
