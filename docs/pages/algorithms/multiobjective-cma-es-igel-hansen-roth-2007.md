@page multiobjective_cma_es_igel_hansen_roth_2007 Multi-objective CMA-ES

# Multi-objective CMA-ES

## General description

Multi-objective CMA-ES (`MoCmaEs`) is the public scientific identity associated with Igel, Hansen & Roth (2007),
Covariance Matrix Adaptation for Multi-objective Optimization. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `multiobjective-cma-es-igel-hansen-roth-2007`
- Class: `MoCmaEsOptimizer`
- Parameters: `MoCmaEsParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.110.0
- Primary DOI: `10.1162/evco.2007.15.1.1`

## Complexity

O(ND^3+MN^2) covariance factorization and multiobjective selection per generation. Memory usage is O(ND^2+N(D+M)).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of Multi-objective CMA-ES is appropriate.

## Detailed operation

MO-CMA-ES maintains individual covariance/step-size strategy states and applies nondominated multiobjective selection.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`MoCmaEsParameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.MoCmaEs;
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

MoCmaEsOptimizer algorithm =
    MetaheuristicFactory.Create<MoCmaEsOptimizer>(
        MetaheuristicAlgorithmIds.MoCmaEs);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new MoCmaEsParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`multiobjective-cma-es-igel-hansen-roth-2007`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}x'&=x+\sigma A z,\qquad z\sim\mathcal N(0,I),\quad AA^\top=C,\\C'&=(1-c_C)C+c_C\,\frac{(x'-x)(x'-x)^\top}{\sigma^2},\qquad \sigma'=\sigma\exp\!\left(\frac{p_s-p_{\mathrm{target}}}{d_\sigma(1-p_{\mathrm{target}})}\right).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Igel, Hansen & Roth (2007), Covariance Matrix Adaptation for Multi-objective Optimization, Evolutionary Computation 15(1), 1-28. DOI: `10.1162/evco.2007.15.1.1`.
