@page hype_bader_zitzler_2011 HypE

# HypE

## General description

HypE (`Hype`) is the public scientific identity associated with Bader & Zitzler (2011),
HypE: An Algorithm for Fast Hypervolume-Based Many-Objective Optimization. The release keeps this identity separate from adjacent multiobjective variants.

## Technical specifications

- Stable ID: `hype-bader-zitzler-2011`
- Class: `HypeOptimizer`
- Parameters: `HypeParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.112.0
- Primary DOI: `10.1162/EVCO_A_00009`

## Complexity

O(SNM+MN^2) hypervolume sampling and nondominated sorting per generation. Memory usage is O(N(D+M)+S).

## Applicability

Bounded continuous multiobjective or many-objective optimization where the scientific selection/update mechanism of HypE is appropriate.

## Detailed operation

HypE drives mating and environmental selection with Monte-Carlo estimates of hypervolume contribution.

The implementation operates directly on objective vectors, respects each declared optimization sense and propagates the caller cancellation token.

## Parameters

`HypeParameters` exposes the controls required by this scientific identity and validates the conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Hype;
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

HypeOptimizer algorithm =
    MetaheuristicFactory.Create<HypeOptimizer>(
        MetaheuristicAlgorithmIds.Hype);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new HypeParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`hype-bader-zitzler-2011`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}\widehat I_H(x)&=\frac1S\sum_{s=1}^{S}\frac{\mathbf 1[x\preceq u_s]}{|\{y:y\preceq u_s\}|},\\P_{t+1}&=\operatorname{EnvironmentalSelect}_{\widehat I_H}(P_t\cup Q_t,N).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, at least two objectives and one explicit optimization sense per objective.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic multiobjective mechanism and its selection pressure.

### Scientific references

Bader & Zitzler (2011), HypE: An Algorithm for Fast Hypervolume-Based Many-Objective Optimization, Evolutionary Computation 19(1), 45-76. DOI: `10.1162/EVCO_A_00009`.
