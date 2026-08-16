# Guided Local Search - Voudouris-Tsang

## General description

Guided Local Search (GLS) is a metaheuristic layer over local search. When the current solution is locally optimal under a penalty-augmented objective, GLS identifies costly active solution features that have not already been penalized too often, increments their penalties, and resumes local search on the modified landscape.

MetaheuristicsPlatform v0.26.0 implements the canonical generic GLS mechanism of Tsang and Voudouris / Voudouris and Tsang. The application owns the definition of solution features and their feature costs; the platform owns penalty memory, utility computation, augmented-objective descent, common stopping, callbacks, evaluation accounting and best-so-far management.

## Technical specifications

- Stable algorithm ID: `guided-local-search-voudouris-tsang-1999`.
- Public optimizer: `GuidedLocalSearchOptimizer<TSolution,TMove,TUndo,TMoveEnumerator,TFeature,TFeatureEnumerator>`.
- Allocation-free move enumeration through `IEnumeratedNeighborhood`.
- Allocation-free active-feature enumeration through `IGuidedLocalSearchFeatureModel`.
- Reversible move application through `IReversibleMoveOperator`.
- Optional exact original-objective fast path through `IMoveObjectiveDeltaEvaluator`.
- Optional exact augmented-penalty fast path through `IGuidedLocalSearchPenaltyDeltaEvaluator`.
- Hash-based generic feature penalty memory with optional feature comparer.
- All active features tied for maximum canonical utility are penalized together.
- Original-objective best-so-far remains independently protected by `OptimizationContext`, even when augmented-objective guidance accepts a move that worsens the original objective.

## Complexity

Let `N(x)` be the move neighborhood and `F(x)` the set of active GLS features.

Without exact delta evaluators, one augmented-neighborhood scan costs

\f[
O\!\left(
|N(x)|\,
(C_{\mathrm{move}} + C_f + |F(x)| + C_{\mathrm{undo}})
\right).
\f]

With both exact objective and exact penalty-sum delta evaluators, the scan becomes

\f[
O\!\left(|N(x)|(C_{\Delta f}+C_{\Delta p})\right)
\f]

plus rare move application/cloning when a probed candidate improves the original global best.

At a guided local optimum, feature-utility selection costs

\f[
O(|F(x)|)
\f]

and penalty updates cost `O(q)` for `q` tied maximum-utility features.

Penalty memory is `O(P)`, where `P` is the number of distinct features penalized during the run.

## Applicability

GLS is appropriate when meaningful solution features can be identified and associated with non-negative costs representing undesirable structural contributions. Typical examples include route edges, assignment components, violated soft constraints, expensive setup/configuration choices, or other domain-defined elements.

The feature abstraction is deliberately generic. The library does not assume that a feature is an edge, variable, job, machine, or integer index.

## Detailed operation

1. Generate and evaluate an initial solution with the original objective.
2. Perform local descent using the augmented objective with all penalties initially zero.
3. At an augmented local optimum, enumerate the active solution features.
4. Compute each active feature utility from its cost and current penalty.
5. Penalize every active feature tied for maximum strictly positive utility.
6. Resume local descent on the new augmented objective.
7. Preserve the best solution according to the original problem objective independently from the guided incumbent.
8. Repeat until the common stopping criterion fires or `MaximumPenaltyUpdates` is reached.

## Parameters

`GuidedLocalSearchParameters` exposes:

- `PenaltyWeight`: the regularization / penalty weight `lambda`; default `1.0`;
- `MaximumPenaltyUpdates`: maximum number of feature-penalty updates; default `100`;
- `MaximumAcceptedMovesPerPenaltyPhase`: safety cap on augmented-objective accepted moves between penalty updates;
- `SelectionPolicy`: first-improvement or best-improvement under the augmented objective.

The implementation intentionally does not impose a universal automatic scaling law for `PenaltyWeight`. Appropriate scaling depends on the magnitude of the original objective and the selected feature system.

## API example

```csharp
var gls = new GuidedLocalSearchOptimizer<
    MySolution,
    MyMove,
    MyUndo,
    MyMoveEnumerator,
    MyFeature,
    MyFeatureEnumerator>(
        initialSolutionGenerator,
        neighborhood,
        reversibleMoveOperator,
        featureModel,
        objectiveDeltaEvaluator,
        penaltyDeltaEvaluator,
        moveApplicability);

var result = gls.Optimize(
    problem,
    new GuidedLocalSearchParameters
    {
        PenaltyWeight = 0.2,
        MaximumPenaltyUpdates = 250,
        SelectionPolicy = LocalSearchSelectionPolicy.BestImprovement
    },
    solutionCloner,
    stoppingCriterion,
    options);
```

## Stable factory ID

`guided-local-search-voudouris-tsang-1999`

The method is catalogued as a composition because its move neighborhood, feature system, feature costs and optional delta evaluators are representation dependent.

## Mathematical details

### Problem formulation

For the canonical minimization setting,

\f[
\min_{x\in X} f(x),
\f]

let the application define features `i=1,\ldots,m`, indicator functions `I_i(x)`, non-negative feature costs `c_i(x)`, integer penalties `p_i`, and a penalty weight `\lambda>0`.

The GLS augmented objective is

\f[
h(x)=f(x)+\lambda\sum_{i=1}^{m}p_i I_i(x).
\f]

For a maximization problem, MetaheuristicsPlatform uses the sense-consistent extension

\f[
h(x)=f(x)-\lambda\sum_{i=1}^{m}p_i I_i(x),
\f]

so higher penalties still make active features less attractive.

### Update equations / iterations

At a guided local optimum `x*`, the canonical utility of active feature `i` is

\f[
u_i(x^*)=
I_i(x^*)\frac{c_i(x^*)}{1+p_i}.
\f]

Let

\f[
U_{\max}=\max_i u_i(x^*).
\f]

Every active feature tied at the maximum strictly positive utility is updated by

\f[
p_i \leftarrow p_i+1
\qquad
\text{when }u_i(x^*)=U_{\max}>0.
\f]

Local search then resumes under the modified `h`.

### Assumptions

The feature enumerator must return each active feature at most once for a solution. Feature costs must be finite and non-negative. The optional penalty-delta evaluator must return the exact candidate value of the unscaled penalty sum `sum p_i I_i(x)`.

The original objective and the augmented objective serve different roles: the augmented objective chooses the guided trajectory, whereas the common optimization context records the best original objective observed.

### Convergence conditions

With a finite solution space, a fixed penalty vector and strict augmented-objective acceptance, each local-descent phase terminates at an augmented local optimum. GLS then changes the landscape by updating penalties, so classical monotonic descent arguments do not apply across penalty updates. The finite `MaximumPenaltyUpdates` parameter guarantees finite execution in the library independently of any problem-specific global-convergence assumptions.

GLS is a heuristic/metaheuristic framework and does not provide a universal finite-time guarantee of reaching a global optimum.

### Scientific references

- E. Tsang, C. Voudouris (1997), *Fast local search and guided local search and their application to British Telecom's workforce scheduling problem*, Operations Research Letters 20(3), 119-127. DOI: `10.1016/S0167-6377(96)00042-9`.
- C. Voudouris, E. Tsang (1999), *Guided local search and its application to the traveling salesman problem*, European Journal of Operational Research 113(2), 469-499. DOI: `10.1016/S0377-2217(98)00099-X`.
