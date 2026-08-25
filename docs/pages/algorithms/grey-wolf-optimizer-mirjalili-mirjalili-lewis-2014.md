@page grey_wolf_optimizer_mirjalili_mirjalili_lewis_2014 Grey Wolf Optimizer

# Grey Wolf Optimizer

## General description

Grey Wolf Optimizer (`GWO`) is the scientific identity introduced by
Mirjalili, Mirjalili & Lewis in 2014. This page documents the canonical bounded-continuous
platform implementation corresponding to that publication, without silently mixing later
variants or hybridizations.

## Technical specifications

- Stable ID: `grey-wolf-optimizer-mirjalili-mirjalili-lewis-2014`
- Class: `GreyWolfOptimizer`
- Parameters: `GreyWolfOptimizerParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.72.0
- Primary DOI: `10.1016/j.advengsoft.2013.12.007`

## Complexity

O(ND + N log N) per generation plus N objective evaluations. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization using alpha/beta/delta leadership and encircling dynamics.

## Detailed operation

Canonical alpha-beta-delta leadership, three independent A/C encircling vectors, arithmetic mean position update and linear a decrease from 2 to 0.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is
performed by the platform continuous search space. Completed-iteration accounting is kept
separate from partial iterations stopped by an evaluation or external stopping criterion.

## Parameters

The public parameter object `GreyWolfOptimizerParameters` exposes only controls used by the
canonical scientific mechanism. Validation rejects non-finite probabilities/scales and
population sizes that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<GreyWolfOptimizer>(
        MetaheuristicAlgorithmIds.GreyWolfOptimizer);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new GreyWolfOptimizerParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`grey-wolf-optimizer-mirjalili-mirjalili-lewis-2014`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}
D_{\alpha}&=\left|C_1X_{\alpha}-X\right|,\\
X_1&=X_{\alpha}-A_1D_{\alpha},\\
X^{t+1}&=\frac{X_1+X_2+X_3}{3},\\
a_t&=2\left(1-\frac{t}{T}\right)\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and finite objective values; at least three wolves; alpha, beta and delta are the three best current solutions under the configured objective sense.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; the linearly decreasing coefficient a moves the canonical model from exploration toward exploitation.

### Scientific references

Mirjalili, Mirjalili & Lewis (2014), Grey Wolf Optimizer, Advances in Engineering Software 69, 46-61.
DOI: `10.1016/j.advengsoft.2013.12.007`.
