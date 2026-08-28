@page ensemble_constraint_handling_mallipeddi_suganthan_2010 Ensemble of Constraint Handling Techniques

# Ensemble of Constraint Handling Techniques

## General description

Ensemble of Constraint Handling Techniques (`EnsembleConstraintHandling`) is the public scientific identity associated with Mallipeddi & Suganthan (2010), *Ensemble of constraint handling techniques*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `ensemble-constraint-handling-mallipeddi-suganthan-2010`
- Class: `EnsembleConstraintHandlingOptimizer`
- Parameters: `EnsembleConstraintHandlingParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.128.0
- Primary DOI/permanent identifier: `10.1109/TEVC.2009.2033582`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

Multiple subpopulations apply distinct constraint-handling techniques and periodically exchange elites, preserving the ensemble principle.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`EnsembleConstraintHandlingParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.EnsembleConstraintHandling;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new EnsembleConstraintHandlingOptimizer().Optimize(problem, new EnsembleConstraintHandlingParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`ensemble-constraint-handling-mallipeddi-suganthan-2010`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}P_{t+1}^{(k)}&=\mathcal E_k(P_t^{(k)}),\qquad \text{elite exchange across }k.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Mallipeddi & Suganthan (2010), *Ensemble of constraint handling techniques*, IEEE Transactions on Evolutionary Computation 14(4), 561-579. DOI/permanent identifier: `10.1109/TEVC.2009.2033582`.
