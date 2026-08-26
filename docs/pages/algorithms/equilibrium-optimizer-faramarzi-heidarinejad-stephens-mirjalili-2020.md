@page equilibrium_optimizer_faramarzi_heidarinejad_stephens_mirjalili_2020 Equilibrium Optimizer

# Equilibrium Optimizer

## General description

Equilibrium Optimizer (`EO`) is the scientific identity introduced by Faramarzi, Heidarinejad, Stephens and Mirjalili in 2020. This page documents the canonical bounded-continuous platform implementation corresponding to that publication, without silently mixing later variants, binary adaptations, multi-objective extensions or hybridizations.

## Technical specifications

- Stable ID: `equilibrium-optimizer-faramarzi-heidarinejad-stephens-mirjalili-2020`
- Class: `EquilibriumOptimizer`
- Parameters: `EquilibriumOptimizerParameters`
- Family: Other / music-inspired methods
- Search space: bounded continuous vectors
- Public since: v0.87.0
- Primary DOI: `10.1016/j.knosys.2019.105190`

## Complexity

O(ND) per iteration plus N objective evaluations. Memory usage is O(ND).

## Applicability

Bounded continuous derivative-free optimization using an equilibrium pool, exponential turnover and generation-rate control.

## Detailed operation

Canonical EO with four best equilibrium candidates plus their average, a1=2, a2=1, GP=0.5 defaults, exponential F term, generation probability and source-code memory saving.

All objective evaluations pass through the common `OptimizationContext`; boundary repair is performed by the platform continuous search space before objective evaluation.

## Parameters

The public parameter object `EquilibriumOptimizerParameters` exposes only controls used by the canonical scientific mechanism. Validation rejects controls or objective domains that would silently alter the published equations.

## Stable factory ID

`equilibrium-optimizer-faramarzi-heidarinejad-stephens-mirjalili-2020`

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<EquilibriumOptimizer>(
        MetaheuristicAlgorithmIds.EquilibriumOptimizer);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new EquilibriumOptimizerParameters(),
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
\begin{aligned}t&=(1-\tau/T)^{a_2\tau/T},\quad F=a_1\operatorname{sign}(r-0.5)(e^{-\lambda t}-1),\\G_0&=GCP(C_{eq}-\lambda C),\quad G=G_0F,\\C^{t+1}&=C_{eq}+(C-C_{eq})F+\frac{G}{\lambda V}(1-F)\end{aligned}
\f]

### Assumptions

Finite bounded continuous box, finite objective values and at least four particles. The equilibrium pool contains the four best-so-far candidates and their arithmetic mean; V=1 is the canonical internal constant.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; the exponential turnover term and generation probability balance exploration and exploitation around the equilibrium pool.

### Scientific references

Faramarzi, Heidarinejad, Stephens and Mirjalili (2020), Equilibrium optimizer: A novel optimization algorithm, Knowledge-Based Systems 191, 105190. DOI: `10.1016/j.knosys.2019.105190`.
