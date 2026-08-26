@page imperialist_competitive_algorithm_atashpaz_gargari_lucas_2007 Imperialist Competitive Algorithm

# Imperialist Competitive Algorithm

## General description

Imperialist Competitive Algorithm (`ICA`) is the scientific identity introduced by Atashpaz-Gargari and Lucas in 2007. This page documents the canonical bounded-continuous platform implementation corresponding to that publication, without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007`
- Class: `ImperialistCompetitiveAlgorithmOptimizer`
- Parameters: `ImperialistCompetitiveAlgorithmParameters`
- Family: Other / music-inspired methods
- Search space: bounded continuous vectors
- Public since: v0.83.0
- Primary DOI: `10.1109/CEC.2007.4425083`

## Complexity

O(ND) per iteration plus assimilation/revolution objective evaluations. Memory usage is O(ND + N).

## Applicability

Bounded continuous derivative-free optimization using countries, empires, assimilation, revolution and imperialistic competition.

## Detailed operation

Canonical ICA lifecycle with fitness-ranked imperialists, probabilistic colony allocation, beta-scaled assimilation with angular deviation, revolution, imperialist/colony exchange and weakest-empire competition.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed by the platform continuous search space before objective evaluation.

## Parameters

The public parameter object `ImperialistCompetitiveAlgorithmParameters` exposes only controls used by the canonical scientific mechanism. Validation rejects controls or objective domains that would silently alter the published equations.

## Stable factory ID

`imperialist-competitive-algorithm-atashpaz-gargari-lucas-2007`

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<ImperialistCompetitiveAlgorithmOptimizer>(
        MetaheuristicAlgorithmIds.ImperialistCompetitiveAlgorithm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new ImperialistCompetitiveAlgorithmParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}s&\sim\mathcal U(0,\beta d),\quad \theta\sim\mathcal U(-\gamma,\gamma),\\x_c^{t+1}&=x_c^t+s\,u(x_I^t-x_c^t,\theta),\\C_n&=c_n+\zeta\,\overline c_{\mathrm{colonies}(n)}\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and finite objective values; at least two imperialists and at least one colony. Objective sense is converted only to an order-equivalent competition cost.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; assimilation, revolution and imperialistic competition preserve the stochastic search mechanism of the 2007 ICA.

### Scientific references

Atashpaz-Gargari and Lucas (2007), Imperialist competitive algorithm: An algorithm for optimization inspired by imperialistic competition, IEEE Congress on Evolutionary Computation, 4661-4667. DOI: `10.1109/CEC.2007.4425083`.
