@page salp_swarm_algorithm_mirjalili_gandomi_mirjalili_saremi_faris_mirjalili_2017 Salp Swarm Algorithm

# Salp Swarm Algorithm

## General description

Salp Swarm Algorithm (`SSA`) is the scientific identity introduced by Mirjalili, Gandomi, Mirjalili, Saremi, Faris & Mirjalili in 2017.
This page documents the canonical bounded-continuous platform implementation corresponding to that
publication, without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `salp-swarm-algorithm-mirjalili-gandomi-mirjalili-saremi-faris-mirjalili-2017`
- Class: `SalpSwarmAlgorithmOptimizer`
- Parameters: `SalpSwarmAlgorithmParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.76.0
- Primary DOI: `10.1016/j.advengsoft.2017.07.002`

## Complexity

O(ND) per generation plus N objective evaluations. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization with food-directed leaders and chain-following salps.

## Detailed operation

Canonical single-objective SSA with c1=2 exp(-(4t/T)^2), leader coordinates sampled around the food source, and followers updated by the published half-sum chain rule.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed
by the platform continuous search space before objective evaluation. The implementation preserves the
published stochastic mechanism while using the platform's explicit completed-iteration accounting.

## Parameters

The public parameter object `SalpSwarmAlgorithmParameters` exposes only controls used by the canonical scientific mechanism.
Validation rejects population sizes or numerical controls that make the published update undefined.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<SalpSwarmAlgorithmOptimizer>(
        MetaheuristicAlgorithmIds.SalpSwarmAlgorithm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new SalpSwarmAlgorithmParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`salp-swarm-algorithm-mirjalili-gandomi-mirjalili-saremi-faris-mirjalili-2017`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}\ell&=t+1,\quad T_s=T+1,\quad c_1(t)=2e^{-\left(4\ell/T_s\right)^2},\\x_{i,d}^{t+1}&=F_d+c_1\left((u_d-l_d)c_2+l_d\right)\quad(i\le\lfloor N/2\rfloor,\ c_3<\tfrac12),\\x_{i,d}^{t+1}&=F_d-c_1\left((u_d-l_d)c_2+l_d\right)\quad(i\le\lfloor N/2\rfloor,\ c_3\ge\tfrac12),\\x_{i,d}^{t+1}&=\frac{x_{i,d}^{t}+x_{i-1,d}^{t+1}}{2}\quad(i>\lfloor N/2\rfloor)\end{aligned}
\f]


**Platform iteration mapping.** The authors' MATLAB implementation counts the initial fitness-only pass as source iteration 1 and starts position updates at `l = 2`. `MaximumIterations` in MetaheuristicsPlatform counts completed position-update iterations, so the exact source schedule is mapped with `l = iteration + 1` and `Max_iter_source = MaximumIterations + 1`. The source ordering is preserved: the entire salp chain is updated with a frozen food position before boundary repair, fitness evaluation and food replacement.

### Assumptions

Finite bounded continuous box and finite objective values; at least four salps; the food source is the best-so-far solution and the first half of the chain acts as leaders.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; c1 decreases rapidly toward zero, progressively contracting leader motion around the food source.

### Scientific references

Mirjalili, Gandomi, Mirjalili, Saremi, Faris & Mirjalili (2017), Salp Swarm Algorithm: A bio-inspired optimizer for engineering design problems, Advances in Engineering Software 114, 163-191.
DOI: `10.1016/j.advengsoft.2017.07.002`.
