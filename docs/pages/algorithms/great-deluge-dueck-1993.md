@page great_deluge_dueck_1993 Great Deluge Algorithm - Dueck 1993

# Great Deluge Algorithm - Dueck 1993

## General description

Great Deluge (GDA) is the absolute-boundary trajectory heuristic introduced by Dueck
(1993). A candidate is compared with a moving water level rather than with the current
objective degradation.

@subpage acceptance_based_trajectory_methods

## Technical specifications

- Stable ID: `great-deluge-dueck-1993`.
- Optimizer: `GreatDelugeOptimizer<TSolution,TMove,TUndo>`.
- Policy: `GreatDelugeAcceptancePolicy`.
- Initial water level: initial objective.
- Single method-specific scalar: positive `RainSpeed`.
- Generic stochastic neighborhood and reversible move operator.
- Optional exact candidate-objective delta fast path.
- Common `OptimizationContext<TSolution>` lifecycle.

## Complexity

Acceptance is O(1). With exact deltas, rejection is O(C_delta) and acceptance is
O(C_delta+C_move). Reversible full evaluation is O(C_move+C_eval) when accepted and
O(C_move+C_eval+C_undo) when rejected. Acceptance state is O(1).

## Applicability

Any representation admitting a stochastic neighborhood and reversible moves. Because the
level and rain speed are absolute objective values, objective scaling changes the natural
parameter scale.

## Detailed operation

The initial objective becomes the initial water level. For minimization a candidate is
accepted exactly when its objective is no greater than the level. After every attempted
transition the level decreases by `RainSpeed`; maximization mirrors both directions.

The classical Dueck rule is preserved even when the level has advanced beyond the current
solution. Thus a candidate improving the current solution can still be rejected if it
fails the absolute level. That is the scientific distinction from Extended Great Deluge.

Every candidate consumes an evaluation, but only accepted visited states can update the
best-so-far snapshot.

## Parameters

- `RainSpeed` — positive absolute level change per attempted transition; default `0.01`.
- `MaximumConsecutiveSamplingFailures` — default `64`.
- Generic iteration/evaluation/time/no-improvement/target/cancellation controls remain
  independent.

## API example

```csharp
var algorithm =
    new GreatDelugeOptimizer<MySolution, MyMove, MyUndo>(
        initialSolutionGenerator,
        stochasticNeighborhood,
        reversibleMoveOperator,
        exactDeltaEvaluator);

var result =
    algorithm.Optimize(
        problem,
        new GreatDelugeParameters { RainSpeed = 0.05 },
        solutionCloner,
        stoppingCriterion);
```

## Stable factory ID

`great-deluge-dueck-1993`

## Mathematical details

### Problem formulation

\f[
\min_{x\in\mathcal X}f(x)\qquad\text{or}\qquad\max_{x\in\mathcal X}f(x).
\f]

### Update equations / iterations

For minimization,

\f[
\begin{aligned}
B_0&=f(x_0),\\
x_{k+1}&=
\begin{cases}
x'_k,&f(x'_k)\le B_k,\\
x_k,&f(x'_k)>B_k,
\end{cases}\\
B_{k+1}&=B_k-\delta_B,\qquad\delta_B>0.
\end{aligned}
\f]

Maximization reverses the inequalities and increases the level.

### Assumptions

Objectives and levels are finite; rain speed is positive and expressed in objective
units; reversible moves and optional exact deltas are exact; best-so-far ownership is
restricted to accepted visited states.

### Convergence conditions

No unconditional global-optimality guarantee is claimed. A positive linear rain speed
progressively tightens the classical acceptance boundary. Practical finite termination
uses the platform's generic stopping contract.

### Scientific references

- Dueck, G. (1993), DOI `10.1006/jcph.1993.1010`.
- Burke, Bykov, Newall & Petrovic (2003), DOI `10.2298/YJOR0302139B`.
- Burke & Bykov (2016), DOI `10.1287/ijoc.2015.0680`.