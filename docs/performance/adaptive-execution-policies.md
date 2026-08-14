# Adaptive execution policies

## Why movement and evaluation are separate

The first calibration campaign showed that the best parallelization decision depends
on the kind of work.

PSO movement is homogeneous and approximately driven by:
- particle count;
- dimension;
- processor count.

Objective evaluation may instead contain:
- decoding;
- repair;
- local search;
- exact subproblem solving;
- simulation;
- external numerical kernels.

Its cost and variance can therefore differ by orders of magnitude.

## Movement

`PsoExecutionOptions` now controls movement/social kernels only.

In Auto mode, the default calibrated rule is CPU-scaled and shape-aware:

```text
particleCount >= max(16, 2 * processorCount)
and
particleCount * dimension >= max(1024, 160 * processorCount)
```

An explicit `MinimumParallelWork > 0` overrides this with the legacy scalar threshold.

The rule is intentionally configurable and is not claimed to be universally optimal.

## Evaluation

Generic evaluation behavior lives under `MetaheuristicsPlatform.Execution`.

A problem can expose:

```text
EvaluationCharacteristics
  SupportsParallelEvaluation
  CostHint
  VariabilityHint
```

Cost hints:
- Unknown
- Trivial
- Light
- Medium
- Heavy
- VeryHeavy

Variability hints:
- Unknown
- Uniform
- Moderate
- High

Heavy/high-variability evaluation uses fine-grained candidate scheduling for load
balancing. Cheap homogeneous evaluation uses coarse ranges.

## MLLP example

A future MLLP adapter may declare:

```text
SupportsParallelEvaluation = true
CostHint = Heavy
VariabilityHint = High
```

when one evaluation performs:

```text
particle encoding
 -> decode
 -> repair
 -> local search
 -> objective
```

The metaheuristic itself does not need to know any MLLP detail.

## Research reproducibility

Execution policy changes scheduling only.

Particle-owned RNG streams remain attached to particle identity rather than thread
identity, so synchronous PSO remains deterministic for a fixed seed.