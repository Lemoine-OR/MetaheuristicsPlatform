using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class RestartIteratedLocalSearchTests
{
    [Fact]
    public void MultiStartRetainsBestSolutionAcrossIndependentStarts()
    {
        var generator = new SequenceGenerator(0, 4, 5);
        var optimizer = new MultiStartLocalSearchOptimizer<int>(
            generator,
            new IdentityLocalSearch());

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(5),
            new MultiStartLocalSearchParameters { MaximumStarts = 3 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(5, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
        Assert.Equal("MaximumStarts", result.StopDecision.Criterion);
        Assert.Equal(3, result.Statistics.Evaluations);
    }

    [Fact]
    public void IlsPerturbationCanEscapeInitialBasin()
    {
        var optimizer = new IteratedLocalSearchOptimizer<int>(
            Initial(0),
            new TwoBasinLocalSearch(),
            new DelegateSolutionPerturbation<int>(
                static (ref int solution, IOptimizationProblem<int> _, MetaheuristicsPlatform.Random.IRandomSource __) =>
                    solution += 6));

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(10),
            new IteratedLocalSearchParameters
            {
                MaximumIterations = 1,
                Acceptance = NeighborhoodAcceptanceKind.ImprovingOnly
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(10, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
        Assert.Equal("MaximumIlsIterations", result.StopDecision.Criterion);
    }

    [Fact]
    public void IlsAlwaysAcceptanceNeverLosesBestSoFar()
    {
        var optimizer = new IteratedLocalSearchOptimizer<int>(
            Initial(10),
            new IdentityLocalSearch(),
            new DelegateSolutionPerturbation<int>(
                static (ref int solution, IOptimizationProblem<int> _, MetaheuristicsPlatform.Random.IRandomSource __) =>
                    solution = 0));

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(10),
            new IteratedLocalSearchParameters
            {
                MaximumIterations = 1,
                Acceptance = NeighborhoodAcceptanceKind.Always
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(10, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
    }

    [Fact]
    public void RestartAndIlsParametersRejectInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MultiStartLocalSearchParameters { MaximumStarts = 0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IteratedLocalSearchParameters { MaximumIterations = 0 }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IteratedLocalSearchParameters
            {
                Acceptance = (NeighborhoodAcceptanceKind)999
            }.Validate());
    }

    [Fact]
    public void CatalogContainsRestartAndIlsStableIds()
    {
        string[] ids =
        [
            "multi-start-local-search",
            "iterated-local-search-lourenco-martin-stutzle"
        ];

        foreach (string id in ids)
        {
            MetaheuristicCatalogEntry entry = MetaheuristicCatalog.GetRequired(id);
            Assert.Equal(id, entry.Id);
            Assert.True(entry.RequiresComposition);
        }
    }

    [Fact]
    public void PublicAlgorithmIdsExposeRestartAndIls()
    {
        Assert.Equal(
            "multi-start-local-search",
            MetaheuristicAlgorithmIds.MultiStartLocalSearch);
        Assert.Equal(
            "iterated-local-search-lourenco-martin-stutzle",
            MetaheuristicAlgorithmIds.IteratedLocalSearch);
    }

    [Fact]
    public void IlsDescriptorCarriesCanonicalScientificReference()
    {
        var optimizer = new IteratedLocalSearchOptimizer<int>(
            Initial(0),
            new IdentityLocalSearch(),
            new DelegateSolutionPerturbation<int>(
                static (ref int solution, IOptimizationProblem<int> _, MetaheuristicsPlatform.Random.IRandomSource __) =>
                    solution++));

        Assert.Contains(
            optimizer.Descriptor.References,
            reference => reference.Doi == "10.1007/0-306-48056-5_11");
    }

    private static INeighborhoodSearchInitialSolutionGenerator<int> Initial(int value) =>
        new DelegateNeighborhoodSearchInitialSolutionGenerator<int>((_, _) => value);

    private sealed class SequenceGenerator : INeighborhoodSearchInitialSolutionGenerator<int>
    {
        private readonly int[] _values;
        private int _index;

        public SequenceGenerator(params int[] values) => _values = values;

        public int Create(
            IOptimizationProblem<int> problem,
            MetaheuristicsPlatform.Random.IRandomSource random)
        {
            int index = Math.Min(_index, _values.Length - 1);
            _index++;
            return _values[index];
        }
    }

    private sealed class IdentityLocalSearch : ILocalSearchProcedure<int>
    {
        public LocalSearchProcedureResult Improve(
            ref int solution,
            double currentFitness,
            OptimizationContext<int> context,
            ISolutionCloner<int> solutionCloner,
            CancellationToken cancellationToken) =>
            new(
                currentFitness,
                acceptedMoves: 0,
                localOptimum: true,
                StoppingDecision.Continue("LocalOptimum"));
    }

    private sealed class TwoBasinLocalSearch : ILocalSearchProcedure<int>
    {
        public LocalSearchProcedureResult Improve(
            ref int solution,
            double currentFitness,
            OptimizationContext<int> context,
            ISolutionCloner<int> solutionCloner,
            CancellationToken cancellationToken)
        {
            int localOptimum = solution >= 6 ? 10 : 0;
            if (solution == localOptimum)
            {
                return new LocalSearchProcedureResult(
                    currentFitness,
                    0,
                    localOptimum: true,
                    StoppingDecision.Continue("LocalOptimum"));
            }

            solution = localOptimum;
            double fitness = context.Evaluate(solution);
            return new LocalSearchProcedureResult(
                fitness,
                1,
                localOptimum: true,
                StoppingDecision.Continue("LocalOptimum"));
        }
    }

    private sealed class QuadraticProblem : IOptimizationProblem<int>
    {
        private readonly int _target;

        public QuadraticProblem(int target) => _target = target;

        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            (solution - _target) * (double)(solution - _target);
    }
}
