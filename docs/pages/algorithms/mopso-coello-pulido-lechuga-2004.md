@page mopso_coello_pulido_lechuga_2004 Multiobjective Particle Swarm Optimizer

# Multiobjective Particle Swarm Optimizer

## General description

Multiobjective Particle Swarm Optimizer (`Mopso`) is the public scientific identity associated with
Coello Coello, Pulido & Lechuga (2004), Handling Multiple Objectives With Particle Swarm Optimization, IEEE Transactions on Evolutionary Computation 8(3), 256-279. This release documents and implements that identity without silently
mixing unrelated variants or reducing the objective vector to an undocumented scalar surrogate.

## Technical specifications

- Stable ID: `mopso-coello-pulido-lechuga-2004`
- Class: `MopsoOptimizer`
- Parameters: `MopsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.101.0
- Primary DOI: `10.1109/TEVC.2004.826067`

## Complexity

O(ND+AM) swarm motion and repository maintenance per iteration. Memory usage is O((N+A)(D+M)).

## Applicability

Bounded continuous native multiobjective Pareto optimization.

## Detailed operation

Pareto-repository PSO with adaptive hypercubes, inverse-density leader selection, pbest dominance and decaying mutation. The implementation operates directly on objective vectors, uses the common bounded continuous search space, respects the declared optimization sense of every objective and propagates the caller cancellation token.

## Parameters

`MopsoParameters` exposes the controls required by this scientific identity and validates the numerical and structural conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Mopso;
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.Multiobjective;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem =
    new ContinuousMultiobjectiveOptimizationProblem(
        BoundedContinuousSearchSpace.Uniform(
            4,
            0.0,
            1.0),
        new[]
        {
            OptimizationSense.Minimize,
            OptimizationSense.Minimize
        },
        static (
            ReadOnlySpan<double> x,
            Span<double> f) =>
        {
            f[0] = x[0];
            f[1] =
                1.0 -
                Math.Sqrt(x[0]) +
                x[1] +
                x[2] +
                x[3];
        });

MopsoOptimizer algorithm =
    MetaheuristicFactory.Create<MopsoOptimizer>(
        MetaheuristicAlgorithmIds.Mopso);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new MopsoParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`mopso-coello-pulido-lechuga-2004`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}v_i^{t+1}&=0.4v_i^t+r_1(p_i^t-x_i^t)+r_2(g_i^t-x_i^t),\qquad g_i^t\sim\operatorname{GridRoulette}(A_t),\\p_m(t)&=(1-t/T)^{5/\eta}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, at least two finite objectives, and one explicit OptimizationSense per objective.

### Convergence conditions

No universal finite-time convergence guarantee is asserted; the implementation preserves the named multiobjective mechanism.

### Scientific references

Coello Coello, Pulido & Lechuga (2004), Handling Multiple Objectives With Particle Swarm Optimization, IEEE Transactions on Evolutionary Computation 8(3), 256-279. DOI: `10.1109/TEVC.2004.826067`.
