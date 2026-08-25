@page bat_algorithm_yang_2010 Bat Algorithm

# Bat Algorithm

## General description

Bat Algorithm (`BA`) is the scientific identity introduced by
Yang in 2010. This page documents the canonical bounded-continuous
platform implementation corresponding to that publication, without silently mixing later
variants or hybridizations.

## Technical specifications

- Stable ID: `bat-algorithm-yang-2010`
- Class: `BatAlgorithmOptimizer`
- Parameters: `BatAlgorithmParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.70.0
- Primary DOI: `10.1007/978-3-642-12538-6_6`

## Complexity

O(ND) per generation plus N objective evaluations. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization with frequency-tuned motion, pulse-rate local search and loudness acceptance.

## Detailed operation

Canonical frequency/velocity/position update, best-centered local random walk, loudness-gated greedy acceptance, geometric loudness decay and asymptotic pulse-rate growth.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is
performed by the platform continuous search space. Completed-iteration accounting is kept
separate from partial iterations stopped by an evaluation or external stopping criterion.

## Parameters

The public parameter object `BatAlgorithmParameters` exposes only controls used by the
canonical scientific mechanism. Validation rejects non-finite probabilities/scales and
population sizes that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<BatAlgorithmOptimizer>(
        MetaheuristicAlgorithmIds.BatAlgorithm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new BatAlgorithmParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`bat-algorithm-yang-2010`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}
f_i&=f_{\min}+(f_{\max}-f_{\min})\beta,\\
v_i^t&=v_i^{t-1}+(x_i^{t-1}-x_*)f_i,\\
x_i^t&=x_i^{t-1}+v_i^t\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and finite objective values; loudness is positive, pulse rate lies in [0,1], and alpha/gamma are positive control parameters.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; the implementation follows Yang's published coupled loudness/pulse adaptation.

### Scientific references

Yang (2010), A New Metaheuristic Bat-Inspired Algorithm, NICSO 2010, 65-74.
DOI: `10.1007/978-3-642-12538-6_6`.
