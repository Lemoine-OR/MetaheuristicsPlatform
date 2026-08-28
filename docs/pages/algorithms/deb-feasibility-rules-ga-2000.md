@page deb_feasibility_rules_ga_2000 Deb Feasibility Rules Genetic Algorithm

# Deb Feasibility Rules Genetic Algorithm

## General description

Deb Feasibility Rules Genetic Algorithm (`DebConstraintGa`) is the public scientific identity associated with Deb (2000), *An efficient constraint handling method for genetic algorithms*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `deb-feasibility-rules-ga-2000`
- Class: `DebConstraintGaOptimizer`
- Parameters: `DebConstraintGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.118.0
- Primary DOI/permanent identifier: `10.1016/S0045-7825(99)00389-8`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

Feasibility-first Deb rules compare feasible candidates by objective, prefer feasible over infeasible, and compare infeasible candidates by aggregate constraint violation.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`DebConstraintGaParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.DebConstraintGa;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new DebConstraintGaOptimizer().Optimize(problem, new DebConstraintGaParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`deb-feasibility-rules-ga-2000`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}x\prec y&\iff\begin{cases}f(x)\prec f(y),&x,y\in\mathcal F,\\x\in\mathcal F,&y\notin\mathcal F,\\V(x)<V(y),&x,y\notin\mathcal F.\end{cases}\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Deb (2000), *An efficient constraint handling method for genetic algorithms*, Computer Methods in Applied Mechanics and Engineering 186, 311-338. DOI/permanent identifier: `10.1016/S0045-7825(99)00389-8`.
