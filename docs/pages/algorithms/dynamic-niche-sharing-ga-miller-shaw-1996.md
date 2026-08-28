@page dynamic_niche_sharing_ga_miller_shaw_1996 Dynamic Niche Sharing Genetic Algorithm

# Dynamic Niche Sharing Genetic Algorithm

## General description

Dynamic Niche Sharing Genetic Algorithm (`DynamicNicheSharingGa`) is the public scientific identity associated with
Miller & Shaw (1996), *Genetic algorithms with dynamic niche sharing for multimodal function optimization*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `dynamic-niche-sharing-ga-miller-shaw-1996`
- Class: `DynamicNicheSharingGaOptimizer`
- Parameters: `DynamicNicheSharingGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.139.0
- Primary DOI/permanent identifier: `10.1109/ICEC.1996.542701`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

The sharing radius is updated from the current population structure and shared fitness penalizes densely occupied niches.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`DynamicNicheSharingGaParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.DynamicNicheSharingGa;
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
    new DynamicNicheSharingGaOptimizer().Optimize(
        problem,
        new DynamicNicheSharingGaParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`dynamic-niche-sharing-ga-miller-shaw-1996`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}\sigma_t&=\mathrm{DynamicSharingRadius}(P_t),\\m_i&=\sum_j\max\left\{0,1-\left(\frac{d_{ij}}{\sigma_t}\right)^\alpha\right\},\\f_i'&=\frac{f_i}{m_i}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Miller & Shaw (1996), *Genetic algorithms with dynamic niche sharing for multimodal function optimization*, Proceedings of the IEEE International Conference on Evolutionary Computation, 786-791.
DOI/permanent identifier: `10.1109/ICEC.1996.542701`.
