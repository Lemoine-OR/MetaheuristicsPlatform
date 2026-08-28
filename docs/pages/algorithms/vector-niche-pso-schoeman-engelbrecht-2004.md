@page vector_niche_pso_schoeman_engelbrecht_2004 Vector-Niche Particle Swarm Optimization

# Vector-Niche Particle Swarm Optimization

## General description

Vector-Niche Particle Swarm Optimization (`VectorNichePso`) is the public scientific identity associated with
Schoeman & Engelbrecht (2004), *Using vector operations to identify niches for particle swarm optimization*. It is kept separate from adjacent multimodal
and niching mechanisms.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named scientific niching mechanism is preserved, while the common platform API, seed/cancellation plumbing, factory/catalog integration and benchmark harness are explicit platform adaptations.

## Technical specifications

- Stable ID: `vector-niche-pso-schoeman-engelbrecht-2004`
- Class: `VectorNichePsoOptimizer`
- Parameters: `VectorNichePsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Objective model: native single-objective multimodal optimization
- Result model: a set of spatially distinct candidate optima
- Public since: v0.134.0
- Primary DOI/permanent identifier: `10.1109/ICCIS.2004.1460441`

## Complexity

Population-based stochastic search. Distance-aware niching operations add pairwise or
neighborhood work on top of objective evaluations; exact cost depends on the named mechanism.

## Applicability

Bounded continuous multimodal optimization where multiple separated high-quality solutions,
global optima, local optima, or attraction basins are scientifically relevant.

## Detailed operation

Vector relationships between particles and candidate niche leaders are used to demarcate niches and maintain independent subswarms.

The implementation uses the native `IContinuousMultimodalOptimizationProblem` contract and returns
`MultimodalOptimizationResult` without collapsing the population to a single reported optimum.

## Parameters

`VectorNichePsoParameters` exposes the scientific controls used by this identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multimodal.VectorNichePso;
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
    new VectorNichePsoOptimizer().Optimize(
        problem,
        new VectorNichePsoParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`vector-niche-pso-schoeman-engelbrecht-2004`

## Mathematical details

### Problem formulation

\f[
\operatorname{FindMany}_{x\in\mathcal X} f(x)
\quad\text{with spatially distinct locally or globally optimal solutions}.
\f]

### Update equations / iterations

\f[
\begin{aligned}s_i&=\mathrm{VectorNicheBest}(x_i,P),\\v_i^{t+1}&=wv_i^t+c_1r_1(p_i-x_i^t)+c_2r_2(s_i-x_i^t).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective values, explicit optimization sense,
and Euclidean decision-space distances for niching.

### Convergence conditions

No universal finite-time guarantee of finding every optimum is asserted. The implementation
preserves the named multimodal/niching mechanism and deterministic seeded random-source contract.

### Scientific references

Schoeman & Engelbrecht (2004), *Using vector operations to identify niches for particle swarm optimization*, IEEE Conference on Cybernetics and Intelligent Systems, 361-366.
DOI/permanent identifier: `10.1109/ICCIS.2004.1460441`.
