@page whale_optimization_algorithm_mirjalili_lewis_2016 Whale Optimization Algorithm

# Whale Optimization Algorithm

## General description

Whale Optimization Algorithm (`WOA`) is the scientific identity introduced by Mirjalili & Lewis in 2016.
This page documents the canonical bounded-continuous platform implementation corresponding to that
publication, without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `whale-optimization-algorithm-mirjalili-lewis-2016`
- Class: `WhaleOptimizationAlgorithmOptimizer`
- Parameters: `WhaleOptimizationAlgorithmParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.74.0
- Primary DOI: `10.1016/j.advengsoft.2016.01.008`

## Complexity

O(ND) per generation plus N objective evaluations. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization with encircling, random-prey exploration and logarithmic bubble-net spirals.

## Detailed operation

Canonical WOA with scalar A/C coefficients per whale, random-prey exploration for |A|>=1, best-prey encircling for |A|<1, and the 50/50 logarithmic spiral mechanism.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed
by the platform continuous search space before objective evaluation. The implementation preserves the
published stochastic mechanism while using the platform's explicit completed-iteration accounting.

## Parameters

The public parameter object `WhaleOptimizationAlgorithmParameters` exposes only controls used by the canonical scientific mechanism.
Validation rejects population sizes or numerical controls that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<WhaleOptimizationAlgorithmOptimizer>(
        MetaheuristicAlgorithmIds.WhaleOptimizationAlgorithm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new WhaleOptimizationAlgorithmParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`whale-optimization-algorithm-mirjalili-lewis-2016`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}a_t&=2\left(1-\frac{t}{T}\right),\quad A=2a_tr_1-a_t,\quad C=2r_2,\\a_{2,t}&=-1-\frac{t}{T},\quad \ell\sim\mathcal U[a_{2,t},1],\quad p\sim\mathcal U[0,1),\\D_{\star}&=\left|CX^{\star}-X\right|,\quad X^{t+1}=X^{\star}-AD_{\star}\quad(p<\tfrac12,\ |A|<1),\\D_r&=\left|CX_r-X\right|,\quad X^{t+1}=X_r-AD_r\quad(p<\tfrac12,\ |A|\ge 1),\\D_s&=\left|X^{\star}-X\right|,\quad X^{t+1}=D_se^{b\ell}\cos(2\pi\ell)+X^{\star}\quad(p\ge\tfrac12)\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and finite objective values; at least two whales; the best-so-far whale is the prey estimate and random-prey exploration samples the current population.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; the linear decrease of a from 2 toward 0 reduces the probability of |A|>=1 exploration.

### Scientific references

Mirjalili & Lewis (2016), The Whale Optimization Algorithm, Advances in Engineering Software 95, 51-67.
DOI: `10.1016/j.advengsoft.2016.01.008`.
