@page bare_bones_particle_swarm_kennedy_2003 Bare Bones Particle Swarm

# Bare Bones Particle Swarm

## General description

Bare Bones Particle Swarm (`BBPSO`) is the public scientific identity associated with
Kennedy (2003), Bare bones particle swarms, Proceedings of the 2003 IEEE Swarm Intelligence Symposium, 80-87. This release deliberately documents and implements that identity without
silently mixing later variants or unrelated PSO mechanisms.

## Technical specifications

- Stable ID: `bare-bones-particle-swarm-kennedy-2003`
- Class: `BareBonesParticleSwarmOptimizer`
- Parameters: `BareBonesPsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.90.0
- Primary DOI: `10.1109/SIS.2003.1202251`

## Complexity

O(ND) Gaussian sampling plus N objective evaluations per iteration. Memory usage is O(ND).

## Applicability

Bounded continuous optimization using velocity-free Gaussian sampling around personal/global best positions.

## Detailed operation

Velocity is eliminated. Each coordinate is sampled from a Gaussian centered at (p_i+g)/2 with standard deviation |p_i-g|, followed by bounded repair and personal-best update.

All objective evaluations use the common `OptimizationContext`, respect objective sense and
carry the caller cancellation token. Boundary repair uses the bounded continuous search space.

## Parameters

`BareBonesPsoParameters` exposes only the controls required by this scientific identity and validates
the conditions needed by its equations.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<BareBonesParticleSwarmOptimizer>(
        MetaheuristicAlgorithmIds.BareBonesParticleSwarm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new BareBonesPsoParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`bare-bones-particle-swarm-kennedy-2003`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}x_{i,d}^{t+1}&\sim\mathcal N\!\left(\frac{p_{i,d}^t+g_d^t}{2},\,|p_{i,d}^t-g_d^t|\right).\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and finite objective values; global-best bare-bones form.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; the release is the velocity-free Gaussian mechanism of Kennedy (2003).

### Scientific references

Kennedy (2003), Bare bones particle swarms, Proceedings of the 2003 IEEE Swarm Intelligence Symposium, 80-87. DOI: `10.1109/SIS.2003.1202251`.
