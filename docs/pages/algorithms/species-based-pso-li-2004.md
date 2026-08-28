@page species_based_pso_li_2004 Species-Based Particle Swarm Optimization

# Species-Based Particle Swarm Optimization

## General description

Species-Based Particle Swarm Optimization (`SpeciesBasedPso`) is the public scientific identity associated with
Li (2004), *Adaptively choosing neighbourhood bests using species in a particle swarm optimizer for multimodal function optimization*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `species-based-pso-li-2004`
- Class: `SpeciesBasedPsoOptimizer`
- Parameters: `SpeciesBasedPsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.133.0
- Primary DOI/permanent identifier: `10.1007/978-3-540-24854-5_10`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

Particles are grouped into species around dominant seeds; each particle uses its species seed as its neighborhood best.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`SpeciesBasedPsoParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.SpeciesBasedPso;
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
    new SpeciesBasedPsoOptimizer().Optimize(
        problem,
        new SpeciesBasedPsoParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`species-based-pso-li-2004`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}v_i^{t+1}&=wv_i^t+c_1r_1(p_i-x_i^t)+c_2r_2(s_i-x_i^t),\\x_i^{t+1}&=x_i^t+v_i^{t+1},\qquad s_i=\mathrm{SpeciesSeed}(x_i).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Li (2004), *Adaptively choosing neighbourhood bests using species in a particle swarm optimizer for multimodal function optimization*, GECCO 2004, LNCS 3102, 105-116.
DOI/permanent identifier: `10.1007/978-3-540-24854-5_10`.
