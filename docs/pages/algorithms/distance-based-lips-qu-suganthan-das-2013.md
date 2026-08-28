@page distance_based_lips_qu_suganthan_das_2013 Distance-Based Locally Informed Particle Swarm

# Distance-Based Locally Informed Particle Swarm

## General description

Distance-Based Locally Informed Particle Swarm (`LocallyInformedPso`) is the public scientific identity associated with
Qu, Suganthan & Das (2013), *A Distance-Based Locally Informed Particle Swarm Model for Multimodal Optimization*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `distance-based-lips-qu-suganthan-das-2013`
- Class: `LocallyInformedPsoOptimizer`
- Parameters: `LocallyInformedPsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.137.0
- Primary DOI/permanent identifier: `10.1109/TEVC.2012.2203138`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

Each particle is informed by distance-selected neighboring personal bests rather than one global leader, enabling several optima to coexist.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`LocallyInformedPsoParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.LocallyInformedPso;
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
    new LocallyInformedPsoOptimizer().Optimize(
        problem,
        new LocallyInformedPsoParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`distance-based-lips-qu-suganthan-das-2013`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}\mathcal N_i&=\mathrm{Nearest}(x_i,k),\\v_i^{t+1}&=wv_i^t+\frac{1}{|\mathcal N_i|}\sum_{j\in\mathcal N_i}c_jr_j(p_j-x_i^t).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Qu, Suganthan & Das (2013), *A Distance-Based Locally Informed Particle Swarm Model for Multimodal Optimization*, IEEE Transactions on Evolutionary Computation 17(3), 387-402.
DOI/permanent identifier: `10.1109/TEVC.2012.2203138`.
