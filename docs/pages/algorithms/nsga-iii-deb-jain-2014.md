@page nsga_iii_deb_jain_2014 NSGA-III

# NSGA-III

## General description

NSGA-III (`NsgaIII`) is the public scientific identity associated with
Deb & Jain (2014), An Evolutionary Many-Objective Optimization Algorithm Using Reference-Point-Based Nondominated Sorting Approach, Part I, IEEE Transactions on Evolutionary Computation 18(4), 577-601. This release documents and implements that identity without silently
mixing unrelated variants or reducing the objective vector to an undocumented scalar surrogate.

## Technical specifications

- Stable ID: `nsga-iii-deb-jain-2014`
- Class: `NsgaIIIOptimizer`
- Parameters: `NsgaIIIParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.103.0
- Primary DOI: `10.1109/TEVC.2013.2281535`

## Complexity

O(MN^2+NRM) sorting and reference-direction association per generation. Memory usage is O(N(D+M)+RM).

## Applicability

Bounded continuous native multiobjective Pareto optimization.

## Detailed operation

NSGA-II framework with normalized objective vectors, Das-Dennis reference directions and reference niching. The implementation operates directly on objective vectors, uses the common bounded continuous search space, respects the declared optimization sense of every objective and propagates the caller cancellation token.

## Parameters

`NsgaIIIParameters` exposes the controls required by this scientific identity and validates the numerical and structural conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.NsgaIII;
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

NsgaIIIOptimizer algorithm =
    MetaheuristicFactory.Create<NsgaIIIOptimizer>(
        MetaheuristicAlgorithmIds.NsgaIII);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new NsgaIIIParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`nsga-iii-deb-jain-2014`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}z_i&=\frac{f(x_i)-z^\star}{a-z^\star},\\\pi(i)&=\arg\min_r d_\perp(z_i,\lambda^r),\qquad P_{t+1}=\operatorname{ReferenceNiching}(F_\ell,\pi,\rho,N).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, at least two finite objectives, and one explicit OptimizationSense per objective.

### Convergence conditions

No universal finite-time convergence guarantee is asserted; the implementation preserves the named multiobjective mechanism.

### Scientific references

Deb & Jain (2014), An Evolutionary Many-Objective Optimization Algorithm Using Reference-Point-Based Nondominated Sorting Approach, Part I, IEEE Transactions on Evolutionary Computation 18(4), 577-601. DOI: `10.1109/TEVC.2013.2281535`.
