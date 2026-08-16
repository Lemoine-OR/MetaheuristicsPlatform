# Reduced Variable Neighborhood Search

## General description

Reduced Variable Neighborhood Search (RVNS) is a literature-established VNS variant that deliberately removes the local-improvement phase. It retains systematic shaking and neighborhood change, making it attractive when a full local search after every perturbation is too expensive.

## Technical specifications

- Stable algorithm ID: `reduced-variable-neighborhood-search`.
- Public optimizer: `ReducedVariableNeighborhoodSearchOptimizer<TSolution>`.
- Reuses `ISolutionPerturbation<TSolution>` and `OptimizationContext<TSolution>`.
- No local-search procedure is invoked.
- Every shaking decision is counted as one common iteration because RVNS has no inner local-search iterations.

## Complexity

For ordered shaking neighborhoods `N_1,...,N_K`, one complete non-improving sweep costs

\[
O\left(\sum_{k=1}^{K}(C_{\mathrm{shake},k}+C_{\mathrm{eval},k})\right).
\]

Space is `O(|solution|)` beyond domain-owned shaking workspace.

## Applicability

RVNS is suitable when meaningful increasingly distant shaking neighborhoods exist and the cost of repeated local improvement would dominate the search budget.

## Detailed operation

1. Generate and evaluate an incumbent.
2. Set `k = 1`.
3. Shake in neighborhood `N_k`.
4. Evaluate the shaken candidate without local descent.
5. If it strictly improves the incumbent, accept it and reset `k = 1`.
6. Otherwise increment `k`.
7. Repeat sweeps until common stopping or `MaximumCycles`.

## Parameters

`ReducedVariableNeighborhoodSearchParameters` exposes `MaximumCycles`. Generic stopping criteria, callbacks and reproducibility options remain provided by the common runtime.

## API example

```csharp
var rvns = new ReducedVariableNeighborhoodSearchOptimizer<MySolution>(
    initialGenerator,
    shakingNeighborhoods);
```

## Stable factory ID

`reduced-variable-neighborhood-search`

The method requires typed composition because shaking neighborhoods depend on the solution representation.

## Mathematical details

### Problem formulation

\[
\min_{x\in X} f(x).
\]

### Update equations / iterations

For a shaken candidate \(x'\in N_k(x)\),

\[
x\leftarrow x',\quad k\leftarrow1
\quad\text{if } f(x')<f(x),
\]

otherwise

\[
k\leftarrow k+1.
\]

No local-improvement step is performed.

### Assumptions

Shaking operators generate candidates compatible with the problem representation and feasibility conventions.

### Convergence conditions

The finite `MaximumCycles` safety cap guarantees termination. RVNS is a metaheuristic and has no universal finite-time guarantee of reaching a global optimum.

### Scientific references

- P. Hansen, N. Mladenovic (2001), *Variable neighborhood search: Principles and applications*, European Journal of Operational Research 130(3), 449-467. DOI: `10.1016/S0377-2217(00)00100-4`.
- P. Hansen, N. Mladenovic, R. Todosijevic, S. Hanafi (2017), *Variable neighborhood search: basics and variants*, EURO Journal on Computational Optimization 5(3), 423-454. DOI: `10.1007/s13675-016-0075-x`.
