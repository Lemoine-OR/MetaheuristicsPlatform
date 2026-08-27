@page inertia_weight_particle_swarm_shi_eberhart_1998 Inertia Weight Particle Swarm Optimization

# Inertia Weight Particle Swarm Optimization

## General description

Inertia Weight Particle Swarm Optimization (`IWPSO`) is the public scientific identity associated with
Shi & Eberhart (1998), A Modified Particle Swarm Optimizer, Proceedings of the 1998 IEEE International Conference on Evolutionary Computation, 69-73. This release deliberately documents and implements that identity without
silently mixing later variants or unrelated PSO mechanisms.

## Technical specifications

- Stable ID: `inertia-weight-particle-swarm-shi-eberhart-1998`
- Class: `InertiaWeightParticleSwarmOptimizer`
- Parameters: `InertiaWeightPsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.88.0
- Primary DOI: `10.1109/ICEC.1998.699146`

## Complexity

O(ND) movement plus N objective evaluations per iteration. Memory usage is O(ND).

## Applicability

Bounded continuous derivative-free optimization using the 1998 constant inertia-weight PSO mechanism.

## Detailed operation

Canonical synchronous global-best PSO with explicit constant inertia weight w; it does not silently substitute the later linearly decreasing inertia schedule.

All objective evaluations use the common `OptimizationContext`, respect objective sense and
carry the caller cancellation token. Boundary repair uses the bounded continuous search space.

## Parameters

`InertiaWeightPsoParameters` exposes only the controls required by this scientific identity and validates
the conditions needed by its equations.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<InertiaWeightParticleSwarmOptimizer>(
        MetaheuristicAlgorithmIds.InertiaWeightParticleSwarm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new InertiaWeightPsoParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`inertia-weight-particle-swarm-shi-eberhart-1998`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}v_{i,d}^{t+1}&=w v_{i,d}^{t}+c_1r_1(p_{i,d}^{t}-x_{i,d}^{t})+c_2r_2(g_d^t-x_{i,d}^t),\\x_{i,d}^{t+1}&=x_{i,d}^t+v_{i,d}^{t+1}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous box, finite objective values, synchronous personal/global best updates and a constant inertia weight.

### Convergence conditions

No universal global convergence guarantee is asserted; the release preserves the inertia-weight mechanism introduced by Shi and Eberhart without mixing later schedules.

### Scientific references

Shi & Eberhart (1998), A Modified Particle Swarm Optimizer, Proceedings of the 1998 IEEE International Conference on Evolutionary Computation, 69-73. DOI: `10.1109/ICEC.1998.699146`.
