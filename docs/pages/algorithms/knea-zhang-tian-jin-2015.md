@page knea_zhang_tian_jin_2015 Knee Point Driven Evolutionary Algorithm

# Knee Point Driven Evolutionary Algorithm

## General description

Knee Point Driven Evolutionary Algorithm (`Knea`) is the public scientific identity associated with Zhang, Tian & Jin (2015),
A Knee Point Driven Evolutionary Algorithm for Many-Objective Optimization. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `knea-zhang-tian-jin-2015`
- Class: `KneaOptimizer`
- Parameters: `KneaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.116.0
- Primary DOI: `10.1109/TEVC.2014.2378512`

## Complexity

O(MN^2+N^2M) nondominated sorting and knee-neighborhood analysis per generation. Memory usage is O(N(D+M)+N^2).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of Knee Point Driven Evolutionary Algorithm is appropriate.

## Detailed operation

KnEA detects locally preferred knee candidates and combines knee pressure with nondominated environmental selection.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`KneaParameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Knea;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem =
    new ContinuousMultiobjectiveOptimizationProblem(
        BoundedContinuousSearchSpace.Uniform(4, 0.0, 1.0),
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

KneaOptimizer algorithm =
    MetaheuristicFactory.Create<KneaOptimizer>(
        MetaheuristicAlgorithmIds.Knea);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new KneaParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`knea-zhang-tian-jin-2015`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}\mathcal N_k(x)&=\operatorname{kNN}(x,F),\\K(x)&=\mathbf 1\!\left[\operatorname{conv}(x)\le\operatorname{conv}(y)\ \forall y\in\mathcal N_k(x)\right],\qquad F_K(x)=\operatorname{rank}(x)+(1-\eta)\operatorname{conv}(x)-\eta K(x).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Zhang, Tian & Jin (2015), A Knee Point Driven Evolutionary Algorithm for Many-Objective Optimization, IEEE Transactions on Evolutionary Computation 19(6), 761-776. DOI: `10.1109/TEVC.2014.2378512`.
