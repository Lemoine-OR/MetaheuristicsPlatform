# General Variable Neighborhood Search

## General description

General Variable Neighborhood Search (GVNS) combines the VNS shaking phase with Variable Neighborhood Descent (VND) as its improvement phase. MetaheuristicsPlatform composes the existing reusable `VariableNeighborhoodDescentProcedure<TSolution>` rather than introducing a second VND engine.

## Technical specifications

- Stable algorithm ID: `general-variable-neighborhood-search`.
- Public optimizer: `GeneralVariableNeighborhoodSearchOptimizer<TSolution>`.
- Ordered shaking through `ISolutionPerturbation<TSolution>`.
- Ordered local improvement through the existing VND procedure.
- Strict original-objective improvement resets the shaking neighborhood to the first one.

## Complexity

A complete non-improving shaking sweep costs

\f[
O\left(\sum_{k=1}^{K}
(C_{\mathrm{shake},k}+C_{\mathrm{eval},k}+C_{\mathrm{VND},k})
\right).
\f]

Space is `O(|solution| + VND workspace)`.

## Applicability

GVNS is appropriate when both diversification neighborhoods and several complementary local-search neighborhoods can be defined on the same representation.

## Detailed operation

1. Generate and evaluate an incumbent.
2. Shake in `N_k`.
3. Apply VND to the shaken solution.
4. If the VND local optimum strictly improves the incumbent, accept it and reset `k = 1`.
5. Otherwise increment `k`.
6. Repeat until common stopping or `MaximumCycles`.

## Parameters

`GeneralVariableNeighborhoodSearchParameters` exposes `MaximumCycles` and `MaximumNeighborhoodRestarts`, the latter being passed directly to the reusable VND procedure.

## API example

```csharp
var gvns = new GeneralVariableNeighborhoodSearchOptimizer<MySolution>(
    initialGenerator,
    shakingNeighborhoods,
    localSearchNeighborhoods);
```

## Stable factory ID

`general-variable-neighborhood-search`

## Mathematical details

### Problem formulation

\f[
\min_{x\in X} f(x).
\f]

### Update equations / iterations

After shaking \f$x'\in N_k(x)\f$, let

\f[
x''=\mathrm{VND}(x').
\f]

Then

\f[
x\leftarrow x'',\quad k\leftarrow1
\quad\text{if } f(x'')<f(x),
\f]

otherwise \f$k\leftarrow k+1\f$.

Within VND, every strict local improvement restarts the local-neighborhood sequence at its first neighborhood.

### Assumptions

Shaking and VND neighborhoods operate on a compatible solution representation and respect the problem's optimization sense.

### Convergence conditions

Each VND phase is bounded by its restart safety cap and the outer search by `MaximumCycles`; therefore the library run terminates under finite configured caps. No universal global-optimum guarantee is implied.

### Scientific references

- P. Hansen, N. Mladenovic (2001), *Variable neighborhood search: Principles and applications*, European Journal of Operational Research 130(3), 449-467. DOI: `10.1016/S0377-2217(00)00100-4`.
- P. Hansen, N. Mladenovic, R. Todosijevic, S. Hanafi (2017), *Variable neighborhood search: basics and variants*, EURO Journal on Computational Optimization 5(3), 423-454. DOI: `10.1007/s13675-016-0075-x`.
