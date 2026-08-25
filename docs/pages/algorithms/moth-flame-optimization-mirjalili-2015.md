@page moth_flame_optimization_mirjalili_2015 Moth-Flame Optimization

# Moth-Flame Optimization

## General description

Moth-Flame Optimization (`MFO`) is the scientific identity introduced by Mirjalili in 2015.
This page documents the canonical bounded-continuous platform implementation corresponding to that
publication, without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `moth-flame-optimization-mirjalili-2015`
- Class: `MothFlameOptimizer`
- Parameters: `MothFlameOptimizerParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.73.0
- Primary DOI: `10.1016/j.knosys.2015.07.006`

## Complexity

O(ND + N log N) per generation plus N objective evaluations. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization with logarithmic moth-to-flame spirals and a linearly decreasing flame count.

## Detailed operation

Canonical moth/flame dual population, historical flame elitism, logarithmic spiral update, and linearly decreasing flame count from Mirjalili (2015).

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed
by the platform continuous search space before objective evaluation. The implementation preserves the
published stochastic mechanism while using the platform's explicit completed-iteration accounting.

## Parameters

The public parameter object `MothFlameOptimizerParameters` exposes only controls used by the canonical scientific mechanism.
Validation rejects population sizes or numerical controls that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<MothFlameOptimizer>(
        MetaheuristicAlgorithmIds.MothFlameOptimization);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new MothFlameOptimizerParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`moth-flame-optimization-mirjalili-2015`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}D_{ij}^{t}&=\left|F_j^t-M_i^t\right|,\\M_i^{t+1}&=D_{ij}^{t}e^{b\tau}\cos(2\pi\tau)+F_j^t,\\N_f(t)&=\operatorname{round}\!\left(N-t\frac{N-1}{T}\right),\\a_t&=-1-\frac{t}{T},\quad \tau\sim\mathcal U[a_t,1]\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and finite objective values; at least two moths; flames are the best historical positions under the configured objective sense.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; the decreasing number of flames and shrinking lower bound of the spiral parameter progressively emphasize exploitation.

### Scientific references

Mirjalili (2015), Moth-flame optimization algorithm: A novel nature-inspired heuristic paradigm, Knowledge-Based Systems 89, 228-249.
DOI: `10.1016/j.knosys.2015.07.006`.
