@page biobjective_multimodal_ea_deb_saha_2012 Bi-Objective Evolutionary Multimodal Optimizer

# Bi-Objective Evolutionary Multimodal Optimizer

## General description

Bi-Objective Evolutionary Multimodal Optimizer (`BiobjectiveMultimodalEa`) is the public scientific identity associated with
Deb & Saha (2012), *Multimodal optimization using a bi-objective evolutionary algorithm*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `biobjective-multimodal-ea-deb-saha-2012`
- Class: `BiobjectiveMultimodalEaOptimizer`
- Parameters: `BiobjectiveMultimodalEaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.138.0
- Primary DOI/permanent identifier: `10.1162/EVCO_a_00042`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

The original objective is paired with a finite-difference gradient-norm objective; weak Pareto selection preserves separated stationary optima under the platform's bounded continuous contract.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`BiobjectiveMultimodalEaParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.BiobjectiveMultimodalEa;
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
    new BiobjectiveMultimodalEaOptimizer().Optimize(
        problem,
        new BiobjectiveMultimodalEaParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`biobjective-multimodal-ea-deb-saha-2012`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}F_1(x)&=\widetilde f(x),\\F_2(x)&=\lVert\nabla f(x)\rVert_2,\\P_{t+1}&=\mathrm{ParetoSelect}(P_t\cup Q_t).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Deb & Saha (2012), *Multimodal optimization using a bi-objective evolutionary algorithm*, Evolutionary Computation 20(1), 27-62.
DOI/permanent identifier: `10.1162/EVCO_a_00042`.
