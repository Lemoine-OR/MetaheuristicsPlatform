using MetaheuristicsPlatform.Algorithms.GeneticAlgorithm;
using MetaheuristicsPlatform.Algorithms.Memetic;
using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Classification;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Random;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class MemeticAlgorithmTests
{
    [Fact]
    public void DescriptorUsesStableHybridId()
    {
        MemeticAlgorithmOptimizer<int> optimizer =
            CreateOptimizer(
                new SingleStepImprover());

        Assert.Equal(
            MetaheuristicAlgorithmIds.MemeticAlgorithm,
            optimizer.Descriptor.Id);

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Evolutionary));

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.LocalSearch));

        Assert.True(
            optimizer.Descriptor.Families.HasFlag(
                MetaheuristicFamily.Hybrid));

        Assert.Contains(
            optimizer.Descriptor.References,
            reference =>
                reference.Doi ==
                "10.1109/TEVC.2005.850260");
    }

    [Fact]
    public void DefaultParametersAreValid()
    {
        new MemeticAlgorithmParameters().Validate();
    }

    [Fact]
    public void PolicyConstructorsRejectInvalidParameters()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new PeriodicMemeticLocalSearchPolicy(0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ProbabilisticMemeticLocalSearchPolicy(-0.1));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TopFractionMemeticLocalSearchPolicy(0.0));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new StagnationAdaptiveMemeticLocalSearchPolicy(
                minimumProbability: 0.8,
                maximumProbability: 0.2));
    }

    [Fact]
    public void DisabledLocalSearchMatchesGaEvaluationAccounting()
    {
        var localSearch =
            new SingleStepImprover();

        MemeticAlgorithmOptimizer<int> optimizer =
            CreateOptimizer(
                localSearch,
                new ProbabilisticMemeticLocalSearchPolicy(0.0));

        OptimizationResult<int> result =
            Run(
                optimizer,
                generations: 1,
                populationSize: 4);

        Assert.Equal(0, localSearch.Calls);
        Assert.Equal(8L, result.Statistics.Evaluations);
    }

    [Fact]
    public void EveryOffspringRunsLocalSearchOnAllGeneratedChildren()
    {
        var localSearch =
            new SingleStepImprover();

        MemeticAlgorithmOptimizer<int> optimizer =
            CreateOptimizer(
                localSearch,
                new EveryOffspringMemeticLocalSearchPolicy());

        OptimizationResult<int> result =
            Run(
                optimizer,
                generations: 1,
                populationSize: 4);

        Assert.Equal(4, localSearch.Calls);
        Assert.Equal(12L, result.Statistics.Evaluations);
    }

    [Fact]
    public void PeriodicPolicyRunsOnlyOnMatchingGeneration()
    {
        var localSearch =
            new SingleStepImprover();

        MemeticAlgorithmOptimizer<int> optimizer =
            CreateOptimizer(
                localSearch,
                new PeriodicMemeticLocalSearchPolicy(2));

        _ = Run(
            optimizer,
            generations: 2,
            populationSize: 4);

        Assert.Equal(4, localSearch.Calls);
    }

    [Fact]
    public void TopFractionPolicyImprovesOnlyBestHalfOfOffspring()
    {
        var localSearch =
            new SingleStepImprover();

        MemeticAlgorithmOptimizer<int> optimizer =
            CreateOptimizer(
                localSearch,
                new TopFractionMemeticLocalSearchPolicy(0.5));

        _ = Run(
            optimizer,
            generations: 1,
            populationSize: 4);

        Assert.Equal(2, localSearch.Calls);
    }

    [Fact]
    public void LocalSearchEvaluationsConsumeGlobalBudget()
    {
        var localSearch =
            new SingleStepImprover();

        MemeticAlgorithmOptimizer<int> optimizer =
            CreateOptimizer(
                localSearch);

        OptimizationResult<int> result =
            optimizer.Optimize(
                new LinearProblem(),
                Parameters(
                    populationSize: 2,
                    generations: 10),
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(5),
                cancellationToken:
                    TestContext.Current.CancellationToken);

        Assert.Equal(5L, result.Statistics.Evaluations);
        Assert.Equal(1, localSearch.Calls);
    }

    [Fact]
    public void LamarckianLearningPassesImprovedGenotypeToNextGeneration()
    {
        var crossover =
            new RecordingCrossover();

        MemeticAlgorithmOptimizer<int> optimizer =
            CreateOptimizer(
                new SingleStepImprover(step: 5),
                crossover: crossover,
                learningPolicy:
                    new LamarckianMemeticLearningPolicy());

        _ = Run(
            optimizer,
            generations: 2,
            populationSize: 2);

        Assert.True(crossover.FirstParents.Count >= 2);
        Assert.Equal(10, crossover.FirstParents[0]);
        Assert.Equal(5, crossover.FirstParents[1]);
    }

    [Fact]
    public void BaldwinianLearningKeepsGenotypeWhileSelectionUsesLearnedFitness()
    {
        var crossover =
            new RecordingCrossover();

        MemeticAlgorithmOptimizer<int> optimizer =
            CreateOptimizer(
                new SingleStepImprover(step: 5),
                crossover: crossover,
                learningPolicy:
                    new BaldwinianMemeticLearningPolicy());

        _ = Run(
            optimizer,
            generations: 2,
            populationSize: 2);

        Assert.True(crossover.FirstParents.Count >= 2);
        Assert.Equal(10, crossover.FirstParents[0]);
        Assert.Equal(10, crossover.FirstParents[1]);
    }

    [Fact]
    public void WorseningLocalSearchIsRejected()
    {
        MemeticAlgorithmOptimizer<int> optimizer =
            CreateOptimizer(
                new WorseningLocalSearch());

        Assert.Throws<InvalidOperationException>(() =>
            Run(
                optimizer,
                generations: 1,
                populationSize: 2));
    }

    [Fact]
    public void SameSeedProducesSameMemeticResult()
    {
        OptimizationResult<int> first =
            RunSeeded(123456789UL);

        OptimizationResult<int> second =
            RunSeeded(123456789UL);

        Assert.Equal(
            first.BestFitness,
            second.BestFitness);

        Assert.Equal(
            first.BestSolution,
            second.BestSolution);

        Assert.Equal(
            first.Statistics.Evaluations,
            second.Statistics.Evaluations);
    }

    [Fact]
    public void StableIdSupportsTypedFactoryRegistration()
    {
        MemeticAlgorithmOptimizer<int> optimizer =
            CreateOptimizer(
                new SingleStepImprover());

        MetaheuristicFactory.Register(
            MetaheuristicAlgorithmIds.MemeticAlgorithm,
            () => optimizer,
            replace: true);

        MemeticAlgorithmOptimizer<int> created =
            MetaheuristicFactory.Create<
                MemeticAlgorithmOptimizer<int>>(
                MetaheuristicAlgorithmIds.MemeticAlgorithm);

        Assert.Same(optimizer, created);
    }

    [Fact]
    public void BaldwinianLearningDoesNotMutateInheritedArrayGenotype()
    {
        var crossover =
            new RecordingArrayCrossover();

        var optimizer =
            new MemeticAlgorithmOptimizer<int[]>(
                new ArrayInitializer(),
                new FixedArrayParentSelection(),
                crossover,
                new NoOpArrayMutation(),
                new ArrayImprover(),
                new EveryOffspringMemeticLocalSearchPolicy(),
                new BaldwinianMemeticLearningPolicy());

        _ = optimizer.Optimize(
            new ArrayProblem(),
            new MemeticAlgorithmParameters
            {
                GeneticAlgorithm =
                    new GeneticAlgorithmParameters
                    {
                        PopulationSize = 2,
                        MaximumGenerations = 2,
                        CrossoverProbability = 1.0,
                        MutationProbability = 0.0
                    }
            },
            new ArraySolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken:
                TestContext.Current.CancellationToken);

        Assert.True(crossover.FirstParents.Count >= 2);
        Assert.Equal(10, crossover.FirstParents[1][0]);
    }

    private static MemeticAlgorithmOptimizer<int> CreateOptimizer(
        ILocalSearchProcedure<int> localSearch,
        IMemeticLocalSearchPolicy? localSearchPolicy = null,
        IGeneticCrossoverMethod<int>? crossover = null,
        IMemeticLearningPolicy? learningPolicy = null) =>
        new(
            new FixedInitializer(),
            new FirstParentSelection(),
            crossover ?? new RecordingCrossover(),
            new NoOpMutation(),
            localSearch,
            localSearchPolicy,
            learningPolicy);

    private static MemeticAlgorithmParameters Parameters(
        int populationSize,
        int generations) =>
        new()
        {
            GeneticAlgorithm =
                new GeneticAlgorithmParameters
                {
                    PopulationSize = populationSize,
                    MaximumGenerations = generations,
                    CrossoverProbability = 1.0,
                    MutationProbability = 0.0,
                    EliteCount = 0
                }
        };

    private static OptimizationResult<int> Run(
        MemeticAlgorithmOptimizer<int> optimizer,
        int generations,
        int populationSize) =>
        optimizer.Optimize(
            new LinearProblem(),
            Parameters(
                populationSize,
                generations),
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(1000),
            cancellationToken:
                TestContext.Current.CancellationToken);

    private static OptimizationResult<int> RunSeeded(
        ulong seed)
    {
        var optimizer =
            new MemeticAlgorithmOptimizer<int>(
                new RandomInitializer(),
                new TournamentGeneticParentSelectionMethod<int>(2),
                new RecordingCrossover(),
                new RandomStepMutation(),
                new SingleStepImprover(),
                new StagnationAdaptiveMemeticLocalSearchPolicy(
                    minimumProbability: 0.25,
                    maximumProbability: 1.0,
                    stagnationWindow: 2),
                new LamarckianMemeticLearningPolicy());

        return optimizer.Optimize(
            new LinearProblem(),
            Parameters(
                populationSize: 6,
                generations: 3),
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(1000),
            new OptimizationOptions
            {
                Seed = seed
            },
            cancellationToken:
                TestContext.Current.CancellationToken);
    }

    private sealed class LinearProblem :
        IOptimizationProblem<int>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            int solution) =>
            solution;
    }

    private sealed class FixedInitializer :
        IGeneticPopulationInitializer<int>
    {
        private int _index;

        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            int result =
                _index == 0
                    ? 10
                    : 20 + _index;

            _index++;

            return result;
        }
    }

    private sealed class RandomInitializer :
        IGeneticPopulationInitializer<int>
    {
        public int Create(
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            random.NextInt32(10, 101);
    }

    private sealed class FirstParentSelection :
        IGeneticParentSelectionMethod<int>
    {
        public int SelectParent(
            IReadOnlyList<GeneticPopulationMember<int>> population,
            OptimizationSense sense,
            IRandomSource random) =>
            0;
    }

    private sealed class RecordingCrossover :
        IGeneticCrossoverMethod<int>
    {
        public List<int> FirstParents { get; } = new();

        public GeneticOffspringPair<int> Crossover(
            int firstParent,
            int secondParent,
            IOptimizationProblem<int> problem,
            IRandomSource random)
        {
            FirstParents.Add(firstParent);

            return new(
                firstParent,
                firstParent);
        }
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

    private sealed class RandomStepMutation :
        IGeneticMutationMethod<int>
    {
        public int Mutate(
            int solution,
            IOptimizationProblem<int> problem,
            IRandomSource random) =>
            solution -
            random.NextInt32(0, 3);
    }

    private sealed class SingleStepImprover :
        ILocalSearchProcedure<int>
    {
        private readonly int _step;

        public SingleStepImprover(
            int step = 1)
        {
            _step = step;
        }

        public int Calls { get; private set; }

        public LocalSearchProcedureResult Improve(
            ref int solution,
            double currentFitness,
            OptimizationContext<int> context,
            ISolutionCloner<int> solutionCloner,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Calls++;
            solution -= _step;

            double objective =
                context.Evaluate(solution);

            StoppingDecision stop =
                context.EvaluateStopping();

            return new LocalSearchProcedureResult(
                objective,
                acceptedMoves: 1,
                localOptimum: true,
                stop);
        }
    }

    private sealed class WorseningLocalSearch :
        ILocalSearchProcedure<int>
    {
        public LocalSearchProcedureResult Improve(
            ref int solution,
            double currentFitness,
            OptimizationContext<int> context,
            ISolutionCloner<int> solutionCloner,
            CancellationToken cancellationToken)
        {
            solution += 1;

            double objective =
                context.Evaluate(solution);

            return new LocalSearchProcedureResult(
                objective,
                acceptedMoves: 1,
                localOptimum: true,
                StoppingDecision.Continue("Worsened"));
        }
    }

    private sealed class ArrayProblem :
        IOptimizationProblem<int[]>
    {
        public OptimizationSense Sense =>
            OptimizationSense.Minimize;

        public double Evaluate(
            int[] solution) =>
            solution[0];
    }

    private sealed class ArrayInitializer :
        IGeneticPopulationInitializer<int[]>
    {
        private int _next = 10;

        public int[] Create(
            IOptimizationProblem<int[]> problem,
            IRandomSource random)
        {
            int[] result =
                new[] { _next };

            _next += 10;
            return result;
        }
    }

    private sealed class FixedArrayParentSelection :
        IGeneticParentSelectionMethod<int[]>
    {
        public int SelectParent(
            IReadOnlyList<GeneticPopulationMember<int[]>> population,
            OptimizationSense sense,
            IRandomSource random) =>
            0;
    }

    private sealed class RecordingArrayCrossover :
        IGeneticCrossoverMethod<int[]>
    {
        public List<int[]> FirstParents { get; } = new();

        public GeneticOffspringPair<int[]> Crossover(
            int[] firstParent,
            int[] secondParent,
            IOptimizationProblem<int[]> problem,
            IRandomSource random)
        {
            FirstParents.Add(
                (int[])firstParent.Clone());

            return new(
                (int[])firstParent.Clone(),
                (int[])firstParent.Clone());
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

    private sealed class ArrayImprover :
        ILocalSearchProcedure<int[]>
    {
        public LocalSearchProcedureResult Improve(
            ref int[] solution,
            double currentFitness,
            OptimizationContext<int[]> context,
            ISolutionCloner<int[]> solutionCloner,
            CancellationToken cancellationToken)
        {
            solution[0] -= 5;

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
