@page black_hole_algorithm_hatamlou_2013 Black Hole Algorithm

# Black Hole Algorithm

## General description

Black Hole Algorithm (`BH`) is the scientific identity introduced by Hatamlou in 2013. This page documents the canonical bounded-continuous platform implementation corresponding to that publication, without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `black-hole-algorithm-hatamlou-2013`
- Class: `BlackHoleOptimizer`
- Parameters: `BlackHoleParameters`
- Family: Other / music-inspired methods
- Search space: bounded continuous vectors
- Public since: v0.84.0
- Primary DOI: `10.1016/j.ins.2012.08.023`

## Complexity

O(ND) per iteration plus N-1 attraction evaluations and event-horizon replacement evaluations. Memory usage is O(ND).

## Applicability

Positive-cost bounded continuous minimization using attraction to the current black hole and event-horizon replacement.

## Detailed operation

Canonical Black Hole Algorithm: every star moves toward the current best using one uniform scalar, a better star becomes the black hole, and stars inside the published event horizon are reinitialized.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed by the platform continuous search space before objective evaluation.

## Parameters

The public parameter object `BlackHoleParameters` exposes only controls used by the canonical scientific mechanism. Validation rejects controls or objective domains that would silently alter the published equations.

## Stable factory ID

`black-hole-algorithm-hatamlou-2013`

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<BlackHoleOptimizer>(
        MetaheuristicAlgorithmIds.BlackHoleAlgorithm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new BlackHoleParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x),\quad f(x)>0
\f]

### Update equations / iterations

\f[
\begin{aligned}x_i^{t+1}&=x_i^t+r_i(x_{BH}^t-x_i^t),\quad r_i\sim\mathcal U(0,1),\\R_t&=\left|\frac{f(x_{BH}^t)}{\sum_{i=1}^{N}f(x_i^t)}\right|,\\\|x_i-x_{BH}\|_2<R_t&\Rightarrow x_i\leftarrow\operatorname{Uniform}(\mathcal X)\end{aligned}
\f]

### Assumptions

Canonical event-horizon semantics require finite strictly positive objective values and minimization. Maximization or non-positive costs are rejected rather than silently transformed.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; random event-horizon replacement maintains exploration while attraction concentrates stars around the current black hole.

### Scientific references

Hatamlou (2013), Black hole: A new heuristic optimization approach for data clustering, Information Sciences 222, 175-184. DOI: `10.1016/j.ins.2012.08.023`.
