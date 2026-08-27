@page grid_based_evolutionary_algorithm_yang_li_liu_zheng_2013 Grid-Based Evolutionary Algorithm

# Grid-Based Evolutionary Algorithm

## General description

Grid-Based Evolutionary Algorithm (`Grea`) is the public scientific identity associated with Yang, Li, Liu & Zheng (2013),
A Grid-Based Evolutionary Algorithm for Many-Objective Optimization. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `grid-based-evolutionary-algorithm-yang-li-liu-zheng-2013`
- Class: `GreaOptimizer`
- Parameters: `GreaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.109.0
- Primary DOI: `10.1109/TEVC.2012.2227145`

## Complexity

O(MN^2) Pareto sorting plus O(MN) grid assignment per generation. Memory usage is O(N(D+M)).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of Grid-Based Evolutionary Algorithm is appropriate.

## Detailed operation

GrEA uses normalized objective-space grids to couple convergence pressure with grid density and distribution.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`GreaParameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Grea;
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

GreaOptimizer algorithm =
    MetaheuristicFactory.Create<GreaOptimizer>(
        MetaheuristicAlgorithmIds.Grea);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new GreaParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`grid-based-evolutionary-algorithm-yang-li-liu-zheng-2013`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}g_{i,m}&=\left\lfloor G\,\frac{f_m(x_i)-f_m^{\min}}{f_m^{\max}-f_m^{\min}}\right\rfloor,\\F_i&=\operatorname{gridRank}(g_i)+\operatorname{gridDensity}(g_i)+\varepsilon\,\operatorname{conv}(x_i).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Yang, Li, Liu & Zheng (2013), A Grid-Based Evolutionary Algorithm for Many-Objective Optimization, IEEE Transactions on Evolutionary Computation 17(5), 721-736. DOI: `10.1109/TEVC.2012.2227145`.
