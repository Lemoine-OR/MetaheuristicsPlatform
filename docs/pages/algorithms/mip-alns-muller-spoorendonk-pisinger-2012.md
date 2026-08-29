@page mip_alns_muller_spoorendonk_pisinger_2012 MIP-based Adaptive Large Neighborhood Search

# MIP-based Adaptive Large Neighborhood Search

## General description

MIP-based Adaptive Large Neighborhood Search (`MipAdaptiveLargeNeighborhoodSearch`) is the public scientific identity associated with
Muller, Spoorendonk & Pisinger (2012), *A hybrid adaptive large neighborhood search heuristic for lot-sizing with setup times*. It uses mathematical-programming
relaxation and/or exact restricted optimization through the native exact-repair domain contract.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The paper's MIP-based exact repair inside adaptive LNS is preserved; the lot-sizing-specific neighborhoods are replaced by a generic variable-destroy contract.

## Technical specifications

- Stable ID: `mip-alns-muller-spoorendonk-pisinger-2012`
- Class: `MipAdaptiveLargeNeighborhoodSearchOptimizer`
- Parameters: `MipAdaptiveLargeNeighborhoodSearchParameters`
- Family: Matheuristics and exact-repair integration
- Domain contract: `IExactRepairMatheuristicDomain`
- Restricted-solver request: `ExactRepairRequest`
- Result: `MatheuristicOptimizationResult`
- Public since: v0.161.0
- Primary DOI/permanent identifier: `10.1016/j.ejor.2011.11.036`

## Complexity

One exact repair subproblem per ALNS iteration plus O(n) destroy/fix construction.

Space: O(n) incumbent/destroy mask plus exact-solver state.

## Applicability

Combinatorial/MIP problems where exact reoptimization is an effective repair mechanism for large destroyed neighborhoods.

## Detailed operation

Destroys a variable subset, fixes the complement to the incumbent and invokes the exact solver as a large-neighborhood repair operator with adaptive destroy size.

## Parameters

`MipAdaptiveLargeNeighborhoodSearchParameters` validates the scientific control parameters and exact-solver resource limits used by this mechanism.

## API example

```csharp
IExactRepairMatheuristicDomain domain = GetDomain();

var result =
    new MipAdaptiveLargeNeighborhoodSearchOptimizer().Optimize(
        domain,
        new MipAdaptiveLargeNeighborhoodSearchParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`mip-alns-muller-spoorendonk-pisinger-2012`

## Mathematical details

### Problem formulation

\f[
\min\{c^\top x:x\in X\cap\mathbb Z^p\}
\f]

### Update equations / iterations

\f[
\begin{aligned}U_t&=\operatorname{destroy}(x^t),\\x^{t+1}&\in\arg\min\{c^\top x:x\in X,\ x_j=x_j^t\ \forall j\notin U_t\}.\end{aligned}
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

Muller, Spoorendonk & Pisinger (2012), *A hybrid adaptive large neighborhood search heuristic for lot-sizing with setup times*, European Journal of Operational Research 218(3), 614-623.
DOI/permanent identifier: `10.1016/j.ejor.2011.11.036`.
