using MetaheuristicsPlatform.Algorithms.Neighborhood;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Stopping;

namespace MetaheuristicsPlatform.Tests;

public sealed class AdvancedVariableNeighborhoodSearchTests
{
    [Fact]
    public void RvnsFindsImprovementWithoutLocalSearch()
    {
        var first = new CountingPerturbation(static (ref int _) => { });
        var second = new CountingPerturbation(static (ref int x) => x = 10);

        var optimizer = new ReducedVariableNeighborhoodSearchOptimizer<int>(
            Initial(0),
            new ISolutionPerturbation<int>[] { first, second });

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(10),
            new ReducedVariableNeighborhoodSearchParameters { MaximumCycles = 1 },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(10, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
        Assert.Equal("MaximumReducedVnsCycles", result.StopDecision.Criterion);
        Assert.Equal(2, first.Calls);
        Assert.Equal(2, second.Calls);
    }

    [Fact]
    public void GvnsUsesVariableNeighborhoodDescentAsImprovementPhase()
    {
        var optimizer = new GeneralVariableNeighborhoodSearchOptimizer<int>(
            Initial(0),
            new ISolutionPerturbation<int>[]
            {
                Perturb(static (ref int x) => x = 5)
            },
            new ILocalSearchProcedure<int>[]
            {
                new ConditionalLocalSearch(5, 8),
                new ConditionalLocalSearch(8, 10)
            });

        OptimizationResult<int> result = optimizer.Optimize(
            new QuadraticProblem(10),
            new GeneralVariableNeighborhoodSearchParameters
            {
                MaximumCycles = 1,
                MaximumNeighborhoodRestarts = 20
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(10, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
        Assert.Equal("MaximumGeneralVnsCycles", result.StopDecision.Criterion);
    }

    [Fact]
    public void SvnsAcceptsWorseDistantCandidateAndEscapesValley()
    {
        var optimizer = new SkewedVariableNeighborhoodSearchOptimizer<int>(
            Initial(0),
            new ISolutionPerturbation<int>[]
            {
                Perturb(
                    static (ref int x) =>
                    {
                        if (x < 2)
                        {
                            x++;
                        }
                    })
            },
            new IdentityLocalSearch(),
            new AbsoluteDistance());

        OptimizationResult<int> result = optimizer.Optimize(
            new ThreePointValleyProblem(),
            new SkewedVariableNeighborhoodSearchParameters
            {
                MaximumCycles = 1,
                Alpha = 2.0
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(2, result.BestSolution);
        Assert.Equal(-10.0, result.BestFitness, 12);
        Assert.Equal("MaximumSkewedVnsCycles", result.StopDecision.Criterion);
    }

    [Fact]
    public void SvnsWithZeroAlphaRejectsStrictlyWorseRecentering()
    {
        var optimizer = new SkewedVariableNeighborhoodSearchOptimizer<int>(
            Initial(0),
            new ISolutionPerturbation<int>[]
            {
                Perturb(static (ref int x) => x = 1)
            },
            new IdentityLocalSearch(),
            new AbsoluteDistance());

        OptimizationResult<int> result = optimizer.Optimize(
            new ThreePointValleyProblem(),
            new SkewedVariableNeighborhoodSearchParameters
            {
                MaximumCycles = 1,
                Alpha = 0.0
            },
            new ImmutableSolutionCloner<int>(),
            new MaxEvaluationsStoppingCriterion(100),
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(0, result.BestSolution);
        Assert.Equal(0.0, result.BestFitness, 12);
    }

    [Fact]
    public void SvnsRejectsInvalidDomainDistance()
    {
        var optimizer = new SkewedVariableNeighborhoodSearchOptimizer<int>(
            Initial(0),
            new ISolutionPerturbation<int>[]
            {
                Perturb(static (ref int x) => x = 1)
            },
            new IdentityLocalSearch(),
            new InvalidDistance());

        Assert.Throws<InvalidOperationException>(() =>
            optimizer.Optimize(
                new ThreePointValleyProblem(),
                new SkewedVariableNeighborhoodSearchParameters
                {
                    MaximumCycles = 1,
                    Alpha = 1.0
                },
                new ImmutableSolutionCloner<int>(),
                new MaxEvaluationsStoppingCriterion(100),
                cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public void AdvancedVnsParametersRejectInvalidValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ReducedVariableNeighborhoodSearchParameters
            {
                MaximumCycles = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeneralVariableNeighborhoodSearchParameters
            {
                MaximumCycles = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GeneralVariableNeighborhoodSearchParameters
            {
                MaximumNeighborhoodRestarts = 0
            }.Validate());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SkewedVariableNeighborhoodSearchParameters
            {
                Alpha = -0.1
            }.Validate());
    }

    [Fact]
    public void AdvancedVnsRequiresItsCompositionComponents()
    {
        Assert.Throws<ArgumentException>(() =>
            new ReducedVariableNeighborhoodSearchOptimizer<int>(
                Initial(0),
                Array.Empty<ISolutionPerturbation<int>>()));

        Assert.Throws<ArgumentException>(() =>
            new GeneralVariableNeighborhoodSearchOptimizer<int>(
                Initial(0),
                new ISolutionPerturbation<int>[]
                {
                    Perturb(static (ref int _) => { })
                },
                Array.Empty<ILocalSearchProcedure<int>>()));

        Assert.Throws<ArgumentNullException>(() =>
            new SkewedVariableNeighborhoodSearchOptimizer<int>(
                Initial(0),
                new ISolutionPerturbation<int>[]
                {
                    Perturb(static (ref int _) => { })
                },
                new IdentityLocalSearch(),
                null!));
    }

    [Fact]
    public void CatalogContainsAdvancedVariableNeighborhoodStableIds()
    {
        foreach (string id in new[]
                 {
                     "reduced-variable-neighborhood-search",
                     "general-variable-neighborhood-search",
                     "skewed-variable-neighborhood-search-hansen-mladenovic-2001"
                 })
        {
            MetaheuristicCatalogEntry entry = MetaheuristicCatalog.GetRequired(id);
            Assert.Equal(id, entry.Id);
            Assert.True(entry.RequiresComposition);
        }
    }

    [Fact]
    public void PublicAlgorithmIdsExposeAdvancedVariableNeighborhoodMethods()
    {
        Assert.Equal(
            "reduced-variable-neighborhood-search",
            MetaheuristicAlgorithmIds.ReducedVariableNeighborhoodSearch);

        Assert.Equal(
            "general-variable-neighborhood-search",
            MetaheuristicAlgorithmIds.GeneralVariableNeighborhoodSearch);

        Assert.Equal(
            "skewed-variable-neighborhood-search-hansen-mladenovic-2001",
            MetaheuristicAlgorithmIds.SkewedVariableNeighborhoodSearch);
    }

    [Fact]
    public void AdvancedVnsDescriptorsCarryCanonicalReferences()
    {
        var rvns = new ReducedVariableNeighborhoodSearchOptimizer<int>(
            Initial(0),
            new ISolutionPerturbation<int>[]
            {
                Perturb(static (ref int _) => { })
            });

        var svns = new SkewedVariableNeighborhoodSearchOptimizer<int>(
            Initial(0),
            new ISolutionPerturbation<int>[]
            {
                Perturb(static (ref int _) => { })
            },
            new IdentityLocalSearch(),
            new AbsoluteDistance());

        Assert.Contains(
            rvns.Descriptor.References,
            reference => reference.Doi == "10.1007/s13675-016-0075-x");

        Assert.Contains(
            svns.Descriptor.References,
            reference => reference.Doi == "10.1016/S0377-2217(00)00100-4");
    }

    private static INeighborhoodSearchInitialSolutionGenerator<int> Initial(int value) =>
        new DelegateNeighborhoodSearchInitialSolutionGenerator<int>((_, _) => value);

    private delegate void RefIntAction(ref int value);

    private static ISolutionPerturbation<int> Perturb(RefIntAction action) =>
        new DelegateSolutionPerturbation<int>(
            (ref int solution,
             IOptimizationProblem<int> _,
             MetaheuristicsPlatform.Random.IRandomSource __) =>
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

        public LocalSearchProcedureResult Improve(
            ref int solution,
            double currentFitness,
            OptimizationContext<int> context,
            ISolutionCloner<int> solutionCloner,
            CancellationToken cancellationToken)
        {
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

    private sealed class AbsoluteDistance : ISolutionDistance<int>
    {
        public double Distance(in int first, in int second) =>
            Math.Abs(first - second);
    }

    private sealed class InvalidDistance : ISolutionDistance<int>
    {
        public double Distance(in int first, in int second) => -1.0;
    }

    private sealed class QuadraticProblem : IOptimizationProblem<int>
    {
        private readonly int _target;

        public QuadraticProblem(int target) => _target = target;

        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            (solution - _target) * (double)(solution - _target);
    }

    private sealed class ThreePointValleyProblem : IOptimizationProblem<int>
    {
        public OptimizationSense Sense => OptimizationSense.Minimize;

        public double Evaluate(int solution) =>
            solution switch
            {
                0 => 0.0,
                1 => 1.0,
                2 => -10.0,
                _ => 1000.0
            };
    }
}
