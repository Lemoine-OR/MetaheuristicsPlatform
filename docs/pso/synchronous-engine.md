# Synchronous PSO execution engine

## Iteration semantics

A synchronous iteration uses a stable set of personal-best positions as social
information while every particle computes its next position.

Only after movement are new positions evaluated and personal bests updated.

This means particles can be moved in parallel without locks:
- particle `i` writes only position/velocity `i`;
- all social personal-best data are read-only during movement.

## Objective evaluation

`ISpanContinuousOptimizationProblem` provides allocation-free objective evaluation.

A problem explicitly declares whether concurrent evaluations are safe.

If:
- the PSO parameters allow parallel objective evaluation; and
- the problem declares `SupportsParallelEvaluation=true`;

then objective evaluations use the same coarse range executor as particle movement.

Otherwise they remain sequential.

## Deterministic commit

Parallel objective values are written into the particle fitness buffer.

They are then registered with the common `OptimizationContext` in deterministic particle
order.

A `double[]` candidate snapshot is created only when a fitness can become a new global
best. That owned snapshot is transferred directly to the Core, avoiding a second clone.

Non-improving evaluations update counters/callbacks without allocating solution arrays.

## Fused standard kernels

The standard high-performance path recognizes:
- `CanonicalBestInfluencePolicy`;
- `FullyInformedInfluencePolicy`.

For these, attraction, velocity, position and boundary handling occur in one dimension
loop.

For fully connected canonical PSO, the global social guide is reduced once in O(N) and
reused by every particle; the engine does not perform N complete-neighborhood scans.

Custom influence policies remain supported through a preallocated attraction scratch
buffer.

This preserves extensibility without forcing the standard research path to pay all
abstraction costs.