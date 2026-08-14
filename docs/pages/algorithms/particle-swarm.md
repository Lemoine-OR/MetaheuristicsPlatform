@page particle_swarm Particle Swarm Optimization

# Particle Swarm Optimization

## General description

Flat particle-major buffers, deterministic target-owned RNG streams, graphless fully-connected canonical fast path, calibrated movement/evaluation parallelism.

## Technical specifications

- **Stable factory ID:** `particle-swarm`
- **Implementation class:** `ParticleSwarmOptimizer`
- **Family:** Swarm intelligence
- **Source:** `src/MetaheuristicsPlatform/Algorithms/PSO/ParticleSwarmOptimizer.cs`
- **Runtime creation:** direct typed factory creation

## Complexity

- **Time:** O(ND) per iteration for the canonical graphless fast path; topology/social policies may add overhead
- **Space:** O(ND)

## Applicability

Continuous bounded search spaces; generic platform infrastructure also supports alternative social/topology policies

## Detailed operation

The implementation follows the cited scientific method while preserving the platform invariants: deterministic random streams where applicable, explicit ownership of mutable state, common stopping/callback lifecycle, and no avoidable hot-loop allocation.

## Parameters

Generic: seed, stopping criteria, callbacks, cancellation, evaluation execution. Specific parameters are exposed by the algorithm parameter object and documented by the generated API reference.

## API example


```csharp
var algorithm =
    MetaheuristicFactory.Create<ParticleSwarmOptimizer>(
        "particle-swarm");
```


## Stable factory ID

```text
particle-swarm
```

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
v_{i,d}^{k+1}=\omega_k v_{i,d}^{k}+c_1r_{1,i,d}(p_{i,d}^{k}-x_{i,d}^{k})+c_2r_{2,i,d}(g_{i,d}^{k}-x_{i,d}^{k}),\qquad x_{i,d}^{k+1}=x_{i,d}^{k}+v_{i,d}^{k+1}
\f]

### Assumptions

The canonical implementation assumes a bounded continuous representation. Stability results depend on the chosen inertia/constriction and stochastic assumptions; topology changes the social guide but not the generic state model.

### Convergence conditions

No universal finite-time global convergence claim is made. Under the Clerc–Kennedy constriction analysis, stability conditions can be derived for the linearized stochastic dynamics; practical convergence still depends on objective, topology and parameterization.

### Scientific references

Kennedy & Eberhart (1995), Particle Swarm Optimization, IEEE ICNN; Clerc & Kennedy (2002), The particle swarm — explosion, stability, and convergence in a multidimensional complex space, IEEE TEC 6(1), 58–73

DOI: `10.1109/4235.985692`

## Scientific references

- Kennedy & Eberhart (1995), Particle Swarm Optimization, IEEE ICNN; Clerc & Kennedy (2002), The particle swarm — explosion, stability, and convergence in a multidimensional complex space, IEEE TEC 6(1), 58–73
- DOI: `10.1109/4235.985692`
