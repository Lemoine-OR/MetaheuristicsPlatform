@page comprehensive_learning_particle_swarm_liang_qin_suganthan_baskar_2006 Comprehensive Learning Particle Swarm Optimizer

# Comprehensive Learning Particle Swarm Optimizer

## General description

Comprehensive Learning Particle Swarm Optimizer (`CLPSO`) is the public scientific identity associated with
Liang, Qin, Suganthan & Baskar (2006), Comprehensive learning particle swarm optimizer for global optimization of multimodal functions, IEEE Transactions on Evolutionary Computation 10(3), 281-295. This release deliberately documents and implements that identity without
silently mixing later variants or unrelated PSO mechanisms.

## Technical specifications

- Stable ID: `comprehensive-learning-particle-swarm-liang-qin-suganthan-baskar-2006`
- Class: `ComprehensiveLearningParticleSwarmOptimizer`
- Parameters: `ComprehensiveLearningPsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Public since: v0.92.0
- Primary DOI: `10.1109/TEVC.2005.857610`

## Complexity

O(ND) exemplar-guided movement plus N objective evaluations per iteration. Memory usage is O(ND).

## Applicability

Multimodal bounded continuous global optimization using dimension-wise exemplars learned from different particles.

## Detailed operation

Dimension-wise comprehensive learning with particle-specific Pc, two-particle tournaments, c=1.49445, inertia 0.9 to 0.4 and refreshing gap m=7.

All objective evaluations use the common `OptimizationContext`, respect objective sense and
carry the caller cancellation token. Boundary repair uses the bounded continuous search space.

## Parameters

`ComprehensiveLearningPsoParameters` exposes only the controls required by this scientific identity and validates
the conditions needed by its equations.

## API example

```csharp
var algorithm =
    MetaheuristicFactory.Create<ComprehensiveLearningParticleSwarmOptimizer>(
        MetaheuristicAlgorithmIds.ComprehensiveLearningParticleSwarm);

OptimizationResult<double[]> result =
    algorithm.Optimize(
        problem,
        new ComprehensiveLearningPsoParameters(),
        new ArraySolutionCloner<double>(),
        stoppingCriterion,
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`comprehensive-learning-particle-swarm-liang-qin-suganthan-baskar-2006`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X\subseteq\mathbb R^D} f(x)
\f]

### Update equations / iterations

\f[
\begin{aligned}v_{i,d}^{t+1}&=w_t v_{i,d}^{t}+c\,r_{i,d}^{t}\bigl(p_{f_i(d),d}^{t}-x_{i,d}^{t}\bigr).\end{aligned}
\f]

### Assumptions

Finite bounded continuous domain; particle-specific dimension exemplars and refresh after the configured stagnation gap.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted; the release preserves the published comprehensive-learning mechanism and schedules.

### Scientific references

Liang, Qin, Suganthan & Baskar (2006), Comprehensive learning particle swarm optimizer for global optimization of multimodal functions, IEEE Transactions on Evolutionary Computation 10(3), 281-295. DOI: `10.1109/TEVC.2005.857610`.
