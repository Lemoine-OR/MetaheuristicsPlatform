@page reference_grade_scientific_provenance Scientific Provenance Contract

# Scientific Provenance Contract

## Purpose

Add immutable scientific provenance records with canonical SHA-256 fingerprints.

## Contract

Public type: `ScientificProvenanceRecord`. Stable consolidation ID: `reference-grade-scientific-provenance`.

## Invariants

- Deterministic validation for identical inputs.
- No mutation of algorithm catalog IDs or factory registration.
- No dependency on Publication Core internals.
- None; this is the first reference-grade consolidation component.

## API example

```csharp
// See `ScientificProvenanceRecord` in MetaheuristicsPlatform.ReferenceGrade.
```

## Failure modes

Invalid or incomplete reference-grade metadata fails fast through explicit argument or state validation.

## Stability guarantee

The public contract introduced in v0.166.0 is additive; existing algorithm IDs and optimizer signatures are unchanged.

## Versioning rule

This consolidation release changes the library version without adding a new algorithm identity.

## Validation

The release ships a dedicated xUnit test and documentation-contract script, and is validated by the unchanged frozen Publication Core v1 pipeline.
