@page constriction_particle_swarm_clerc_kennedy_2002 Clerc-Kennedy Constriction Particle Swarm

# Clerc-Kennedy Constriction Particle Swarm

## General description

Clerc-Kennedy Constriction Particle Swarm (`CKPSO`) is the public scientific identity associated with
Clerc & Kennedy (2002), The particle swarm - explosion, stability, and convergence in a multidimensional complex space, IEEE Transactions on Evolutionary Computation 6(1), 58-73. This release deliberately documents and implements that identity without
silently mixing later variants or unrelated PSO mechanisms.

## Technical specifications

- Stable ID: `constriction-particle-swarm-clerc-kennedy-2002`
- Class: `ConstrictionParticleSwarmOptimizer`
- Parameters: `ConstrictionPsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.89.0
- Primary DOI: `10.1109/4235.985692`

## Complexity

O(ND) movement plus N objective evaluations per iteration. Memory usage is O(ND).

## Applicability

Bounded continuous optimization using the Clerc-Kennedy constriction analysis with phi > 4.

## Detailed operation

Canonical constriction-factor velocity dynamics with chi computed from phi=c1+c2 and kappa; default c1=c2=2.05, phi=4.10, kappa=1.

All objective evaluations use the common `OptimizationContext`, respect objective sense and
carry the caller cancellation token. Boundary repair uses the bounded continuous search space.

## Parameters

`ConstrictionPsoParameters` exposes only the controls required by this scientific identity and validates
the conditions needed by its equations.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<ConstrictionParticleSwarmOptimizer>(
        MetaheuristicAlgorithmIds.ConstrictionParticleSwarm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new ConstrictionPsoParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`constriction-particle-swarm-clerc-kennedy-2002`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}\chi&=\frac{2\kappa}{|2-\phi-\sqrt{\phi^2-4\phi}|},\quad \phi=c_1+c_2>4,\\v_{i}^{t+1}&=\chi\left(v_i^t+c_1r_1(p_i^t-x_i^t)+c_2r_2(g^t-x_i^t)\right).\end{aligned}
\f]

### Assumptions

Finite bounded continuous box; phi greater than four for the published constriction expression.

### Convergence conditions

The release implements the published constriction dynamics and exposes the stability-related phi/kappa controls; it does not claim objective-independent global convergence.

### Scientific references

Clerc & Kennedy (2002), The particle swarm - explosion, stability, and convergence in a multidimensional complex space, IEEE Transactions on Evolutionary Computation 6(1), 58-73. DOI: `10.1109/4235.985692`.
