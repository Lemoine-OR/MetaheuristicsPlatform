@page homomorphous_mapping_ea_koziel_michalewicz_1999 Homomorphous-Mapping Evolutionary Algorithm

# Homomorphous-Mapping Evolutionary Algorithm

## General description

Homomorphous-Mapping Evolutionary Algorithm (`HomomorphousMappingEa`) is the public scientific identity associated with Koziel & Michalewicz (1999), *Evolutionary algorithms, homomorphous mappings, and constrained parameter optimization*. The release keeps this mechanism separate from neighboring constraint-handling strategies.

## Technical specifications

- Stable ID: `homomorphous-mapping-ea-koziel-michalewicz-1999`
- Class: `HomomorphousMappingEaOptimizer`
- Parameters: `HomomorphousMappingEaParameters`
- Family: Evolutionary methods
- Search space: bounded continuous vectors
- Constraint model: native inequalities `g_i(x) <= 0` and equalities `h_j(x) = 0`
- Public since: v0.127.0
- Primary DOI/permanent identifier: `10.1162/evco.1999.7.1.19`

## Complexity

Population-based stochastic search; cost is dominated by objective/constraint evaluations and the named ranking, penalty, repair or ensemble operation.

## Applicability

Bounded continuous constrained optimization with finite objective/constraint evaluations and explicit equality tolerance.

## Detailed operation

A feasible reference point anchors a decoder that maps search points into the feasible region by radial segment projection and bisection.

The implementation consumes `IContinuousConstrainedOptimizationProblem`; constraints are never silently folded into the objective except where the named scientific method explicitly defines a transformed score.

## Parameters

`HomomorphousMappingEaParameters` exposes the controls required by this scientific identity and validates them before search.

## API example

```csharp
using MetaheuristicsPlatform.Algorithms.Constraints.HomomorphousMappingEa;
using MetaheuristicsPlatform.Constraints;
using MetaheuristicsPlatform.Core;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

var problem = new ContinuousConstrainedOptimizationProblem(
    BoundedContinuousSearchSpace.Uniform(2, 0.0, 1.0),
    OptimizationSense.Minimize, 1, 0,
    static x => x[0] * x[0] + x[1] * x[1],
    static (ReadOnlySpan<double> x, Span<double> g, Span<double> h) => { g[0] = 1.0 - x[0] - x[1]; });
var result = new HomomorphousMappingEaOptimizer().Optimize(problem, new HomomorphousMappingEaParameters(), new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`homomorphous-mapping-ea-koziel-michalewicz-1999`

## Mathematical details

### Problem formulation

\f[
\min/\max\; f(x)\quad\text{s.t.}\quad g_i(x)\le0,\quad h_j(x)=0,\quad x\in\mathcal X.
\f]

### Update equations / iterations

\f[
\begin{aligned}H(u)&=x_0+\rho(u)(u-x_0),\qquad H(u)\in\mathcal F.\end{aligned}
\f]

### Assumptions

Finite bounded continuous decision box, finite objective/constraint values, explicit optimization sense, and explicit equality tolerance.

### Convergence conditions

No universal finite-time global convergence guarantee is asserted. The implementation preserves the named stochastic constraint-handling mechanism.

### Scientific references

Koziel & Michalewicz (1999), *Evolutionary algorithms, homomorphous mappings, and constrained parameter optimization*, Evolutionary Computation 7(1), 19-44. DOI/permanent identifier: `10.1162/evco.1999.7.1.19`.
