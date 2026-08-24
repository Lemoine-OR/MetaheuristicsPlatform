@page biogeography_based_optimization_simon_2008 Biogeography-Based Optimization

# Biogeography-Based Optimization

## General description

Biogeography-Based Optimization (`BBO`) is the scientific identity introduced by
Simon in 2008. This page documents the canonical bounded-continuous
platform implementation corresponding to that publication, without silently mixing later
variants or hybridizations.

## Technical specifications

- Stable ID: `biogeography-based-optimization-simon-2008`
- Class: `BiogeographyBasedOptimizationOptimizer`
- Parameters: `BiogeographyBasedOptimizationParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Public since: v0.68.0
- Primary DOI: `10.1109/TEVC.2008.919004`

## Complexity

O(N log N + N^2 D) per generation plus objective-evaluation cost. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization using rank-based immigration, emigration, mutation and elitism.

## Detailed operation

Canonical rank-derived species counts, linear immigration/emigration curves, equilibrium species-probability mutation, explicit elitism and bounded SIV repair.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is
performed by the platform continuous search space. Completed-iteration accounting is kept
separate from partial iterations stopped by an evaluation or external stopping criterion.

## Parameters

The public parameter object `BiogeographyBasedOptimizationParameters` exposes only controls used by the
canonical scientific mechanism. Validation rejects non-finite probabilities/scales and
population sizes that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<BiogeographyBasedOptimizationOptimizer>(
        MetaheuristicAlgorithmIds.BiogeographyBasedOptimization);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new BiogeographyBasedOptimizationParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`biogeography-based-optimization-simon-2008`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}
\lambda_s&=I\left(1-\frac{s}{S_{\max}}\right),\\
\mu_s&=E\frac{s}{S_{\max}},\\
m(s)&=m_{\max}\left(1-\frac{P_s}{P_{\max}}\right)\end{aligned}
\f]

### Assumptions

Finite bounded continuous box; finite objective values; habitat quality ranking maps monotonically to species count; migration and mutation use the paper's linear rates.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; the implementation reproduces the stochastic migration/mutation mechanism and preserves elites explicitly.

### Scientific references

Simon (2008), Biogeography-Based Optimization, IEEE Transactions on Evolutionary Computation 12(6), 702-713.
DOI: `10.1109/TEVC.2008.919004`.
