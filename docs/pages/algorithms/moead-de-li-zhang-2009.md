@page moead_de_li_zhang_2009 MOEA/D-DE

# MOEA/D-DE

## General description

MOEA/D-DE (`MoeadDe`) is the public scientific identity associated with Li & Zhang (2009),
Multiobjective Optimization Problems With Complicated Pareto Sets, MOEA/D and NSGA-II. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `moead-de-li-zhang-2009`
- Class: `MoeadDeOptimizer`
- Parameters: `MoeadDeParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.111.0
- Primary DOI: `10.1109/TEVC.2008.925798`

## Complexity

O(NTM+NTD) neighborhood scalarization and DE variation per generation. Memory usage is O(N(D+M+T)).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of MOEA/D-DE is appropriate.

## Detailed operation

MOEA/D-DE combines Tchebycheff decomposition, neighborhood mating/update and differential-evolution reproduction.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`MoeadDeParameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.MoeadDe;
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

MoeadDeOptimizer algorithm =
    MetaheuristicFactory.Create<MoeadDeOptimizer>(
        MetaheuristicAlgorithmIds.MoeadDe);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new MoeadDeParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`moead-de-li-zhang-2009`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}y&=x_i+F(x_{r_1}-x_{r_2}),\\g^{te}(y\mid\lambda^j,z^\star)&=\max_m\lambda_m^j|f_m(y)-z_m^\star|,\qquad x_j\leftarrow y\ \text{if }g^{te}(y)\le g^{te}(x_j).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Li & Zhang (2009), Multiobjective Optimization Problems With Complicated Pareto Sets, MOEA/D and NSGA-II, IEEE Transactions on Evolutionary Computation 13(2), 284-302. DOI: `10.1109/TEVC.2008.925798`.
