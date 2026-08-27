@page spea2_zitzler_laumanns_thiele_2001 SPEA2

# SPEA2

## General description

SPEA2 (`Spea2`) is the public scientific identity associated with Zitzler, Laumanns & Thiele (2001),
SPEA2: Improving the Strength Pareto Evolutionary Algorithm. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `spea2-zitzler-laumanns-thiele-2001`
- Class: `Spea2Optimizer`
- Parameters: `Spea2Parameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.107.0
- Primary DOI: `10.3929/ethz-a-004284029`

## Complexity

O(N^2M+N^2 log N) strength and density work per generation. Memory usage is O((N+A)(D+M)+N^2).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of SPEA2 is appropriate.

## Detailed operation

SPEA2 with fine-grained raw strength fitness, kth-neighbor density estimation and nearest-neighbor archive truncation.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`Spea2Parameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Spea2;
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

Spea2Optimizer algorithm =
    MetaheuristicFactory.Create<Spea2Optimizer>(
        MetaheuristicAlgorithmIds.Spea2);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new Spea2Parameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`spea2-zitzler-laumanns-thiele-2001`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}S(i)&=|\{j:i\prec j\}|,\\R(i)&=\sum_{j\prec i}S(j),\qquad D(i)=\frac{1}{\sigma_i^{(k)}+2},\qquad F(i)=R(i)+D(i).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Zitzler, Laumanns & Thiele (2001), SPEA2: Improving the Strength Pareto Evolutionary Algorithm, TIK Report 103, ETH Zurich. DOI: `10.3929/ethz-a-004284029`.
