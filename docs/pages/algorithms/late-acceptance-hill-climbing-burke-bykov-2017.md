@page late_acceptance_hill_climbing_burke_bykov_2017 Late Acceptance Hill Climbing - Burke-Bykov 2017

# Late Acceptance Hill Climbing - Burke-Bykov 2017

## General description

Late Acceptance Hill Climbing (LAHC) is a one-point local-search metaheuristic whose
acceptance reference is taken from the history of the search rather than from a cooling
schedule. The platform implements the final Burke-Bykov formulation published in 2017,
not merely the earlier 2008 prototype.

@subpage acceptance_based_trajectory_methods

## Technical specifications

- Stable ID: `late-acceptance-hill-climbing-burke-bykov-2017`.
- Optimizer: `LateAcceptanceHillClimbingOptimizer<TSolution,TMove,TUndo>`.
- Policy: `LateAcceptancePolicy`.
- Single scientific control parameter: positive history length `HistoryLength`.
- Circular objective-history buffer; no stored solutions.
- Generic stochastic neighborhood, reversible move operator and optional exact delta.
- Common `OptimizationContext<TSolution>` lifecycle and visited-state accounting.

## Complexity

Acceptance and history update are O(1) per attempted transition. Exact-delta rejection is
O(C_delta); an accepted exact-delta transition costs O(C_delta+C_move). Full reversible
evaluation adds C_eval and, on rejection, C_undo. The LAHC-specific memory is O(L), where
L is the history length.

## Applicability

Any solution representation admitting a stochastic neighborhood and reversible moves.
Unlike temperature-, threshold- or water-level methods, the acceptance reference uses
objective values previously observed by the same search and is therefore invariant under
strictly increasing affine rescaling of the objective.

## Detailed operation

Let the circular history contain L objective values and let v=k mod L. In the final
Burke-Bykov formulation for minimization, a candidate is accepted when it either strictly
improves the active history value or is not worse than the current solution. After the
accept/reject decision, the active history value is replaced only if the resulting current
objective strictly improves it. The index is then advanced.

This detail matters: rejected candidates can still cause the active history entry to be
improved by the unchanged incumbent, while a worse value is never written back into the
history. Maximization uses the exact mirrored quality relation.

## Parameters

- `HistoryLength` — positive circular history length; library default `100`.
- `MaximumConsecutiveSamplingFailures` — default `64`.
- `HistoryLength = 1` reduces the acceptance behavior to ordinary non-worsening
  hill climbing.
- The default history length is a library convenience, not a universal value prescribed
  by Burke and Bykov.
- Generic stopping, callback, seed and cancellation controls remain independent.

## API example

```csharp
var algorithm =
    new LateAcceptanceHillClimbingOptimizer<MySolution, MyMove, MyUndo>(
        initialSolutionGenerator,
        stochasticNeighborhood,
        reversibleMoveOperator,
        exactDeltaEvaluator);

var result =
    algorithm.Optimize(
        problem,
        new LateAcceptanceParameters { HistoryLength = 500 },
        solutionCloner,
        stoppingCriterion);
```

## Stable factory ID

`late-acceptance-hill-climbing-burke-bykov-2017`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X} f(x)\qquad\text{or}\qquad
\max_{x\in\mathcal X} f(x).
\f]

### Update equations / iterations

For minimization, with a history array H of length L,

\f[
\begin{aligned}
v_k&=k\bmod L,\\
x_{k+1}&=
\begin{cases}
x'_k,& f(x'_k)<H_{v_k}\ \lor\ f(x'_k)\le f(x_k),\\
x_k,&\text{otherwise},
\end{cases}\\
H_{v_k}&\leftarrow \min\!\left\{H_{v_k},f(x_{k+1})\right\}.
\end{aligned}
\f]

For maximization, `<`, `<=` and `min` are mirrored by `>`, `>=` and `max`.

### Assumptions

Objective comparisons are exact for the represented `double` values; optional move deltas
must agree with full objective evaluation; history length is strictly positive; the
stochastic neighborhood and reversible operator define valid transitions.

### Convergence conditions

LAHC is a stochastic heuristic and the library makes no universal finite-time global
convergence claim. With L=1 it reduces to non-worsening hill climbing. For larger L,
historical acceptance can admit temporary deterioration while the retained history values
improve monotonically in objective quality.

### Scientific references

- Burke, E. K.; Bykov, Y. (2008), *A Late Acceptance Strategy in Hill-Climbing for
  Exam Timetabling Problems*, PATAT 2008.
- Burke, E. K.; Bykov, Y. (2017), *The late acceptance Hill-Climbing heuristic*,
  European Journal of Operational Research 258(1), 70-78.
  DOI `10.1016/j.ejor.2016.07.012`.
