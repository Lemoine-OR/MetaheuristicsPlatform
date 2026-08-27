@page standard_particle_swarm_bratton_kennedy_2007 Standard Particle Swarm Optimization 2007

# Standard Particle Swarm Optimization 2007

## General description

Standard Particle Swarm Optimization 2007 (`SPSO-2007`) is the public scientific identity associated with
Bratton & Kennedy (2007), Defining a Standard for Particle Swarm Optimization, Proceedings of the 2007 IEEE Swarm Intelligence Symposium, 120-127. This release deliberately documents and implements that identity without
silently mixing later variants or unrelated PSO mechanisms.

## Technical specifications

- Stable ID: `standard-particle-swarm-bratton-kennedy-2007`
- Class: `StandardPso2007Optimizer`
- Parameters: `StandardPso2007Parameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.94.0
- Primary DOI: `10.1109/SIS.2007.368035`

## Complexity

O(ND + NK) per iteration plus N objective evaluations. Memory usage is O(ND + N^2) for particle state and the explicit adaptive random informer graph.

## Applicability

Bounded continuous baseline PSO with standardized parameter values and adaptive random local communication.

## Detailed operation

SPSO-2007 parameterization w=1/(2 ln 2), c=1/2+ln 2, default swarm size 10+floor(2 sqrt(D)), K=3 random informing attempts and topology regeneration on non-improvement.

All objective evaluations use the common `OptimizationContext`, respect objective sense and
carry the caller cancellation token. Boundary repair uses the bounded continuous search space.

## Parameters

`StandardPso2007Parameters` exposes only the controls required by this scientific identity and validates
the conditions needed by its equations.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<StandardPso2007Optimizer>(
        MetaheuristicAlgorithmIds.StandardParticleSwarm2007);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new StandardPso2007Parameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`standard-particle-swarm-bratton-kennedy-2007`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}v_{i,d}^{t+1}&=w v_{i,d}^{t}+U(0,c)(p_{i,d}^{t}-x_{i,d}^{t})+U(0,c)(l_{i,d}^{t}-x_{i,d}^{t}).\end{aligned}
\f]

### Assumptions

Finite bounded continuous box and a nonempty random informer neighborhood for every particle.

### Convergence conditions

The release is a reproducible SPSO-2007 baseline; no objective-independent finite-time global convergence guarantee is asserted.

### Scientific references

Bratton & Kennedy (2007), Defining a Standard for Particle Swarm Optimization, Proceedings of the 2007 IEEE Swarm Intelligence Symposium, 120-127. DOI: `10.1109/SIS.2007.368035`.
