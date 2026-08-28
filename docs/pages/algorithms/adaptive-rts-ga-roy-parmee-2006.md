@page adaptive_rts_ga_roy_parmee_2006 Adaptive Restricted Tournament Selection Genetic Algorithm

# Adaptive Restricted Tournament Selection Genetic Algorithm

## General description

Adaptive Restricted Tournament Selection Genetic Algorithm (`AdaptiveRestrictedTournamentGa`) is the public scientific identity associated with
Roy & Parmee (2006), *Adaptive Restricted Tournament Selection for the identification of multiple sub-optima in a multi-modal function*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `adaptive-rts-ga-roy-parmee-2006`
- Class: `AdaptiveRestrictedTournamentGaOptimizer`
- Parameters: `AdaptiveRestrictedTournamentGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.141.0
- Primary DOI/permanent identifier: `10.1007/BFb0032787`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

Shared near-neighbor clustering adapts the restricted tournament competition neighborhood, avoiding a fixed modality-dependent niche radius.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`AdaptiveRestrictedTournamentGaParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.AdaptiveRestrictedTournamentGa;
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
    new AdaptiveRestrictedTournamentGaOptimizer().Optimize(
        problem,
        new AdaptiveRestrictedTournamentGaParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`adaptive-rts-ga-roy-parmee-2006`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}w_t&=\mathrm{AdaptiveWindow}(P_t),\\j^\star&=\arg\min_{j\in\mathcal T_i(w_t)}\lVert u_i-x_j\rVert_2,\\x_{j^\star}&\leftarrow u_i\quad\text{if }u_i\text{ is better}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Roy & Parmee (2006), *Adaptive Restricted Tournament Selection for the identification of multiple sub-optima in a multi-modal function*, Evolutionary Computing, 236-256.
DOI/permanent identifier: `10.1007/BFb0032787`.
