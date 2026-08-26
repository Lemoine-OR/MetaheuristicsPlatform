@page big_bang_big_crunch_erol_eksin_2006 Big Bang-Big Crunch

# Big Bang-Big Crunch

## General description

Big Bang-Big Crunch (`BB-BC`) is the scientific identity introduced by Erol & Eksin in 2006.
This page documents the canonical bounded-continuous platform implementation corresponding to that publication,
without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `big-bang-big-crunch-erol-eksin-2006`
- Class: `BigBangBigCrunchOptimizer`
- Parameters: `BigBangBigCrunchParameters`
- Family: Other / music-inspired methods
- Search space: bounded continuous vectors
- Public since: v0.78.0
- Primary DOI: `10.1016/j.advengsoft.2005.04.005`

## Complexity

O(ND) per iteration plus N objective evaluations. Memory usage is O(ND).

## Applicability

Bounded continuous derivative-free optimization using alternating random expansion and a shrinking Big-Crunch representative.

## Detailed operation

Published BB-BC minimal-cost representative option with Gaussian Big-Bang sampling whose radius decreases as alpha/t; no later local-search hybrid is mixed in.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed
by the platform continuous search space before objective evaluation. The implementation preserves the
published stochastic mechanism while using the platform's explicit completed-iteration accounting.

## Parameters

The public parameter object `BigBangBigCrunchParameters` exposes only controls used by the canonical scientific mechanism.
Validation rejects population sizes or numerical controls that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<BigBangBigCrunchOptimizer>(
        MetaheuristicAlgorithmIds.BigBangBigCrunch);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new BigBangBigCrunchParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`big-bang-big-crunch-erol-eksin-2006`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}c^t&=x_{b(t)}^t,\quad b(t)\in\operatorname*{arg\,min}_i f(x_i^t),\\x_{i,d}^{t+1}&=c_d^t+z_{i,d}^t\,\alpha\,\frac{u_d-\ell_d}{t},\quad z_{i,d}^t\sim\mathcal N(0,1)\end{aligned}
\f]

### Assumptions

Finite bounded continuous box, finite objective values and at least two candidates. The original paper explicitly allows the Big-Crunch representative to be either the center of mass or the minimal-cost candidate; this identity uses the published minimal-cost option so it remains well-defined for zero and signed objectives.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; the published Gaussian radius contracts proportionally to 1/t around the current minimal-cost representative.

### Scientific references

Erol & Eksin (2006), A new optimization method: Big Bang-Big Crunch, Advances in Engineering Software 37(2), 106-111.
DOI: `10.1016/j.advengsoft.2005.04.005`.
