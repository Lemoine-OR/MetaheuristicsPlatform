@page species_based_particle_swarm_parrott_li_2006 Species-Based Particle Swarm Optimization

# Species-Based Particle Swarm Optimization

## General description

Species-Based Particle Swarm Optimization (`SPSO-Species`) is the public scientific identity associated with
Parrott & Li (2006), Locating and tracking multiple dynamic optima by a particle swarm model using speciation, IEEE Transactions on Evolutionary Computation 10(4), 440-458. This release deliberately documents and implements that identity without
silently mixing later variants or unrelated PSO mechanisms.

## Technical specifications

- Stable ID: `species-based-particle-swarm-parrott-li-2006`
- Class: `SpeciesBasedParticleSwarmOptimizer`
- Parameters: `SpeciesBasedPsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.95.0
- Primary DOI: `10.1109/TEVC.2005.859468`

## Complexity

O(N^2D) species reconstruction plus O(ND) movement and N objective evaluations per iteration. Memory usage is O(ND + N).

## Applicability

Static multimodal bounded continuous optimization where multiple species should form around different dominating personal-best seeds.

## Detailed operation

Static SPSO mode: personal bests are sorted by dominance, species seeds are formed with a distance radius, and each particle uses its current species seed as neighborhood best.

All objective evaluations use the common `OptimizationContext`, respect objective sense and
carry the caller cancellation token. Boundary repair uses the bounded continuous search space.

## Parameters

`SpeciesBasedPsoParameters` exposes only the controls required by this scientific identity and validates
the conditions needed by its equations.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<SpeciesBasedParticleSwarmOptimizer>(
        MetaheuristicAlgorithmIds.SpeciesBasedParticleSwarm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new SpeciesBasedPsoParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`species-based-particle-swarm-parrott-li-2006`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)\quad\text{with multiple attraction basins}
\f]

### Update equations / iterations

\f[
\begin{aligned}N_i^t&=s(i,t),\\v_i^{t+1}&=w v_i^t+c_1r_1(p_i^t-x_i^t)+c_2r_2(p_{s(i,t)}^t-x_i^t).\end{aligned}
\f]

### Assumptions

Finite bounded continuous domain, finite objective values and a positive species radius; this release implements the static multimodal SPSO mode, not the dynamic-environment extension.

### Convergence conditions

The method intentionally maintains multiple species and therefore does not assert single-point swarm convergence or a universal global-optimum guarantee.

### Scientific references

Parrott & Li (2006), Locating and tracking multiple dynamic optima by a particle swarm model using speciation, IEEE Transactions on Evolutionary Computation 10(4), 440-458. DOI: `10.1109/TEVC.2005.859468`.
