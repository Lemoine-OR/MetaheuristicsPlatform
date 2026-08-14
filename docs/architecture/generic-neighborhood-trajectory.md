# Generic Neighborhood & Trajectory Foundation

## Purpose

The population-based PSO/DE branch validates high-performance continuous population
search.

Trajectory algorithms stress a different axis of genericity:

```text
one current solution
        |
        v
sample / enumerate move
        |
        v
evaluate transition
        |
        v
accept / reject
        |
        v
next trajectory state
```

The foundation is solution-type agnostic.

`TSolution` may be:
- `double[]`;
- a value-type vector;
- a permutation;
- a bitset;
- an integer assignment;
- a scheduling solution;
- an MLLP production-planning solution;
- a decoded domain object.

`TMove` may be a tiny readonly struct such as:
- swap `(i,j)`;
- insertion `(from,to)`;
- bit flip `i`;
- quantity change `(item,period,delta)`;
- setup shift;
- routing reassignment.

## Stochastic neighborhoods

```csharp
IStochasticNeighborhood<TSolution,TMove>
```

samples one move through the platform deterministic `IRandomSource`.

This is the natural primitive for:
- Simulated Annealing;
- stochastic local search;
- ILS perturbation;
- random VNS shaking.

## Enumerated neighborhoods

```csharp
IEnumeratedNeighborhood<TSolution,TMove,TEnumerator>
```

returns a `struct` cursor implementing:

```csharp
INeighborhoodEnumerator<TMove>
```

The contract intentionally avoids `IEnumerable<TMove>` in hot loops.

This is useful for:
- best improvement;
- first improvement;
- Tabu Search candidate scans;
- VND neighborhood descent.

## Move application

The common application contract is:

```csharp
void Apply(
    ref TSolution solution,
    in TMove move);
```

`ref TSolution` supports:
- mutable classes;
- immutable classes replaced by a new instance;
- mutable structs;
- immutable record structs.

## Reversible fast path

A reversible operator adds:

```csharp
TUndo CaptureUndo(
    in TSolution solution,
    in TMove move);

void Undo(
    ref TSolution solution,
    in TMove move,
    in TUndo undo);
```

The undo token can itself be a small struct.

For a swap move, for example, the undo token may contain only the values that need to be
restored.

The trajectory engine does not need to clone the complete solution.

## Exact differential objective evaluation

```csharp
IMoveObjectiveDeltaEvaluator<TSolution,TMove>
```

may compute the exact candidate objective without applying the move.

This enables:

```text
sample move
   |
delta objective
   |
acceptance
   |---------------- rejected -> nothing mutated
   |
 accepted
   |
apply once
```

For expensive or large combinatorial solutions this is the preferred execution path.

Examples include:
- TSP edge-exchange deltas;
- scheduling move deltas;
- setup/inventory cost changes;
- local MLLP move deltas when analytically available.

## Reversible full-evaluation fallback

If no exact delta evaluator exists:

```text
capture undo
apply
full evaluate
accept?
   |
   +-- yes -> keep
   |
   +-- no  -> undo
```

If full evaluation or acceptance throws, the executor attempts to undo before propagating
the exception.

## Clone-based fallback

Some solution transformations are not naturally reversible.

`ClonedTrajectoryStepExecutor` supports:

```text
clone
apply on candidate
evaluate
accept -> replace current
reject -> discard candidate
```

With an exact delta evaluator, rejected moves are filtered before cloning.

## Acceptance policies

Acceptance is independent through:

```csharp
ITrajectoryAcceptancePolicy
```

The context exposes:
- optimization sense;
- trajectory iteration;
- current objective;
- candidate objective;
- global/best objective;
- transition quality.

The foundation includes only `GreedyAcceptancePolicy`.

It deliberately does not embed:
- temperature;
- tabu tenure;
- aspiration;
- threshold accepting;
- late acceptance;
- record-to-record travel.

Those belong to algorithm-specific policies layered on this contract.

## Statistics

`TrajectoryStatisticsAccumulator` is a mutable value type that records:
- attempts;
- accepts/rejects;
- improving/equal/worsening transitions;
- delta versus full evaluations;
- applied moves;
- undone moves;
- acceptance ratio.

Algorithms choose whether to collect these statistics.

## Intended Simulated Annealing composition

v0.18.0 will build:

```text
IStochasticNeighborhood
        |
        v
TMove
        |
        v
ReversibleTrajectoryStepExecutor
        |
        +-- exact delta evaluator (when available)
        |
        +-- full objective fallback
        |
        v
MetropolisAcceptancePolicy
        |
        v
CoolingSchedule
        |
        v
OptimizationContext / stopping / callbacks
```

## Scientific context for v0.18

The acceptance mechanism for Simulated Annealing traces to:

Nicholas Metropolis, Arianna W. Rosenbluth, Marshall N. Rosenbluth,
Augusta H. Teller, Edward Teller,
"Equation of State Calculations by Fast Computing Machines",
Journal of Chemical Physics 21, 1087-1092, 1953.
DOI: 10.1063/1.1699114.

The optimization framework follows:

Scott Kirkpatrick, C. Daniel Gelatt Jr., Mario P. Vecchi,
"Optimization by Simulated Annealing",
Science 220(4598), 671-680, 1983.
DOI: 10.1126/science.220.4598.671.

v0.17.0 only establishes the reusable trajectory substrate and does not yet implement
a cooling schedule or Metropolis acceptance.