# Simulated Annealing

## Scientific references

Nicholas Metropolis, Arianna W. Rosenbluth, Marshall N. Rosenbluth,
Augusta H. Teller, Edward Teller,
"Equation of State Calculations by Fast Computing Machines",
Journal of Chemical Physics 21(6), 1087-1092, 1953.
DOI: 10.1063/1.1699114.

Scott Kirkpatrick, C. Daniel Gelatt Jr., Mario P. Vecchi,
"Optimization by Simulated Annealing",
Science 220(4598), 671-680, 1983.
DOI: 10.1126/science.220.4598.671.

Miranda Lundy, Alistair Mees,
"Convergence of an annealing algorithm",
Mathematical Programming 34(1), 111-124, 1986.
DOI: 10.1007/BF01582166.

## Generic composition

The optimizer is generic over:

```text
TSolution
TMove
TUndo
```

and composes:

```text
initial-solution generator
        |
        v
stochastic neighborhood
        |
        v
TMove
        |
        v
reversible trajectory executor
        |
        +-- exact delta objective, when available
        |
        +-- full objective + undo fallback
        |
        v
Metropolis acceptance
        |
        v
cooling schedule
        |
        v
OptimizationContext
        |
        +-- generic stopping
        +-- callbacks
        +-- best-so-far
        +-- reproducible RNG
        +-- common OptimizationResult
```

## Metropolis acceptance

For an improving or equal transition:

```text
accept = true
```

For a worsening transition, define positive degradation:

```text
minimization:
delta = candidate - current

maximization:
delta = current - candidate
```

Then:

```text
P(accept) = exp(-delta / T)
```

The implementation does not consume a random number for improving/equal moves.

## Exact temperature inversion

If a representative worsening cost `delta` should be accepted initially with probability
`p0`, the exact Metropolis inversion is:

```text
T0 = -delta / ln(p0)
```

The helper:

```csharp
SimulatedAnnealingTemperature
    .FromWorseningAcceptanceProbability(...)
```

provides this conversion.

A future calibration layer can estimate a representative degradation by sampling the
problem neighborhood without changing the SA engine.

## Cooling schedules

### Geometric

```text
T_next = alpha * T
```

with:

```text
0 < alpha < 1
```

The platform default is `alpha = 0.95`.

This is the practical default, not claimed as a universal optimum.

### Lundy-Mees

```text
T_next =
    T
    -----------
    1 + beta*T
```

This schedule is exposed as a separate published option.

## Temperature levels

`TransitionsPerTemperatureLevel` controls how many attempted moves are made before
cooling.

The default is:

```text
100
```

This is an engineering default and remains configurable because neighborhood scale and
problem dimension differ greatly.

## Fast delta path

With:

```csharp
IMoveObjectiveDeltaEvaluator<TSolution,TMove>
```

the sequence is:

```text
sample move
candidate objective from delta
Metropolis decision
    |
    +-- reject -> do not mutate
    |
    +-- accept -> Apply exactly once
```

This is the preferred path for large combinatorial solutions.

## Reversible full-evaluation path

Without an exact delta evaluator:

```text
CaptureUndo
Apply
problem.Evaluate
Metropolis
    |
    +-- accept -> keep
    |
    +-- reject -> Undo
```

v0.17 guarantees exception-safe undo.

## Common lifecycle

`SimulatedAnnealingOptimizer<TSolution,TMove,TUndo>` implements:

```csharp
IMetaheuristic<TSolution,SimulatedAnnealingParameters>
```

Therefore SA uses the same:
- `OptimizationContext`;
- `IStoppingCriterion`;
- `OptimizationOptions`;
- deterministic random factory;
- callbacks;
- best-solution snapshots;
- `OptimizationResult<TSolution>`;

as the population algorithms.

## Algorithm-specific stopping

The platform keeps generic stopping criteria, but SA may additionally stop on:

```text
MinimumTemperature
NeighborhoodExhausted
```

Both are explicit `StoppingDecision` reasons.

`StopAtMinimumTemperature` can be disabled when the experiment should be governed only
by a generic budget such as maximum iterations, evaluations, wall time or stagnation.

## Evaluation accounting

The initial solution is evaluated through `OptimizationContext.Evaluate`.

Move candidate objectives are computed by either:
- an exact delta evaluator; or
- a direct full problem evaluation in the reversible executor.

They are then registered exactly once with `OptimizationContext` through the external
evaluation registration API.

When a candidate improves the global best, Metropolis necessarily accepts it, so the
current accepted solution can be cloned as the owned best snapshot.

## Performance

The benchmark suite compares:
- a full O(D) sphere reevaluation followed by undo;
- an exact O(1) component-delta evaluation that rejects without mutation.

This benchmark is intended to quantify the benefit of problem-specific differential
evaluation rather than to claim a universal speedup.

## Next trajectory algorithms

The same v0.17/v0.18 substrate is now ready for:

```text
Tabu Search
VND / VNS
Iterated Local Search
memetic local improvement
hybrid DE/PSO + local search
```