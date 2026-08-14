# Generic architecture

## Core rule

A metaheuristic is an algorithm-specific search engine executed inside a common optimization lifecycle.

The Core owns:
- optimization sense;
- evaluation counting;
- iteration counting;
- elapsed time;
- best-so-far tracking;
- improvement statistics;
- deterministic random source creation;
- standardized callbacks;
- stopping evaluation;
- final run result.

The algorithm owns:
- its search state;
- its operators;
- its representation-specific logic;
- its strongly typed algorithm-specific parameters;
- optional algorithm-specific stopping state.

## Configuration layers

Two layers are deliberately separated.

### Generic runtime options

`OptimizationOptions`:
- seed;
- random-source factory;
- callback events;
- callback frequency.

### Algorithm-specific parameters

Every parameter object implements `IMetaheuristicParameters`.

The typed algorithm contract is:

`IMetaheuristic<TSolution, TParameters>`

This prevents untyped bags such as `params object[]` and keeps PSO, GA, SA,
Tabu Search and future methods independently configurable.

## No rigid taxonomy inheritance

Classification is represented by `MetaheuristicDescriptor`.
A descriptor may carry several families and mechanisms simultaneously.

Classification is metadata and does not force a class hierarchy.

## Common stopping contract

Every stopping criterion implements `IStoppingCriterion`.

Generic criteria and algorithm-specific criteria therefore share exactly the same contract.

## Common callbacks

Callbacks receive an immutable `OptimizationEvent<TSolution>` value containing:
- event kind;
- common optimization state;
- best solution snapshot when available;
- current fitness when meaningful;
- optional algorithm-specific data.

Each callback declares the event types it consumes.
Evaluation-level callbacks remain opt-in because they may be very frequent.

`ConvergenceTraceCallback<TSolution>` provides a reusable convergence recorder without forcing history allocation on runs that do not need it.

## Common random source

Algorithms use `OptimizationContext<TSolution>.Random`.

They must not silently instantiate their own `System.Random` instances.
This makes a run reproducible from its recorded seed and random-source implementation.