@page local_search_best_improvement Local Search - Best Improvement

# Local Search - Best Improvement

Stable ID: `local-search-best-improvement`

## General description

Steepest-descent best-improvement scan with allocation-free neighborhood cursor, exact-delta fast path and reversible fallback.

## Technical specifications

- Stable factory ID: `local-search-best-improvement`
- Implementation class: `BestImprovementLocalSearchOptimizer<TSolution,TMove,TUndo,TEnumerator>`
- Family: Trajectory-based methods
- Factory mode: typed composition
- Source: `src/MetaheuristicsPlatform/Algorithms/Neighborhood/LocalSearchOptimizers.cs`

## Complexity

- Time: O(|N(x)| C_delta) per descent step with exact deltas; reversible full evaluation otherwise
- Space: O(|solution|)

## Applicability

Finite enumerated neighborhoods with reversible moves

## Detailed operation

Steepest-descent best-improvement scan with allocation-free neighborhood cursor, exact-delta fast path and reversible fallback. The implementation uses the common `OptimizationContext` lifecycle so stopping criteria, callbacks, deterministic random ownership, best-so-far tracking, cancellation and evaluation accounting remain homogeneous with the rest of MetaheuristicsPlatform.

## Parameters

`MaximumAcceptedMoves` bounds the number of accepted improving moves. Generic stopping criteria, callbacks, deterministic seed ownership and cancellation remain common platform services.

## API example

```csharp
MetaheuristicFactory.Register(
    "local-search-best-improvement",
    () => configuredAlgorithm,
    replace: true);
```

## Stable factory ID

The canonical stable ID is `local-search-best-improvement`. This method requires typed composition because its initial-solution generator, neighborhood and reversible move operator are domain components.

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X} f(x)\quad\text{or}\quad\max_{x\in\mathcal X} f(x)
\f]

### Update equations / iterations

For minimization,

\f[
x_{k+1}\in\arg\min\{f(y):y\in N(x_k),\ f(y)<f(x_k)\}.
\f]

For maximization, `min` and `<` are replaced by `max` and `>`.

### Assumptions

Finite neighborhood; exact delta evaluator, when supplied, must match full objective evaluation.

### Convergence conditions

Terminates at a local optimum under finite neighborhoods and strict improvement, absent earlier generic stopping.

### Scientific references

- Talbi (2009), Metaheuristics: From Design to Implementation. DOI `10.1002/9780470496916`.
