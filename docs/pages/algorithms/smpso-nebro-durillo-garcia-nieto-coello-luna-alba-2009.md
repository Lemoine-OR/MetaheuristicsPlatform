@page smpso_nebro_durillo_garcia_nieto_coello_luna_alba_2009 SMPSO

# SMPSO

## General description

SMPSO (`Smpso`) is the public scientific identity associated with
Nebro, Durillo, Garcia-Nieto, Coello Coello, Luna & Alba (2009), SMPSO: A New PSO-Based Metaheuristic for Multi-objective Optimization, IEEE MCDM 2009, 66-73. This release documents and implements that identity without silently
mixing unrelated variants or reducing the objective vector to an undocumented scalar surrogate.

## Technical specifications

- Stable ID: `smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009`
- Class: `SmpsoOptimizer`
- Parameters: `SmpsoParameters`
- Family: Swarm intelligence
- Search space: bounded continuous vectors
- Objective model: native Pareto vector with explicit `OptimizationSense` per objective
- Public since: v0.102.0
- Primary DOI: `10.1109/MCDM.2009.4938830`

## Complexity

O(ND+AM) speed-constrained swarm motion and archive maintenance. Memory usage is O((N+A)(D+M)).

## Applicability

Bounded continuous native multiobjective Pareto optimization.

## Detailed operation

Speed-constrained MOPSO with constriction, componentwise velocity bounds, polynomial turbulence and external archive. The implementation operates directly on objective vectors, uses the common bounded continuous search space, respects the declared optimization sense of every objective and propagates the caller cancellation token.

## Parameters

`SmpsoParameters` exposes the controls required by this scientific identity and validates the numerical and structural conditions used by its update equations.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Multiobjective.Smpso;
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

SmpsoOptimizer algorithm =
    MetaheuristicFactory.Create<SmpsoOptimizer>(
        MetaheuristicAlgorithmIds.Smpso);

MultiobjectiveOptimizationResult result =
    algorithm.Optimize(
        problem,
        new SmpsoParameters(),
        new OptimizationOptions
        {
            Seed = 123456UL
        });
```

## Stable factory ID

`smpso-nebro-durillo-garcia-nieto-coello-luna-alba-2009`

## Mathematical details

### Problem formulation

\f[
\operatorname{ParetoMin}_{x\in\mathcal X\subseteq\mathbb R^D}F(x)=\bigl(f_1(x),\ldots,f_M(x)\bigr)
\f]

### Update equations / iterations

\f[
\begin{aligned}\tilde v_{i,d}^{t+1}&=\chi\!\left(0.2v_{i,d}^t+c_1r_1(p_{i,d}^t-x_{i,d}^t)+c_2r_2(g_{i,d}^t-x_{i,d}^t)\right),\\v_{i,d}^{t+1}&=\operatorname{clip}(\tilde v_{i,d}^{t+1},-\Delta_d,\Delta_d),\qquad \Delta_d=(u_d-l_d)/2.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, at least two finite objectives, and one explicit OptimizationSense per objective.

### Convergence conditions

No universal finite-time convergence guarantee is asserted; the implementation preserves the named multiobjective mechanism.

### Scientific references

Nebro, Durillo, Garcia-Nieto, Coello Coello, Luna & Alba (2009), SMPSO: A New PSO-Based Metaheuristic for Multi-objective Optimization, IEEE MCDM 2009, 66-73. DOI: `10.1109/MCDM.2009.4938830`.
