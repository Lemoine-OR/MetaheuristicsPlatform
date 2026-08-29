@page reference_grade_benchmark_protocol Benchmark Protocol & Result Envelope

# Benchmark Protocol & Result Envelope

## Purpose

Add reproducible benchmark protocols, metric envelopes and exact run metadata.

## Contract

Public type: `BenchmarkProtocol`. Stable consolidation ID: `reference-grade-benchmark-protocol`.

## Invariants

- Deterministic validation for identical inputs.
- No mutation of algorithm catalog IDs or factory registration.
- No dependency on Publication Core internals.
- Builds on the reference-grade contracts introduced before 0.169.0.

## API example

```csharp
// See `BenchmarkProtocol` in MetaheuristicsPlatform.ReferenceGrade.
```

## Failure modes

Invalid or incomplete reference-grade metadata fails fast through explicit argument or state validation.

## Stability guarantee

The public contract introduced in v0.169.0 is additive; existing algorithm IDs and optimizer signatures are unchanged.

## Versioning rule

This consolidation release changes the library version without adding a new algorithm identity.

## Validation

The release ships a dedicated xUnit test and documentation-contract script, and is validated by the unchanged frozen Publication Core v1 pipeline.
