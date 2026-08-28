@page neighborhood_mutation_de_qu_suganthan_liang_2012 Neighborhood-Mutation Differential Evolution

# Neighborhood-Mutation Differential Evolution

## General description

Neighborhood-Mutation Differential Evolution (`NeighborhoodMutationDe`) is the public scientific identity associated with
Qu, Suganthan & Liang (2012), *Differential Evolution With Neighborhood Mutation for Multimodal Optimization*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `neighborhood-mutation-de-qu-suganthan-liang-2012`
- Class: `NeighborhoodMutationDeOptimizer`
- Parameters: `NeighborhoodMutationDeParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.136.0
- Primary DOI/permanent identifier: `10.1109/TEVC.2011.2161873`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

DE mutation is restricted to Euclidean neighborhoods, preserving distinct attraction basins while evolving each niche toward its own optimum.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`NeighborhoodMutationDeParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.NeighborhoodMutationDe;
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
    new NeighborhoodMutationDeOptimizer().Optimize(
        problem,
        new NeighborhoodMutationDeParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`neighborhood-mutation-de-qu-suganthan-liang-2012`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}\mathcal N_i&=\mathrm{Nearest}(x_i,k),\\v_i&=x_{r_1}+F(x_{r_2}-x_{r_3}),\qquad r_1,r_2,r_3\in\mathcal N_i.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Qu, Suganthan & Liang (2012), *Differential Evolution With Neighborhood Mutation for Multimodal Optimization*, IEEE Transactions on Evolutionary Computation 16(5), 601-614.
DOI/permanent identifier: `10.1109/TEVC.2011.2161873`.
