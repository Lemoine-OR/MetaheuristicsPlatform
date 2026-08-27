@page strength_pareto_evolutionary_algorithm_zitzler_thiele_1999 Strength Pareto Evolutionary Algorithm

# Strength Pareto Evolutionary Algorithm

## General description

Strength Pareto Evolutionary Algorithm (`Spea`) is the public scientific identity associated with Zitzler & Thiele (1999),
Multiobjective Evolutionary Algorithms: A Comparative Case Study and the Strength Pareto Approach. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `strength-pareto-evolutionary-algorithm-zitzler-thiele-1999`
- Class: `SpeaOptimizer`
- Parameters: `SpeaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.106.0
- Primary DOI: `10.1109/4235.797969`

## Complexity

O(N^2M) dominance/strength work plus objective evaluations per generation. Memory usage is O((N+A)(D+M)).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of Strength Pareto Evolutionary Algorithm is appropriate.

## Detailed operation

Original SPEA with an external nondominated set, strength fitness assignment and archive clustering/truncation.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`SpeaParameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Spea;
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

SpeaOptimizer algorithm =
    MetaheuristicFactory.Create<SpeaOptimizer>(
        MetaheuristicAlgorithmIds.Spea);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new SpeaParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`strength-pareto-evolutionary-algorithm-zitzler-thiele-1999`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}S(i)&=\frac{|\{j:i\prec j\}|}{N},\\F(j)&=1+\sum_{i\in A,\ i\prec j}S(i),\qquad A^+=\operatorname{ND}(P\cup A).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Zitzler & Thiele (1999), Multiobjective Evolutionary Algorithms: A Comparative Case Study and the Strength Pareto Approach, IEEE Transactions on Evolutionary Computation 3(4), 257-271. DOI: `10.1109/4235.797969`.
