@page sine_cosine_algorithm_mirjalili_2016 Sine Cosine Algorithm

# Sine Cosine Algorithm

## General description

Sine Cosine Algorithm (`SCA`) is the scientific identity introduced by Mirjalili in 2016.
This page documents the canonical bounded-continuous platform implementation corresponding to that
publication, without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `sine-cosine-algorithm-mirjalili-2016`
- Class: `SineCosineAlgorithmOptimizer`
- Parameters: `SineCosineAlgorithmParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.75.0
- Primary DOI: `10.1016/j.knosys.2015.12.022`

## Complexity

O(ND) per generation plus N objective evaluations. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization with sine/cosine oscillation around the best destination.

## Detailed operation

Canonical SCA with linearly decreasing r1, r2 in [0,2pi), r3 in [0,2), and an equiprobable sine/cosine switch r4.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed
by the platform continuous search space before objective evaluation. The implementation preserves the
published stochastic mechanism while using the platform's explicit completed-iteration accounting.

## Parameters

The public parameter object `SineCosineAlgorithmParameters` exposes only controls used by the canonical scientific mechanism.
Validation rejects population sizes or numerical controls that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<SineCosineAlgorithmOptimizer>(
        MetaheuristicAlgorithmIds.SineCosineAlgorithm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new SineCosineAlgorithmParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`sine-cosine-algorithm-mirjalili-2016`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}r_1(t)&=a\left(1-\frac{t}{T}\right),\quad r_2\sim\mathcal U[0,2\pi),\quad r_3\sim\mathcal U[0,2),\\x_{i,d}^{t+1}&=x_{i,d}^{t}+r_1\sin(r_2)\left|r_3p_d^t-x_{i,d}^{t}\right|\quad(r_4<\tfrac12),\\x_{i,d}^{t+1}&=x_{i,d}^{t}+r_1\cos(r_2)\left|r_3p_d^t-x_{i,d}^{t}\right|\quad(r_4\ge\tfrac12),\\r_4&\sim\mathcal U[0,1)\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and finite objective values; at least two search agents; the destination is the best-so-far candidate under the configured objective sense.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; r1 decreases linearly from the canonical amplitude a=2 toward zero.

### Scientific references

Mirjalili (2016), SCA: A Sine Cosine Algorithm for solving optimization problems, Knowledge-Based Systems 96, 120-133.
DOI: `10.1016/j.knosys.2015.12.022`.
