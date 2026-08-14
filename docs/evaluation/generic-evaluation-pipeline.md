# Generic problem evaluation pipeline

## Goal

A metaheuristic must not know how a domain solution is constructed or evaluated.

The platform therefore separates:

```text
candidate / encoding
    |
    v
decoder
    |
    v
problem solution
    |
    +--> optional repair
    |
    +--> optional improvement / local search
    |
    v
evaluator
    |
    v
fitness
```

For Lamarckian hybrids an additional feedback stage can project the improved
problem solution back into the metaheuristic representation.

## Contracts

```text
ISolutionDecoder<TCandidate, TSolution>
ISolutionRepair<TSolution>
ISolutionImprover<TSolution>
ISolutionEvaluator<TSolution>
ILamarckianFeedback<TCandidate, TSolution>
```

`EvaluationPipeline<TCandidate, TSolution>` composes those contracts.

## Improvement semantics

### None

No improvement/local-search stage is executed.

### Baldwinian

The decoded solution is improved before evaluation.

The improved solution determines fitness but the original candidate representation
is not changed.

### Lamarckian

The decoded solution is improved before evaluation and then projected back into
the candidate representation.

This allows memetic/hybrid algorithms to inherit local-search improvements.

## MLLP target architecture

A future MLLP integration can be structured as:

```text
PSO particle / GA chromosome / DE vector
    |
    v
MllpDecoder
    |
    v
LotSizingInstance / scheduling decisions
    |
    +--> MllpRepair
    |
    +--> MllpLocalSearch
    |       |
    |       +--> optional ULSAlgorithms subproblem solver
    |
    v
MllpEvaluator
    |
    v
fitness
```

The PSO, GA or DE engine does not contain MLLP-specific code.

## Performance

The generic pipeline is intended for domain evaluation where decode/repair/local
search are meaningful operations.

It does not replace specialized low-overhead paths.

For example, continuous PSO with a cheap mathematical objective continues to use:

```csharp
double Evaluate(ReadOnlySpan<double> solution)
```

directly.

This keeps genericity out of the hottest numerical loop when it is not needed.

## Parallel evaluation

The pipeline exposes the generic `EvaluationCharacteristics` introduced in v0.8.3.

`EvaluationPipelineBatchExecutor` therefore automatically reuses:

- `EvaluationExecutionMode`;
- cost hints;
- variability hints;
- coarse scheduling for homogeneous work;
- fine-grained scheduling for heavy/high-variability work.

This is directly applicable to research problems where local-search duration varies
substantially between candidates.

## Caching

Caching is intentionally not embedded in v0.9.0.

A cache for a Lamarckian pipeline cannot safely store fitness alone: it may also need
the improved phenotype and/or projected candidate representation.

A later cache layer will therefore cache a complete evaluation outcome with explicit
ownership/cloning semantics rather than introducing an unsafe fitness-only shortcut.
## Value-type solution semantics (v0.12.0)

Repair and improvement now receive `ref TSolution`.

This makes the generic pipeline correct for struct-based high-performance solution
representations as well as mutable classes. See `ref-solution-semantics.md`.