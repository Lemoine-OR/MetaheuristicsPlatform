@page proximity_search_fischetti_monaci_2014 Proximity Search

# Proximity Search

## General description

Proximity Search (`ProximitySearchMatheuristic`) is the public scientific identity associated with
Fischetti & Monaci (2014), *Proximity search for 0-1 mixed-integer convex programming*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The proximity objective and improvement cutoff are preserved; generic exact-repair requests replace a specific black-box MIP API.

## Technical specifications

- Stable ID: `proximity-search-fischetti-monaci-2014`
- Class: `ProximitySearchMatheuristicOptimizer`
- Parameters: `ProximitySearchMatheuristicParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.163.0
- Primary DOI/permanent identifier: `10.1007/s10732-014-9266-x`

## Complexity

One exact proximity subproblem per iteration.

Space: O(n) incumbent/reference state plus exact-solver state.

## Applicability

0-1 mixed-integer optimization with an incumbent and exact solver supporting objective cutoffs.

## Detailed operation

Replaces the subproblem objective by distance to the incumbent while imposing an original-objective cutoff that forces improvement.

## Parameters

`ProximitySearchMatheuristicParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new ProximitySearchMatheuristicOptimizer().Optimize(
        domain,
        new ProximitySearchMatheuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`proximity-search-fischetti-monaci-2014`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:x\in X,\ x_B\in\{0,1\}^{|B|}\}
\f]

### Update equations / iterations

\f[
\begin{aligned}\min\ &\Delta_B(x,\bar x)\\
\text{s.t. }&c^\top x\le c^\top\bar x-\theta,\quad x\in X.\end{aligned}
\f]

### Assumptions

The domain exposes finite objective values, variable-kind metadata, integer-feasibility testing,
a feasible incumbent constructor, and deterministic exact/relaxation solve callbacks for a fixed
platform seed and solver configuration.

### Convergence conditions

No universal finite-time global-convergence claim is asserted for the matheuristic wrapper.
Whenever an exact restricted subproblem is solved to optimality by the supplied domain, the
returned point is exact only for that restricted subproblem; global optimality is not implied.

### Scientific references

Fischetti & Monaci (2014), *Proximity search for 0-1 mixed-integer convex programming*, Journal of Heuristics 20(6), 709-731.
DOI/permanent identifier: `10.1007/s10732-014-9266-x`.
