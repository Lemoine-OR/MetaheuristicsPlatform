# Multi-Start Local Search

## General description

Multi-Start Local Search (MSLS) repeatedly generates an independent starting solution, applies a reusable local-search procedure, and retains the best solution observed over all starts. The implementation deliberately composes the v0.23 local-search procedure instead of duplicating neighborhood scanning or move evaluation.

## Technical specifications

- Stable algorithm ID: `multi-start-local-search`.
- Public class: `MultiStartLocalSearchOptimizer<TSolution>`.
- Model: single-solution / restart trajectory method.
- Composition requirements: an `INeighborhoodSearchInitialSolutionGenerator<TSolution>` and an `ILocalSearchProcedure<TSolution>`.
- Randomness: owned by the common `OptimizationContext`; deterministic replay is available when the configured generator consumes the seeded platform RNG deterministically.
- Best-so-far, callbacks, cancellation and stopping use the common runtime lifecycle.

## Complexity

For `S` starts, the total cost is

\f[
O\!\left(\sum_{s=1}^{S}(C_{\mathrm{init},s}+C_{\mathrm{LS},s})\right),
\f]

or `O(S(C_init + C_LS))` under homogeneous costs. Memory is the solution plus the workspace required by the composed local search; the algorithm does not retain all local optima.

## Applicability

MSLS is applicable whenever the problem can generate multiple feasible starting solutions and a compatible local-search procedure exists. It is particularly useful on multimodal landscapes where different starts can enter different attraction basins.

## Detailed operation

1. Generate a start with the platform-owned random source.
2. Evaluate it through `OptimizationContext`.
3. Apply the injected local-search procedure.
4. Keep best-so-far automatically through the common context.
5. Repeat until a common stopping criterion fires or `MaximumStarts` is reached.

The implementation is sequential in v0.24.0 so that callback ordering, deterministic random consumption, mutable local-search components and global evaluation accounting remain unambiguous.

## Parameters

`MultiStartLocalSearchParameters.MaximumStarts` is a finite restart safety cap (default 32). Common time, evaluation, target, stagnation and cancellation criteria remain available through the platform stopping infrastructure. The default is not presented as a problem-independent optimal restart count.

## API example

```csharp
var descent = new MoveLocalSearchProcedure<MySolution, MyMove, MyUndo, MyEnumerator>(
    neighborhood,
    moveOperator,
    LocalSearchSelectionPolicy.BestImprovement,
    deltaEvaluator);

var msls = new MultiStartLocalSearchOptimizer<MySolution>(
    initialSolutionGenerator,
    descent);

var result = msls.Optimize(
    problem,
    new MultiStartLocalSearchParameters { MaximumStarts = 64 },
    solutionCloner,
    stoppingCriterion,
    options);
```

## Stable factory ID

`multi-start-local-search`

Because the method requires domain-owned generation and local-search components, the catalog marks it as a composition algorithm.

## Mathematical details

### Problem formulation

For an optimization problem `best_{x in X} f(x)`, let `G_s` generate start `s` and `L` denote the local-search mapping. MSLS returns

\f[
x^{\star}=\operatorname{best}_{s=1,\ldots,S}\;L(G_s()).
\f]

### Update equations / iterations

\f[
x_s^{(0)}=G_s(),\qquad
x_s^{\mathrm{LS}}=L(x_s^{(0)}),\qquad
x^{\star}_s=\operatorname{best}(x^{\star}_{s-1},x_s^{\mathrm{LS}}).
\f]

### Assumptions

The initial generator must return valid candidates for the problem representation, and the injected local search must respect the `ILocalSearchProcedure<TSolution>` contract. Independence of starts is a modeling property of the generator; the platform does not falsely claim statistical independence when a user-supplied generator is deterministic or state-coupled.

### Convergence conditions

No universal finite-start global-optimality guarantee is claimed. If independent starts have a fixed positive probability `p>0` of entering a basin whose local search reaches a global optimum, the probability of missing that basin after `S` starts is `(1-p)^S`, which tends to zero as `S` tends to infinity. This conditional statement does not hold without the corresponding coverage and independence assumptions.

### Scientific references

- R. Martí (2003), *Multi-Start Methods*, in *Handbook of Metaheuristics*, pp. 355-368. DOI: `10.1007/0-306-48056-5_12`.
- E.-G. Talbi (2009), *Metaheuristics: From Design to Implementation*, Wiley. DOI: `10.1002/9780470496916`.
