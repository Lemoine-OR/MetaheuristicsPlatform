@page cooperative_particle_swarm_cpso_sk_van_den_bergh_engelbrecht_2004 Cooperative Particle Swarm Optimization (CPSO-SK)

# Cooperative Particle Swarm Optimization (CPSO-SK)

## General description

Cooperative Particle Swarm Optimization (CPSO-SK) (`CPSO-SK`) is the public scientific identity associated with
van den Bergh & Engelbrecht (2004), A Cooperative Approach to Particle Swarm Optimization, IEEE Transactions on Evolutionary Computation 8(3), 225-239. This release deliberately documents and implements that identity without
silently mixing later variants or unrelated PSO mechanisms.

## Technical specifications

- Stable ID: `cooperative-particle-swarm-cpso-sk-van-den-bergh-engelbrecht-2004`
- Class: `CooperativeParticleSwarmOptimizer`
- Parameters: `CooperativePsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.93.0
- Primary DOI: `10.1109/TEVC.2004.826069`

## Complexity

O(KSD) movement with K*S context-vector objective evaluations per iteration. Memory usage is O(SD + D).

## Applicability

Bounded continuous optimization benefiting from cooperative decomposition of the decision vector into component subspaces.

## Detailed operation

CPSO-SK cooperative decomposition: K sub-swarms optimize disjoint components and are evaluated through a shared context vector assembled from the current best component of every sub-swarm.

All objective evaluations use the common `OptimizationContext`, respect objective sense and
carry the caller cancellation token. Boundary repair uses the bounded continuous search space.

## Parameters

`CooperativePsoParameters` exposes only the controls required by this scientific identity and validates
the conditions needed by its equations.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<CooperativeParticleSwarmOptimizer>(
        MetaheuristicAlgorithmIds.CooperativeParticleSwarm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new CooperativePsoParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`cooperative-particle-swarm-cpso-sk-van-den-bergh-engelbrecht-2004`

## Mathematical details

### Problem formulation

\f[
\min_{x=(x^{(1)},\dots,x^{(K)})\in\mathcal X} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}x_{\mathrm{ctx}}&=\bigl(g^{(1)},\dots,g^{(K)}\bigr),\\f_i^{(k)}&=f\!\left(g^{(1)},\dots,x_i^{(k)},\dots,g^{(K)}\right).\end{aligned}
\f]

### Assumptions

Finite bounded continuous vector, deterministic balanced component partition and finite objective values.

### Convergence conditions

No universal global convergence guarantee is asserted; the release is explicitly CPSO-SK and does not silently claim the hybrid CPSO-HK variant.

### Scientific references

van den Bergh & Engelbrecht (2004), A Cooperative Approach to Particle Swarm Optimization, IEEE Transactions on Evolutionary Computation 8(3), 225-239. DOI: `10.1109/TEVC.2004.826069`.
