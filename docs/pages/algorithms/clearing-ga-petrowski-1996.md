@page clearing_ga_petrowski_1996 Clearing Genetic Algorithm

# Clearing Genetic Algorithm

## General description

Clearing Genetic Algorithm (`ClearingGa`) is the public scientific identity associated with
Petrowski (1996), *A clearing procedure as a niching method for genetic algorithms*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `clearing-ga-petrowski-1996`
- Class: `ClearingGaOptimizer`
- Parameters: `ClearingGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.130.0
- Primary DOI/permanent identifier: `10.1109/ICEC.1996.542703`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

Clearing assigns the available niche resources only to the best individuals within each niche, with an explicit niche radius and niche capacity.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`ClearingGaParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.ClearingGa;
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
    new ClearingGaOptimizer().Optimize(
        problem,
        new ClearingGaParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`clearing-ga-petrowski-1996`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}\mathrm{winner}(N)&=\arg\min_{x\in N}\widetilde f(x),\\\mathrm{fitness}(x)&\leftarrow+\infty\quad\text{for cleared excess members of }N.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Petrowski (1996), *A clearing procedure as a niching method for genetic algorithms*, Proceedings of the IEEE International Conference on Evolutionary Computation.
DOI/permanent identifier: `10.1109/ICEC.1996.542703`.
