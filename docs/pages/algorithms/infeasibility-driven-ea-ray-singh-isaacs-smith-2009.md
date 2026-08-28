@page infeasibility_driven_ea_ray_singh_isaacs_smith_2009 Infeasibility Driven Evolutionary Algorithm

# Infeasibility Driven Evolutionary Algorithm

## General description

Infeasibility Driven Evolutionary Algorithm (`InfeasibilityDrivenEa`) is the public scientific identity associated with Ray, Singh, Isaacs & Smith (2009), *Infeasibility Driven Evolutionary Algorithm for Constrained Optimization*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `infeasibility-driven-ea-ray-singh-isaacs-smith-2009`
- Class: `InfeasibilityDrivenEaOptimizer`
- Parameters: `InfeasibilityDrivenEaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.129.0
- Primary DOI/permanent identifier: `10.1007/978-3-642-00619-7_7`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

Environmental selection deliberately retains a controlled infeasible fraction so search pressure can follow active constraint boundaries.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`InfeasibilityDrivenEaParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.InfeasibilityDrivenEa;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new InfeasibilityDrivenEaOptimizer().Optimize(problem, new InfeasibilityDrivenEaParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`infeasibility-driven-ea-ray-singh-isaacs-smith-2009`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}P_{t+1}&=P_{t+1}^{\mathrm{feas}}\cup P_{t+1}^{\mathrm{infeas}},\qquad |P_{t+1}^{\mathrm{infeas}}|=\lfloor\rho N\rfloor.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Ray, Singh, Isaacs & Smith (2009), *Infeasibility Driven Evolutionary Algorithm for Constrained Optimization*, Constraint-Handling in Evolutionary Optimization, Studies in Computational Intelligence 198, 145-165. DOI/permanent identifier: `10.1007/978-3-642-00619-7_7`.
