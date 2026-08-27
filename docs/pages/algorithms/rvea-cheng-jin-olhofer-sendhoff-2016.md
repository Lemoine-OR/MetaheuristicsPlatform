@page rvea_cheng_jin_olhofer_sendhoff_2016 RVEA

# RVEA

## General description

RVEA (`Rvea`) is the public scientific identity associated with
Cheng, Jin, Olhofer & Sendhoff (2016), A Reference Vector Guided Evolutionary Algorithm for Many-Objective Optimization, IEEE Transactions on Evolutionary Computation 20(5), 773-791. This release documents and implements that identity without silently
mixing unrelated variants or reducing the objective vector to an undocumented scalar surrogate.

## Technical specifications

- Stable ID: `rvea-cheng-jin-olhofer-sendhoff-2016`
- Class: `RveaOptimizer`
- Parameters: `RveaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.105.0
- Primary DOI: `10.1109/TEVC.2016.2519378`

## Complexity

O(NRM) reference-vector association and angle-penalized selection per generation. Memory usage is O(N(D+M)+RM).

## Applicability

Bounded continuous native multiobjective Pareto optimization.

## Detailed operation

Reference-vector guided many-objective selection using angle-penalized distance normalized by nearest reference-vector angle and periodic vector adaptation. The implementation operates directly on objective vectors, uses the common bounded continuous search space, respects the declared optimization sense of every objective and propagates the caller cancellation token.

## Parameters

`RveaParameters` exposes the controls required by this scientific identity and validates the numerical and structural conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Rvea;
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

RveaOptimizer algorithm =
    MetaheuristicFactory.Create<RveaOptimizer>(
        MetaheuristicAlgorithmIds.Rvea);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new RveaParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`rvea-cheng-jin-olhofer-sendhoff-2016`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}\theta(x,\lambda)&=\arccos\!\frac{f(x)^\top\lambda}{\|f(x)\|\,\|\lambda\|},\\APD(x,\lambda,t)&=\|f(x)\|\left(1+M(t/T)^\alpha\theta(x,\lambda)/\gamma_\lambda\right).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, at least two finite objectives, and one explicit OptimizationSense per objective.

### Convergence conditions

No universal finite-time convergence guarantee is asserted; the implementation preserves the named multiobjective mechanism.

### Scientific references

Cheng, Jin, Olhofer & Sendhoff (2016), A Reference Vector Guided Evolutionary Algorithm for Many-Objective Optimization, IEEE Transactions on Evolutionary Computation 20(5), 773-791. DOI: `10.1109/TEVC.2016.2519378`.
