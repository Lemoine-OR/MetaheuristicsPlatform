@page dins_ghosh_2007 Distance Induced Neighborhood Search

# Distance Induced Neighborhood Search

## General description

Distance Induced Neighborhood Search (`DinsMatheuristic`) is the public scientific identity associated with
Ghosh (2007), *DINS, a MIP Improvement Heuristic*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The distance-induced neighborhood principle is preserved; soft-fixing details are represented through generic fixings/bounds.

## Technical specifications

- Stable ID: `dins-ghosh-2007`
- Class: `DinsMatheuristicOptimizer`
- Parameters: `DinsMatheuristicParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.159.0
- Primary DOI/permanent identifier: `10.1007/978-3-540-72792-7_24`

## Complexity

One relaxation solve and one bounded exact neighborhood solve per iteration.

Space: O(n) incumbent/relaxation state plus exact-solver state.

## Applicability

Generic MIP incumbent improvement using a relaxation-guided distance neighborhood.

## Detailed operation

Builds an exact neighborhood whose distance to the relaxation is bounded by the incumbent-to-relaxation distance, with hard fixings for strong agreements.

## Parameters

`DinsMatheuristicParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new DinsMatheuristicOptimizer().Optimize(
        domain,
        new DinsMatheuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`dins-ghosh-2007`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:x\in X\cap\mathbb Z^p\}
\f]

### Update equations / iterations

\f[
\begin{aligned}D(x,\tilde x)&=\sum_{j\in I}|x_j-\tilde x_j|,\\D(x,\tilde x)&\le D(\bar x,\tilde x).\end{aligned}
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

Ghosh (2007), *DINS, a MIP Improvement Heuristic*, IPCO 2007, LNCS.
DOI/permanent identifier: `10.1007/978-3-540-72792-7_24`.
