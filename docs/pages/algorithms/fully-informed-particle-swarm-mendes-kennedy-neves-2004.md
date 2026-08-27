@page fully_informed_particle_swarm_mendes_kennedy_neves_2004 Fully Informed Particle Swarm

# Fully Informed Particle Swarm

## General description

Fully Informed Particle Swarm (`FIPS`) is the public scientific identity associated with
Mendes, Kennedy & Neves (2004), The Fully Informed Particle Swarm: Simpler, Maybe Better, IEEE Transactions on Evolutionary Computation 8(3), 204-210. This release deliberately documents and implements that identity without
silently mixing later variants or unrelated PSO mechanisms.

## Technical specifications

- Stable ID: `fully-informed-particle-swarm-mendes-kennedy-neves-2004`
- Class: `FullyInformedParticleSwarmOptimizer`
- Parameters: `FullyInformedPsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.91.0
- Primary DOI: `10.1109/TEVC.2004.826074`

## Complexity

O(N^2D) for the fully connected unweighted informer structure plus N evaluations per iteration. Memory usage is O(ND).

## Applicability

Bounded continuous optimization where every particle is influenced by all informers rather than a single neighborhood best.

## Detailed operation

Unweighted fully informed structure: total acceleration is divided equally among all informers, with independent random multipliers per informer and coordinate, combined with Clerc-Kennedy constriction.

All objective evaluations use the common `OptimizationContext`, respect objective sense and
carry the caller cancellation token. Boundary repair uses the bounded continuous search space.

## Parameters

`FullyInformedPsoParameters` exposes only the controls required by this scientific identity and validates
the conditions needed by its equations.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<FullyInformedParticleSwarmOptimizer>(
        MetaheuristicAlgorithmIds.FullyInformedParticleSwarm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new FullyInformedPsoParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`fully-informed-particle-swarm-mendes-kennedy-neves-2004`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}v_{i,d}^{t+1}&=\chi\!\left(v_{i,d}^{t}+\sum_{j\in N_i}\frac{\phi}{|N_i|}r_{i,j,d}(p_{j,d}^{t}-x_{i,d}^{t})\right).\end{aligned}
\f]

### Assumptions

Finite bounded continuous box; nonempty informer set; this public identity uses the unweighted fully connected informer structure.

### Convergence conditions

The constriction controls the linearized motion; no universal objective-independent global convergence guarantee is asserted.

### Scientific references

Mendes, Kennedy & Neves (2004), The Fully Informed Particle Swarm: Simpler, Maybe Better, IEEE Transactions on Evolutionary Computation 8(3), 204-210. DOI: `10.1109/TEVC.2004.826074`.
