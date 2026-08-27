@page theta_dea_yuan_xu_wang_yao_2016 Theta-Dominance Evolutionary Algorithm

# Theta-Dominance Evolutionary Algorithm

## General description

Theta-Dominance Evolutionary Algorithm (`ThetaDea`) is the public scientific identity associated with Yuan, Xu, Wang & Yao (2016),
A New Dominance Relation-Based Evolutionary Algorithm for Many-Objective Optimization. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `theta-dea-yuan-xu-wang-yao-2016`
- Class: `ThetaDeaOptimizer`
- Parameters: `ThetaDeaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.115.0
- Primary DOI: `10.1109/TEVC.2015.2420112`

## Complexity

O(NRM+MN^2) reference clustering and selection per generation. Memory usage is O(N(D+M)+RM).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of Theta-Dominance Evolutionary Algorithm is appropriate.

## Detailed operation

Theta-DEA clusters normalized objective vectors by reference directions and ranks solutions with theta-dominance/PBI pressure.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`ThetaDeaParameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.ThetaDea;
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

ThetaDeaOptimizer algorithm =
    MetaheuristicFactory.Create<ThetaDeaOptimizer>(
        MetaheuristicAlgorithmIds.ThetaDea);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new ThetaDeaParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`theta-dea-yuan-xu-wang-yao-2016`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}r(x)&=\arg\min_k d_\perp(\bar f(x),\lambda^k),\\\theta\operatorname{-fitness}(x)&=d_1(x,\lambda^{r(x)})+\theta d_2(x,\lambda^{r(x)}).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Yuan, Xu, Wang & Yao (2016), A New Dominance Relation-Based Evolutionary Algorithm for Many-Objective Optimization, IEEE Transactions on Evolutionary Computation 20(1), 16-37. DOI: `10.1109/TEVC.2015.2420112`.
