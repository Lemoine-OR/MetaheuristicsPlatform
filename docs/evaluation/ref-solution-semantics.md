# Ref solution semantics

## Problem fixed in v0.12.0

The first generic evaluation pipeline used:

```csharp
bool Repair(TSolution solution, ...);
bool Improve(TSolution solution, ...);
```

This is naturally usable for mutable reference types, but it is semantically wrong for
value-type solutions.

A struct passed by value is copied. Replacing or mutating that copy does not update the
pipeline local.

## Current contract

```csharp
bool Repair(ref TSolution solution, ...);
bool Improve(ref TSolution solution, ...);
```

This supports:
- mutable classes;
- immutable classes replaced by a new instance;
- mutable structs;
- immutable record structs replaced by a new value.

## Delegate compatibility

Existing `SolutionMutationDelegate<TSolution>` remains available for mutable reference
types.

`RefSolutionMutationDelegate<TSolution>` is the preferred contract for new code.

The delegate adapters reject the old by-value delegate when `TSolution` is a value type,
preventing silent loss of mutations.

## Performance

The pipeline still owns one local `TSolution solution`.

Passing that local by ref does not require heap allocation and allows high-performance
value representations to participate in decode/repair/local-search pipelines.