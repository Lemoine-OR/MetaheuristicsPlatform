@page record_to_record_travel_dueck_1993 Record-to-Record Travel - Dueck 1993

# Record-to-Record Travel - Dueck 1993

## General description

Record-to-Record Travel (RRT) was introduced by Dueck (1993) with Great Deluge. A
candidate may be worse than the current solution as long as it remains within a fixed
deviation of the best accepted record.

@subpage acceptance_based_trajectory_methods

## Technical specifications

- Stable ID: `record-to-record-travel-dueck-1993`.
- Optimizer: `RecordToRecordTravelOptimizer<TSolution,TMove,TUndo>`.
- Policy: `RecordToRecordTravelAcceptancePolicy`.
- Single method-specific scalar: non-negative absolute `Deviation`.
- Record: best accepted/visited objective.
- Generic stochastic neighborhood, reversible move operator and optional exact delta.
- Common `OptimizationContext<TSolution>` lifecycle.

## Complexity

Acceptance is O(1). Exact-delta rejection is O(C_delta); accepted moves cost
O(C_delta+C_move). Full reversible evaluation adds C_eval and, for rejection, C_undo.
Acceptance state is O(1).

## Applicability

Any representation admitting a stochastic neighborhood and reversible moves. The
canonical deviation is absolute and therefore depends on objective scale.

## Detailed operation

The initial solution is the first record. In minimization, a candidate is accepted when
`candidate <= record + Deviation`. The record changes only when an accepted visited
candidate improves it. A rejected candidate is merely an evaluated probe.

Maximization uses the mirrored sense-aware deviation.

## Parameters

- `Deviation` — non-negative absolute deviation from the record; default `1.0`.
- `MaximumConsecutiveSamplingFailures` — default `64`.
- At `Deviation = 0`, only record-nonworsening candidates are accepted.
- Generic stopping/callback/cancellation controls remain independent.

## API example

```csharp
var algorithm =
    new RecordToRecordTravelOptimizer<MySolution, MyMove, MyUndo>(
        initialSolutionGenerator,
        stochasticNeighborhood,
        reversibleMoveOperator,
        exactDeltaEvaluator);

var result =
    algorithm.Optimize(
        problem,
        new RecordToRecordTravelParameters { Deviation = 5.0 },
        solutionCloner,
        stoppingCriterion);
```

## Stable factory ID

`record-to-record-travel-dueck-1993`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X}f(x)\qquad\text{or}\qquad\max_{x\in\mathcal X}f(x).
\f]

### Update equations / iterations

For minimization,

\f[
\begin{aligned}
r_k&=\min_{0\le j\le k}f(x_j),\\
x_{k+1}&=
\begin{cases}
x'_k,&f(x'_k)-r_k\le D,\\
x_k,&f(x'_k)-r_k>D,
\end{cases}\\
r_{k+1}&=\min\{r_k,f(x_{k+1})\},\qquad D\ge0.
\end{aligned}
\f]

### Assumptions

Candidate and record objectives are finite; deviation is non-negative and absolute;
reversible moves and optional exact deltas are exact; the record is updated only from
accepted visited states.

### Convergence conditions

With a positive fixed deviation, RRT may continue inside the record band indefinitely.
No universal finite-time global convergence claim is made. Termination uses the common
stopping contract.

### Scientific references

- Dueck, G. (1993), *New Optimization Heuristics: The Great Deluge Algorithm and the
  Record-to-Record Travel*, DOI `10.1006/jcph.1993.1010`.