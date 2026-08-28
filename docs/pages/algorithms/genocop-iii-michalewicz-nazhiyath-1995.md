@page genocop_iii_michalewicz_nazhiyath_1995 GENOCOP III

# GENOCOP III

## General description

GENOCOP III (`GenocopIII`) is the public scientific identity associated with Michalewicz & Nazhiyath (1995), *GENOCOP III: A co-evolutionary algorithm for numerical optimization problems with nonlinear constraints*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `genocop-iii-michalewicz-nazhiyath-1995`
- Class: `GenocopIIIOptimizer`
- Parameters: `GenocopIIIParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.126.0
- Primary DOI/permanent identifier: `10.1109/ICEC.1995.487460`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

Search and feasible reference populations co-evolve; infeasible search points are repaired toward feasible reference points by segment bisection before evaluation.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`GenocopIIIParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.GenocopIII;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new GenocopIIIOptimizer().Optimize(problem, new GenocopIIIParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`genocop-iii-michalewicz-nazhiyath-1995`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}x^{\prime}&=(1-\alpha)r+\alpha x,\qquad r\in\mathcal F,\quad \alpha=\max\{a:(1-a)r+ax\in\mathcal F\}.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Michalewicz & Nazhiyath (1995), *GENOCOP III: A co-evolutionary algorithm for numerical optimization problems with nonlinear constraints*, Proceedings of the 2nd IEEE International Conference on Evolutionary Computation. DOI/permanent identifier: `10.1109/ICEC.1995.487460`.
