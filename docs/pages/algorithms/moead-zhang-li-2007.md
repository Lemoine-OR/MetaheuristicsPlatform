@page moead_zhang_li_2007 MOEA/D

# MOEA/D

## General description

MOEA/D (`Moead`) is the public scientific identity associated with
Zhang & Li (2007), MOEA/D: A Multiobjective Evolutionary Algorithm Based on Decomposition, IEEE Transactions on Evolutionary Computation 11(6), 712-731. This release documents and implements that identity without silently
mixing unrelated variants or reducing the objective vector to an undocumented scalar surrogate.

## Technical specifications

- Stable ID: `moead-zhang-li-2007`
- Class: `MoeadOptimizer`
- Parameters: `MoeadParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.100.0
- Primary DOI: `10.1109/TEVC.2007.892759`

## Complexity

O(NTM) neighborhood scalarization updates plus objective evaluations. Memory usage is O(N(D+M+T)).

## Applicability

Bounded continuous native multiobjective Pareto optimization.

## Detailed operation

Tchebycheff decomposition into neighboring scalar subproblems with differential reproduction and ideal-point updates. The implementation operates directly on objective vectors, uses the common bounded continuous search space, respects the declared optimization sense of every objective and propagates the caller cancellation token.

## Parameters

`MoeadParameters` exposes the controls required by this scientific identity and validates the numerical and structural conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Moead;
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

MoeadOptimizer algorithm =
    MetaheuristicFactory.Create<MoeadOptimizer>(
        MetaheuristicAlgorithmIds.Moead);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new MoeadParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`moead-zhang-li-2007`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}g^{te}(x\mid\lambda^i,z^\star)&=\max_j\lambda_j^i|f_j(x)-z_j^\star|,\\x^j&\leftarrow y\quad\text{if }g^{te}(y\mid\lambda^j,z^\star)\le g^{te}(x^j\mid\lambda^j,z^\star),\quad j\in B(i).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, at least two finite objectives, and one explicit OptimizationSense per objective.

### Convergence conditions

No universal finite-time convergence guarantee is asserted; the implementation preserves the named multiobjective mechanism.

### Scientific references

Zhang & Li (2007), MOEA/D: A Multiobjective Evolutionary Algorithm Based on Decomposition, IEEE Transactions on Evolutionary Computation 11(6), 712-731. DOI: `10.1109/TEVC.2007.892759`.
