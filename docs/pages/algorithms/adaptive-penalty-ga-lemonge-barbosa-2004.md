@page adaptive_penalty_ga_lemonge_barbosa_2004 Lemonge-Barbosa Adaptive Penalty Genetic Algorithm

# Lemonge-Barbosa Adaptive Penalty Genetic Algorithm

## General description

Lemonge-Barbosa Adaptive Penalty Genetic Algorithm (`AdaptivePenaltyGa`) is the public scientific identity associated with Lemonge & Barbosa (2004), *An adaptive penalty scheme for genetic algorithms in structural optimization*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `adaptive-penalty-ga-lemonge-barbosa-2004`
- Class: `AdaptivePenaltyGaOptimizer`
- Parameters: `AdaptivePenaltyGaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.123.0
- Primary DOI/permanent identifier: `10.1002/nme.899`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

Parameter-less constraint-specific penalty coefficients are recomputed from population-average objective and violation information.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`AdaptivePenaltyGaParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.AdaptivePenaltyGa;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new AdaptivePenaltyGaOptimizer().Optimize(problem, new AdaptivePenaltyGaParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`adaptive-penalty-ga-lemonge-barbosa-2004`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}k_j&=\frac{|\langle\widetilde f\rangle|\langle v_j\rangle}{\sum_\ell\langle v_\ell\rangle^2},\\F(x)&=\bar f(x)+\sum_j k_jv_j(x).\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Lemonge & Barbosa (2004), *An adaptive penalty scheme for genetic algorithms in structural optimization*, International Journal for Numerical Methods in Engineering 59, 703-736. DOI/permanent identifier: `10.1002/nme.899`.
