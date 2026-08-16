using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class VariableNeighborhoodSearchTests
{
    [Fact]
    public void VndRestartsAtFirstNeighborhoodAfterImprovement()
    {
        var first = new ConditionalLocalSearch(from: 5, to: 10);
        var second = new ConditionalLocalSearch(from: 0, to: 5);

        var optimizer = new VariableNeighborhoodDescentOptimizer<int>(
            Initial(0),
            new ILocalSearchProcedure<int>[] { first, second });

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(10),
            new VariableNeighborhoodDescentParameters
            {
                MaximumNeighborhoodRestarts = 10
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(10, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
        Assert.Equal("VariableNeighborhoodLocalOptimum", result.StopDecision.Criterion);
        Assert.Equal(3, first.Calls);
        Assert.Equal(2, second.Calls);
    }

    [Fact]
    public void ReusableVndProcedureCanBeComposedInsideVns()
    {
        var vnd = new VariableNeighborhoodDescentProcedure<int>(
            new ILocalSearchProcedure<int>[]
            {
                new ConditionalLocalSearch(from: 6, to: 10),
                new IdentityLocalSearch()
            });

        var optimizer = new VariableNeighborhoodSearchOptimizer<int>(
            Initial(0),
            new ISolutionPerturbation<int>[]
            {
                Perturb(static (ref int x) => x = 6)
            },
            vnd);

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(10),
            new VariableNeighborhoodSearchParameters { MaximumCycles = 1 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(10, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
    }

    [Fact]
    public void VnsResetsShakingNeighborhoodAfterStrictImprovement()
    {
        var first = new CountingPerturbation(static (ref int _) => { });
        var second = new CountingPerturbation(static (ref int x) => x = 6);

        var optimizer = new VariableNeighborhoodSearchOptimizer<int>(
            Initial(0),
            new ISolutionPerturbation<int>[] { first, second },
            new TwoBasinLocalSearch());

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(10),
            new VariableNeighborhoodSearchParameters { MaximumCycles = 1 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(10, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
        Assert.Equal("MaximumVnsCycles", result.StopDecision.Criterion);
        Assert.Equal(2, first.Calls);
        Assert.Equal(2, second.Calls);
    }

    [Fact]
    public void VnsNeverLosesBestSoFarWhenLaterShakesAreWorse()
    {
        var optimizer = new VariableNeighborhoodSearchOptimizer<int>(
            Initial(10),
            new ISolutionPerturbation<int>[]
            {
                Perturb(static (ref int x) => x = 0)
            },
            new IdentityLocalSearch());

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(10),
            new VariableNeighborhoodSearchParameters { MaximumCycles = 2 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(10, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
    }

    [Fact]
    public void VariableNeighborhoodParametersRejectInvalidCaps()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VariableNeighborhoodDescentParameters
            {
                MaximumNeighborhoodRestarts = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new VariableNeighborhoodSearchParameters
            {
                MaximumCycles = 0
            }.Validate());
    }

    [Fact]
    public void VndAndVnsRequireAtLeastOneNeighborhood()
    {
        Assert.Throws<ArgumentException>(() =>
            new VariableNeighborhoodDescentOptimizer<int>(
                Initial(0),
                Array.Empty<ILocalSearchProcedure<int>>()));

        Assert.Throws<ArgumentException>(() =>
            new VariableNeighborhoodSearchOptimizer<int>(
                Initial(0),
                Array.Empty<ISolutionPerturbation<int>>(),
                new IdentityLocalSearch()));
    }

    [Fact]
    public void CatalogContainsVariableNeighborhoodStableIds()
    {
        foreach (string id in new[]
                 {
                     "variable-neighborhood-descent",
                     "variable-neighborhood-search-mladenovic-hansen"
                 })
        {
            MetaheuristicCatalogEntry entry = MetaheuristicCatalog.GetRequired(id);
            Assert.Equal(id, entry.Id);
            Assert.True(entry.RequiresComposition);
        }
    }

    [Fact]
    public void PublicAlgorithmIdsExposeVariableNeighborhoodMethods()
    {
        Assert.Equal(
            "variable-neighborhood-descent",
            MetaheuristicAlgorithmIds.VariableNeighborhoodDescent);

        Assert.Equal(
            "variable-neighborhood-search-mladenovic-hansen",
            MetaheuristicAlgorithmIds.VariableNeighborhoodSearch);
    }

    [Fact]
    public void VnsDescriptorCarriesOriginalScientificReference()
    {
        var optimizer = new VariableNeighborhoodSearchOptimizer<int>(
            Initial(0),
            new ISolutionPerturbation<int>[]
            {
                Perturb(static (ref int _) => { })
            },
            new IdentityLocalSearch());

        Assert.Contains(
            optimizer.Descriptor.References,
            reference => reference.Doi == "10.1016/S0305-0548(97)00031-2");

        Assert.Contains(
            optimizer.Descriptor.References,
            reference => reference.Doi == "10.1016/S0377-2217(00)00100-4");
    }

    private static INeighborhoodSearchInitialSolutionGenerator<int> Initial(int value) =>
        new DelegateNeighborhoodSearchInitialSolutionGenerator<int>((_, _) => value);

    private delegate void RefIntAction(ref int value);

    private static ISolutionPerturbation<int> Perturb(RefIntAction action) =>
        new DelegateSolutionPerturbation<int>(
            (ref int solution, IOptimizationProblem<int> _, MetaheuristicsPlatform.Random.IRandomSource __) =>
                action(ref solution));

    private sealed class CountingPerturbation : ISolutionPerturbation<int>
    {
        private readonly RefIntAction _action;

        public CountingPerturbation(RefIntAction action) => _action = action;

        public int Calls { get; private set; }

        public void Perturb(
            ref int solution,
            IOptimizationProblem<int> problem,
            MetaheuristicsPlatform.Random.IRandomSource random)
        {
            Calls++;
            _action(ref solution);
        }
    }

    private sealed class ConditionalLocalSearch : ILocalSearchProcedure<int>
    {
        private readonly int _from;
        private readonly int _to;

        public ConditionalLocalSearch(int from, int to)
        {
            _from = from;
            _to = to;
        }

        public int Calls { get; private set; }

        public LocalSearchProcedureResult Improve(
            ref int solution,
            double currentFitness,
            OptimizationContext<int> context,
            ISolutionCloner<int> solutionCloner,
            CancellationToken cancellationToken)
        {
            Calls++;

            if (solution != _from)
            {
                return new LocalSearchProcedureResult(
                    currentFitness,
                    0,
                    localOptimum: true,
                    StoppingDecision.Continue("LocalOptimum"));
            }

            double candidateFitness = context.Evaluate(_to);
            if (!context.Problem.Sense.IsBetter(candidateFitness, currentFitness))
            {
                return new LocalSearchProcedureResult(
                    currentFitness,
                    0,
                    localOptimum: true,
                    StoppingDecision.Continue("LocalOptimum"));
            }

            solution = _to;

            return new LocalSearchProcedureResult(
                candidateFitness,
                1,
                localOptimum: true,
                StoppingDecision.Continue("LocalOptimum"));
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
