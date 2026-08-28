@page adaptive_penalty_formulation_ga_tessema_yen_2009 Tessema-Yen Adaptive Penalty Genetic Algorithm

# Tessema-Yen Adaptive Penalty Genetic Algorithm

## General description

Tessema-Yen Adaptive Penalty Genetic Algorithm (`TessemaYenPenaltyGa`) is the public scientific identity associated with Tessema & Yen (2009), *An Adaptive Penalty Formulation for Constrained Evolutionary Optimization*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `adaptive-penalty-formulation-ga-tessema-yen-2009`
- Class: `TessemaYenPenaltyGaOptimizer`
- Parameters: `TessemaYenPenaltyGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.124.0
- Primary DOI/permanent identifier: `10.1109/TSMCA.2009.2013333`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

Normalized objective and violation distance is combined with a feasible-ratio-driven adaptive penalty, retaining useful infeasible candidates without a user-tuned penalty coefficient.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`TessemaYenPenaltyGaParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.TessemaYenPenaltyGa;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new TessemaYenPenaltyGaOptimizer().Optimize(problem, new TessemaYenPenaltyGaParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`adaptive-penalty-formulation-ga-tessema-yen-2009`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}F_p(x)&=d(x)+(1-r_f)X(x)+r_fY(x).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Tessema & Yen (2009), *An Adaptive Penalty Formulation for Constrained Evolutionary Optimization*, IEEE Transactions on Systems, Man, and Cybernetics, Part A 39(3), 565-578. DOI/permanent identifier: `10.1109/TSMCA.2009.2013333`.
