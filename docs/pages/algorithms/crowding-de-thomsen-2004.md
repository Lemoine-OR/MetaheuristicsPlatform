@page crowding_de_thomsen_2004 Crowding Differential Evolution

# Crowding Differential Evolution

## General description

Crowding Differential Evolution (`CrowdingDe`) is the public scientific identity associated with
Thomsen (2004), *Multimodal optimization using crowding-based differential evolution*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `crowding-de-thomsen-2004`
- Class: `CrowdingDeOptimizer`
- Parameters: `CrowdingDeParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.132.0
- Primary DOI/permanent identifier: `10.1109/CEC.2004.1331058`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

Differential-evolution offspring compete with the closest population member rather than only with their target, allowing several niches to be maintained.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`CrowdingDeParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.CrowdingDe;
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
    new CrowdingDeOptimizer().Optimize(
        problem,
        new CrowdingDeParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`crowding-de-thomsen-2004`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}u_i&=\mathrm{DETrial}(x_i),\\j^\star&=\arg\min_j\lVert u_i-x_j\rVert_2,\\x_{j^\star}&\leftarrow u_i\quad\text{if }u_i\text{ is better}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Thomsen (2004), *Multimodal optimization using crowding-based differential evolution*, Proceedings of the 2004 Congress on Evolutionary Computation, 1382-1389.
DOI/permanent identifier: `10.1109/CEC.2004.1331058`.
