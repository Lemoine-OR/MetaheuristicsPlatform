@page pesa_ii_corne_jerram_knowles_oates_2001 PESA-II

# PESA-II

## General description

PESA-II (`PesaII`) is the public scientific identity associated with
Corne, Jerram, Knowles & Oates (2001), PESA-II: Region-Based Selection in Evolutionary Multiobjective Optimization, GECCO 2001, 283-290. This release documents and implements that identity without silently
mixing unrelated variants or reducing the objective vector to an undocumented scalar surrogate.

## Technical specifications

- Stable ID: `pesa-ii-corne-jerram-knowles-oates-2001`
- Class: `PesaIIOptimizer`
- Parameters: `PesaIIParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.98.0
- Primary DOI: `10.5555/2955239.2955289`

## Complexity

O(ANM) archive/grid selection plus objective evaluations. Memory usage is O((A+N)(D+M)).

## Applicability

Bounded continuous native multiobjective Pareto optimization.

## Detailed operation

Region-based selection from an external nondominated archive using adaptive hyperbox density. The implementation operates directly on objective vectors, uses the common bounded continuous search space, respects the declared optimization sense of every objective and propagates the caller cancellation token.

## Parameters

`PesaIIParameters` exposes the controls required by this scientific identity and validates the numerical and structural conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.PesaII;
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

PesaIIOptimizer algorithm =
    MetaheuristicFactory.Create<PesaIIOptimizer>(
        MetaheuristicAlgorithmIds.PesaII);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new PesaIIParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`pesa-ii-corne-jerram-knowles-oates-2001`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}A_{t+1}&=\operatorname{ND}(A_t\cup P_t),\\h^\star&=\arg\min_h n_h,\qquad p\sim\operatorname{Uniform}(A_{t+1}\cap h^\star).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, at least two finite objectives, and one explicit OptimizationSense per objective.

### Convergence conditions

No universal finite-time convergence guarantee is asserted; the implementation preserves the named multiobjective mechanism.

### Scientific references

Corne, Jerram, Knowles & Oates (2001), PESA-II: Region-Based Selection in Evolutionary Multiobjective Optimization, GECCO 2001, 283-290. DOI: `10.5555/2955239.2955289`.
