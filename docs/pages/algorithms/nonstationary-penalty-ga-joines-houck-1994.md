@page nonstationary_penalty_ga_joines_houck_1994 Joines-Houck Nonstationary Penalty Genetic Algorithm

# Joines-Houck Nonstationary Penalty Genetic Algorithm

## General description

Joines-Houck Nonstationary Penalty Genetic Algorithm (`JoinesHouckPenaltyGa`) is the public scientific identity associated with Joines & Houck (1994), *On the use of non-stationary penalty functions to solve nonlinear constrained optimization problems with GA's*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `nonstationary-penalty-ga-joines-houck-1994`
- Class: `JoinesHouckPenaltyGaOptimizer`
- Parameters: `JoinesHouckPenaltyGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.121.0
- Primary DOI/permanent identifier: `10.1109/ICEC.1994.349995`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

Generation-dependent nonstationary penalty pressure follows the Joines-Houck mechanism with explicit C, alpha and beta controls.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`JoinesHouckPenaltyGaParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.JoinesHouckPenaltyGa;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new JoinesHouckPenaltyGaOptimizer().Optimize(problem, new JoinesHouckPenaltyGaParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`nonstationary-penalty-ga-joines-houck-1994`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}F(x,t)&=\widetilde f(x)+(Ct)^{\alpha}\sum_j v_j(x)^{\beta}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Joines & Houck (1994), *On the use of non-stationary penalty functions to solve nonlinear constrained optimization problems with GA's*, Proceedings of the First IEEE Conference on Evolutionary Computation. DOI/permanent identifier: `10.1109/ICEC.1994.349995`.
