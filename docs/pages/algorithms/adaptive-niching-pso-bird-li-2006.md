@page adaptive_niching_pso_bird_li_2006 Adaptive Niching Particle Swarm Optimization

# Adaptive Niching Particle Swarm Optimization

## General description

Adaptive Niching Particle Swarm Optimization (`AdaptiveNichingPso`) is the public scientific identity associated with
Bird & Li (2006), *Adaptively choosing niching parameters in a PSO*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `adaptive-niching-pso-bird-li-2006`
- Class: `AdaptiveNichingPsoOptimizer`
- Parameters: `AdaptiveNichingPsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.135.0
- Primary DOI/permanent identifier: `10.1145/1143997.1143999`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

Population distance statistics determine the niching radius adaptively during the run instead of requiring a fixed radius supplied by the user.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`AdaptiveNichingPsoParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.AdaptiveNichingPso;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multimodal;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem =
    new ContinuousMultimodalOptimizationProblem(
        BoundedContinuousSearchSpace.Uniform(2, -1.0, 1.0),
        OptimizationSense.Minimize,
        static x =>
            Math.Sin(3.0 * Math.PI * x[0]) *
            Math.Sin(3.0 * Math.PI * x[0]) +
            Math.Sin(3.0 * Math.PI * x[1]) *
            Math.Sin(3.0 * Math.PI * x[1]));

var result =
    new AdaptiveNichingPsoOptimizer().Optimize(
        problem,
        new AdaptiveNichingPsoParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`adaptive-niching-pso-bird-li-2006`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}\sigma_t&=\mathrm{AdaptiveNicheRadius}(P_t),\\s_i&=\arg\min_{\widetilde f}\{x_j:\lVert x_j-x_i\rVert_2\le\sigma_t\}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Bird & Li (2006), *Adaptively choosing niching parameters in a PSO*, GECCO 2006, 3-10.
DOI/permanent identifier: `10.1145/1143997.1143999`.
