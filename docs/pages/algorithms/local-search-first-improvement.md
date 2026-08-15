@page local_search_first_improvement Local Search - First Improvement

# Local Search - First Improvement

Stable ID: `local-search-first-improvement`

## General description

First-descent scan that stops immediately at the first strict improving move.

## Technical specifications

- Stable factory ID: `local-search-first-improvement`
- Implementation class: `FirstImprovementLocalSearchOptimizer<TSolution,TMove,TUndo,TEnumerator>`
- Family: Trajectory-based methods
- Factory mode: typed composition
- Source: `src/MetaheuristicsPlatform/Algorithms/Neighborhood/LocalSearchOptimizers.cs`

## Complexity

- Time: O(q C_delta) per accepted move, where q is the number of candidates scanned until first improvement
- Space: O(|solution|)

## Applicability

Finite ordered neighborhoods with reversible moves

## Detailed operation

First-descent scan that stops immediately at the first strict improving move. The implementation uses the common `OptimizationContext` lifecycle so stopping criteria, callbacks, deterministic random ownership, best-so-far tracking, cancellation and evaluation accounting remain homogeneous with the rest of MetaheuristicsPlatform.

## Parameters

`MaximumAcceptedMoves` bounds the number of accepted improving moves. Generic stopping criteria, callbacks, deterministic seed ownership and cancellation remain common platform services.

## API example

```csharp
MetaheuristicFactory.Register(
    "local-search-first-improvement",
    () => configuredAlgorithm,
    replace: true);
```

## Stable factory ID

The canonical stable ID is `local-search-first-improvement`. This method requires typed composition because its initial-solution generator, neighborhood and reversible move operator are domain components.

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X} f(x)\quad\text{or}\quad\max_{x\in\mathcal X} f(x)
\f]

### Update equations / iterations

For minimization,

\f[
x_{k+1}=\operatorname{first}\{y\in N(x_k):f(y)<f(x_k)\}.
\f]

For maximization, `<` is replaced by `>`.

### Assumptions

Neighborhood enumeration order is meaningful; strict improvement prevents objective cycles.

### Convergence conditions

Terminates at an order-dependent local optimum for finite neighborhoods under strict improvement.

### Scientific references

- Talbi (2009), Metaheuristics: From Design to Implementation. DOI `10.1002/9780470496916`.
