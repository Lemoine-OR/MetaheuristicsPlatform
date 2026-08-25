@page flower_pollination_algorithm_yang_2012 Flower Pollination Algorithm

# Flower Pollination Algorithm

## General description

Flower Pollination Algorithm (`FPA`) is the scientific identity introduced by
Yang in 2012. This page documents the canonical bounded-continuous
platform implementation corresponding to that publication, without silently mixing later
variants or hybridizations.

## Technical specifications

- Stable ID: `flower-pollination-algorithm-yang-2012`
- Class: `FlowerPollinationOptimizer`
- Parameters: `FlowerPollinationParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.71.0
- Primary DOI: `10.1007/978-3-642-32894-7_27`

## Complexity

O(ND) per generation plus N objective evaluations. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization mixing global Levy pollination and local flower constancy.

## Detailed operation

Canonical switch-controlled global Levy pollination toward the current best and local random flower-pair pollination with greedy replacement. The Levy-flight operator uses a fixed internal Mantegna beta=1.5 numerical realization rather than exposing a later variant parameter.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is
performed by the platform continuous search space. Completed-iteration accounting is kept
separate from partial iterations stopped by an evaluation or external stopping criterion.

## Parameters

The public parameter object `FlowerPollinationParameters` exposes only controls used by the
canonical scientific mechanism. Validation rejects non-finite probabilities/scales and
population sizes that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<FlowerPollinationOptimizer>(
        MetaheuristicAlgorithmIds.FlowerPollinationAlgorithm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new FlowerPollinationParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`flower-pollination-algorithm-yang-2012`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}
x_i^{t+1}&=x_i^t+\gamma L(g_*-x_i^t)&&\text{(global)},\\
x_i^{t+1}&=x_i^t+\varepsilon(x_j^t-x_k^t)&&\text{(local)}\end{aligned}
\f]

### Assumptions

Finite bounded continuous box; finite objective values; global pollination uses a dimension-wise Levy vector and local pollination samples two distinct population members.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; stochastic exploration/exploitation is governed by the switch probability and Levy-flight scaling.

### Scientific references

Yang (2012), Flower Pollination Algorithm for Global Optimization, UCNC 2012, LNCS 7445, 240-249.
DOI: `10.1007/978-3-642-32894-7_27`.
