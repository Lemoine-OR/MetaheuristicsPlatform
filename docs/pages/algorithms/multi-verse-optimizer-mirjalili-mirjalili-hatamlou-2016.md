@page multi_verse_optimizer_mirjalili_mirjalili_hatamlou_2016 Multi-Verse Optimizer

# Multi-Verse Optimizer

## General description

Multi-Verse Optimizer (`MVO`) is the scientific identity introduced by Mirjalili, Mirjalili and Hatamlou in 2016. This page documents the canonical bounded-continuous platform implementation corresponding to that publication, without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016`
- Class: `MultiVerseOptimizer`
- Parameters: `MultiVerseOptimizerParameters`
- Family: Other / music-inspired methods
- Search space: bounded continuous vectors
- Public since: v0.86.0
- Primary DOI: `10.1007/s00521-015-1870-7`

## Complexity

O(ND + N log N) per iteration plus N objective evaluations. Memory usage is O(ND + N).

## Applicability

Non-negative-cost bounded continuous minimization using white-hole exchange and best-universe wormholes.

## Detailed operation

Canonical minimization-oriented MVO white-hole roulette, normalized inflation rates, elite best universe, linearly increasing wormhole existence probability and power-law travelling distance rate with p=6 by default.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed by the platform continuous search space before objective evaluation.

## Parameters

The public parameter object `MultiVerseOptimizerParameters` exposes only controls used by the canonical scientific mechanism. Validation rejects controls or objective domains that would silently alter the published equations.

## Stable factory ID

`multi-verse-optimizer-mirjalili-mirjalili-hatamlou-2016`

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<MultiVerseOptimizer>(
        MetaheuristicAlgorithmIds.MultiVerseOptimizer);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new MultiVerseOptimizerParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x),\quad f(x)\ge0
\f]

### Update equations / iterations

\f[
\begin{aligned}WEP_t&=WEP_{min}+\frac{t}{T}(WEP_{max}-WEP_{min}),\\TDR_t&=1-\left(\frac{t}{T}\right)^{1/p},\\x_{i,j}&\leftarrow x_{k,j}\;\text{with probability }NI(U_i),\quad x_{i,j}\leftarrow x_j^*\pm TDR_t((u_j-l_j)r+l_j)\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and finite non-negative inflation rates. The original minimization-oriented normalized-inflation/roulette semantics are preserved; maximization and negative objective values are rejected rather than silently transformed.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; WEP increases exploitation while TDR decreases the maximum wormhole displacement according to the published p-controlled schedule.

### Scientific references

Mirjalili, Mirjalili and Hatamlou (2016), Multi-Verse Optimizer: a nature-inspired algorithm for global optimization, Neural Computing and Applications 27, 495-513. DOI: `10.1007/s00521-015-1870-7`.
