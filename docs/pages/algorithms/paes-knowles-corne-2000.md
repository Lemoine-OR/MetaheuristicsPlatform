@page paes_knowles_corne_2000 Pareto Archived Evolution Strategy

# Pareto Archived Evolution Strategy

## General description

Pareto Archived Evolution Strategy (`Paes`) is the public scientific identity associated with
Knowles & Corne (2000), Approximating the Nondominated Front Using the Pareto Archived Evolution Strategy, Evolutionary Computation 8(2), 149-172. This release documents and implements that identity without silently
mixing unrelated variants or reducing the objective vector to an undocumented scalar surrogate.

## Technical specifications

- Stable ID: `paes-knowles-corne-2000`
- Class: `PaesOptimizer`
- Parameters: `PaesParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.97.0
- Primary DOI: `10.1162/106365600568167`

## Complexity

O(AM) archive dominance per mutation step. Memory usage is O(A(D+M)).

## Applicability

Bounded continuous native multiobjective Pareto optimization.

## Detailed operation

Canonical (1+1)-PAES local mutation with Pareto archive and adaptive objective-space grid. The implementation operates directly on objective vectors, uses the common bounded continuous search space, respects the declared optimization sense of every objective and propagates the caller cancellation token.

## Parameters

`PaesParameters` exposes the controls required by this scientific identity and validates the numerical and structural conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Paes;
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

PaesOptimizer algorithm =
    MetaheuristicFactory.Create<PaesOptimizer>(
        MetaheuristicAlgorithmIds.Paes);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new PaesParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`paes-knowles-corne-2000`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}y&=\operatorname{Mutate}(x),\\x_{t+1}&=\operatorname{ParetoGridSelect}(x_t,y,A_t),\qquad A_{t+1}=\operatorname{Archive}(A_t\cup\{y\}).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, at least two finite objectives, and one explicit OptimizationSense per objective.

### Convergence conditions

No universal finite-time convergence guarantee is asserted; the implementation preserves the named multiobjective mechanism.

### Scientific references

Knowles & Corne (2000), Approximating the Nondominated Front Using the Pareto Archived Evolution Strategy, Evolutionary Computation 8(2), 149-172. DOI: `10.1162/106365600568167`.
