@page fuzzy_clustering_niching_ga_imrani_bouroumi_zine_limouri_2000 Fuzzy-Clustering Niching Genetic Algorithm

# Fuzzy-Clustering Niching Genetic Algorithm

## General description

Fuzzy-Clustering Niching Genetic Algorithm (`FuzzyClusteringNichingGa`) is the public scientific identity associated with
Imrani, Bouroumi, Zine & Limouri (2000), *A fuzzy clustering-based niching approach to multimodal function optimization*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `fuzzy-clustering-niching-ga-imrani-bouroumi-zine-limouri-2000`
- Class: `FuzzyClusteringNichingGaOptimizer`
- Parameters: `FuzzyClusteringNichingGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.140.0
- Primary DOI/permanent identifier: `10.1016/S1389-0417(99)00013-3`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

Fuzzy clustering identifies and maintains niches without a fixed sharing radius; reproduction is organized within the inferred clusters.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`FuzzyClusteringNichingGaParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.FuzzyClusteringNichingGa;
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
    new FuzzyClusteringNichingGaOptimizer().Optimize(
        problem,
        new FuzzyClusteringNichingGaParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`fuzzy-clustering-niching-ga-imrani-bouroumi-zine-limouri-2000`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}u_{ik}&=\frac{1}{\sum_\ell(d_{ik}/d_{i\ell})^{2/(m-1)}},\\c_k&=\frac{\sum_i u_{ik}^m x_i}{\sum_i u_{ik}^m}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Imrani, Bouroumi, Zine & Limouri (2000), *A fuzzy clustering-based niching approach to multimodal function optimization*, Cognitive Systems Research 1(2), 119-133.
DOI/permanent identifier: `10.1016/S1389-0417(99)00013-3`.
