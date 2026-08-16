# Skewed Variable Neighborhood Search - Hansen-Mladenovic

## General description

Skewed Variable Neighborhood Search (SVNS) modifies the neighborhood-change criterion so that the search may recenter on a slightly worse local optimum when that solution is sufficiently far from the current incumbent. This promotes exploration of distant valleys while `OptimizationContext` independently preserves the best original objective observed.

## Technical specifications

- Stable algorithm ID: `skewed-variable-neighborhood-search-hansen-mladenovic-2001`.
- Public optimizer: `SkewedVariableNeighborhoodSearchOptimizer<TSolution>`.
- Requires a domain-owned `ISolutionDistance<TSolution>`.
- Distance values must be finite and non-negative.
- `Alpha = 0` reduces the skewed rule to strict original-objective improvement.
- The implementation supports both minimization and maximization through a sense-consistent skewed comparison.

## Complexity

One complete non-accepting sweep costs

\[
O\left(\sum_{k=1}^{K}
(C_{\mathrm{shake},k}+C_{\mathrm{eval},k}+C_{\mathrm{LS},k}+C_{\rho,k})
\right),
\]

where \(C_{\rho,k}\) is the cost of the supplied solution distance.

## Applicability

SVNS is useful when separated local-optimum valleys can be meaningfully characterized by a problem-specific metric or quasi-metric and diversification toward distant near-quality solutions is desirable.

## Detailed operation

1. Generate and evaluate an incumbent.
2. Shake in `N_k`.
3. Apply the configured reusable local search.
4. Accept any strict original-objective improvement.
5. Otherwise evaluate the skewed distance-based recentering criterion.
6. Accepted strict or skewed moves reset `k = 1`; rejection increments `k`.
7. Best-so-far according to the original objective remains independent of the recentered incumbent.

## Parameters

`SkewedVariableNeighborhoodSearchParameters` exposes `MaximumCycles` and the non-negative skewing factor `Alpha`.

## API example

```csharp
var svns = new SkewedVariableNeighborhoodSearchOptimizer<MySolution>(
    initialGenerator,
    shakingNeighborhoods,
    localSearch,
    solutionDistance);
```

## Stable factory ID

`skewed-variable-neighborhood-search-hansen-mladenovic-2001`

## Mathematical details

### Problem formulation

\[
\min_{x\in X} f(x).
\]

### Update equations / iterations

For minimization, after shaking and local improvement yield \(x''\), SVNS recenters when

\[
f(x'')-\alpha\rho(x,x'') < f(x).
\]

For maximization MetaheuristicsPlatform uses the sense-consistent extension

\[
f(x'')+\alpha\rho(x,x'') > f(x).
\]

The ordinary strict-improvement case is included automatically. Any accepted recentering resets \(k\leftarrow1\).

### Assumptions

The supplied distance \(\rho\) is finite and non-negative. Shaking and local search return candidates valid for the underlying problem.

### Convergence conditions

The finite `MaximumCycles` cap guarantees termination. Because skewed recentering may accept a worse original-objective incumbent, monotone descent does not hold across accepted SVNS moves; global optimality is not guaranteed.

### Scientific references

- P. Hansen, N. Mladenovic (2001), *Variable neighborhood search: Principles and applications*, European Journal of Operational Research 130(3), 449-467. DOI: `10.1016/S0377-2217(00)00100-4`.
- P. Hansen, N. Mladenovic, R. Todosijevic, S. Hanafi (2017), *Variable neighborhood search: basics and variants*, EURO Journal on Computational Optimization 5(3), 423-454. DOI: `10.1007/s13675-016-0075-x`.
