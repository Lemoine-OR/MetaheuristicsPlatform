@page reference_grade_reference_grade_stability_gate Reference-Grade Stability Gate

# Reference-Grade Stability Gate

## Purpose

Add the final reference-grade gate integrating provenance, reproducibility, schema, benchmark, composition, random-trace and reference checks.

## Contract

Public type: `ReferenceGradeStabilityGate`. Stable consolidation ID: `reference-grade-stability-gate`.

## Invariants

- Deterministic validation for identical inputs.
- No mutation of algorithm catalog IDs or factory registration.
- No dependency on Publication Core internals.
- Builds on the reference-grade contracts introduced before 0.173.0.

## API example

```csharp
// See `ReferenceGradeStabilityGate` in MetaheuristicsPlatform.ReferenceGrade.
```

## Failure modes

Invalid or incomplete reference-grade metadata fails fast through explicit argument or state validation.

## Stability guarantee

The public contract introduced in v0.173.0 is additive; existing algorithm IDs and optimizer signatures are unchanged.

## Versioning rule

This consolidation release changes the library version without adding a new algorithm identity.

## Validation

The release ships a dedicated xUnit test and documentation-contract script, and is validated by the unchanged frozen Publication Core v1 pipeline.
