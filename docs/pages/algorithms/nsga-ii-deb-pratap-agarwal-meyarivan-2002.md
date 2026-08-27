@page nsga_ii_deb_pratap_agarwal_meyarivan_2002 NSGA-II

# NSGA-II

## General description

NSGA-II (`NsgaII`) is the public scientific identity associated with
Deb, Pratap, Agarwal & Meyarivan (2002), A Fast and Elitist Multiobjective Genetic Algorithm: NSGA-II, IEEE Transactions on Evolutionary Computation 6(2), 182-197. This release documents and implements that identity without silently
mixing unrelated variants or reducing the objective vector to an undocumented scalar surrogate.

## Technical specifications

- Stable ID: `nsga-ii-deb-pratap-agarwal-meyarivan-2002`
- Class: `NsgaIIOptimizer`
- Parameters: `NsgaIIParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.96.0
- Primary DOI: `10.1109/4235.996017`

## Complexity

O(MN^2) nondominated sorting plus objective evaluations per generation. Memory usage is O(MN+ND).

## Applicability

Bounded continuous native multiobjective Pareto optimization.

## Detailed operation

Fast nondominated sorting, elitist parent-offspring survival and crowding-distance diversity. The implementation operates directly on objective vectors, uses the common bounded continuous search space, respects the declared optimization sense of every objective and propagates the caller cancellation token.

## Parameters

`NsgaIIParameters` exposes the controls required by this scientific identity and validates the numerical and structural conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.NsgaII;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem =
    new ContinuousMultiobjectiveOptimizationProblem(
        BoundedContinuousSearchSpace.Uniform(
            4,
            0.0,
            1.0),
        new[]
        {
            OptimizationSense.Minimize,
            OptimizationSense.Minimize
        },
        static (
            ReadOnlySpan<double> x,
            Span<double> f) =>
        {
            f[0] = x[0];
            f[1] =
                1.0 -
                Math.Sqrt(x[0]) +
                x[1] +
                x[2] +
                x[3];
        });

NsgaIIOptimizer algorithm =
    MetaheuristicFactory.Create<NsgaIIOptimizer>(
        MetaheuristicAlgorithmIds.NsgaII);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new NsgaIIParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`nsga-ii-deb-pratap-agarwal-meyarivan-2002`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}R_t&=\operatorname{FastNonDominatedSort}(P_t\cup Q_t),\\P_{t+1}&=\operatorname{CrowdingSelect}(R_t,N).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, at least two finite objectives, and one explicit OptimizationSense per objective.

### Convergence conditions

No universal finite-time convergence guarantee is asserted; the implementation preserves the named multiobjective mechanism.

### Scientific references

Deb, Pratap, Agarwal & Meyarivan (2002), A Fast and Elitist Multiobjective Genetic Algorithm: NSGA-II, IEEE Transactions on Evolutionary Computation 6(2), 182-197. DOI: `10.1109/4235.996017`.
