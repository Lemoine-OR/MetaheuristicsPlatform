@page gravitational_search_algorithm_rashedi_nezamabadi_pour_saryazdi_2009 Gravitational Search Algorithm

# Gravitational Search Algorithm

## General description

Gravitational Search Algorithm (`GSA`) is the scientific identity introduced by Rashedi, Nezamabadi-pour & Saryazdi in 2009.
This page documents the canonical bounded-continuous platform implementation corresponding to that publication,
without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009`
- Class: `GravitationalSearchOptimizer`
- Parameters: `GravitationalSearchParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.79.0
- Primary DOI: `10.1016/j.ins.2009.03.004`

## Complexity

O(N K D) per iteration plus N objective evaluations, with K decreasing from the population toward the published 2% terminal fraction. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization using fitness-dependent masses, decaying gravitational attraction and velocity updates.

## Detailed operation

Canonical GSA mass normalization, G(t)=G0 exp(-alpha t/T), elitist Kbest force set decreasing toward 2%, randomly weighted gravitational acceleration, velocity and position updates.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed
by the platform continuous search space before objective evaluation. The implementation preserves the
published stochastic mechanism while using the platform's explicit completed-iteration accounting.

## Parameters

The public parameter object `GravitationalSearchParameters` exposes only controls used by the canonical scientific mechanism.
Validation rejects population sizes or numerical controls that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<GravitationalSearchOptimizer>(
        MetaheuristicAlgorithmIds.GravitationalSearch);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new GravitationalSearchParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`gravitational-search-algorithm-rashedi-nezamabadi-pour-saryazdi-2009`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}m_i^t&=\frac{f_i^t-w^t}{b^t-w^t},\quad M_i^t=\frac{m_i^t}{\sum_j m_j^t},\\G^t&=G_0e^{-\alpha t/T},\\a_{i,d}^t&=\sum_{j\in K_t,\,j\ne i}r_j^tG^tM_j^t\frac{x_{j,d}^t-x_{i,d}^t}{\lVert x_j^t-x_i^t\rVert_2+\varepsilon},\\v_{i,d}^{t+1}&=r_i^t v_{i,d}^t+a_{i,d}^t,\quad x_{i,d}^{t+1}=x_{i,d}^t+v_{i,d}^{t+1}\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and finite objective values; population size at least two; equal-fitness generations use equal normalized masses as the continuous limiting tie case; Kbest is clamped to at least one agent for finite populations.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; gravitational strength decays exponentially and the published elitist Kbest set shrinks to emphasize exploitation.

### Scientific references

Rashedi, Nezamabadi-pour & Saryazdi (2009), GSA: A Gravitational Search Algorithm, Information Sciences 179(13), 2232-2248.
DOI: `10.1016/j.ins.2009.03.004`.
