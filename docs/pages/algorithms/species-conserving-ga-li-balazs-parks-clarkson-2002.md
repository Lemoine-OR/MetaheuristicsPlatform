@page species_conserving_ga_li_balazs_parks_clarkson_2002 Species Conserving Genetic Algorithm

# Species Conserving Genetic Algorithm

## General description

Species Conserving Genetic Algorithm (`SpeciesConservingGa`) is the public scientific identity associated with
Li, Balazs, Parks & Clarkson (2002), *A species conserving genetic algorithm for multimodal function optimization*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `species-conserving-ga-li-balazs-parks-clarkson-2002`
- Class: `SpeciesConservingGaOptimizer`
- Parameters: `SpeciesConservingGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.131.0
- Primary DOI/permanent identifier: `10.1162/106365602760234081`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

The population is partitioned into species around dominating species seeds; the seeds are explicitly conserved into the next generation.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`SpeciesConservingGaParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.SpeciesConservingGa;
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
    new SpeciesConservingGaOptimizer().Optimize(
        problem,
        new SpeciesConservingGaParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`species-conserving-ga-li-balazs-parks-clarkson-2002`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}S_t&=\mathrm{SpeciesSeeds}(P_t,\sigma_s),\\P_{t+1}&=\mathrm{Conserve}(S_t)\cup\mathrm{Offspring}(P_t).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Li, Balazs, Parks & Clarkson (2002), *A species conserving genetic algorithm for multimodal function optimization*, Evolutionary Computation 10(3), 207-234.
DOI/permanent identifier: `10.1162/106365602760234081`.
