@page homaifar_penalty_ga_1994 Homaifar-Qi-Lai Penalty Genetic Algorithm

# Homaifar-Qi-Lai Penalty Genetic Algorithm

## General description

Homaifar-Qi-Lai Penalty Genetic Algorithm (`HomaifarPenaltyGa`) is the public scientific identity associated with Homaifar, Qi & Lai (1994), *Constrained optimization via genetic algorithms*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `homaifar-penalty-ga-1994`
- Class: `HomaifarPenaltyGaOptimizer`
- Parameters: `HomaifarPenaltyGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.122.0
- Primary DOI/permanent identifier: `10.1177/003754979406200405`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

Static multilevel penalty uses user-defined violation levels and a distinct penalty coefficient for each constraint/level pair; the active violation penalty is quadratic.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`HomaifarPenaltyGaParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.HomaifarPenaltyGa;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new HomaifarPenaltyGaOptimizer().Optimize(problem, new HomaifarPenaltyGaParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`homaifar-penalty-ga-1994`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}F(x)&=\widetilde f(x)+\sum_j R_{k(j),j}v_j(x)^2,\qquad k(j)\text{ selected by violation level}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Homaifar, Qi & Lai (1994), *Constrained optimization via genetic algorithms*, SIMULATION 62(4), 242-253. DOI/permanent identifier: `10.1177/003754979406200405`.
