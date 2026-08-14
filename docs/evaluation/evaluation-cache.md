# Evaluation cache

## Why fitness-only caching is insufficient

For a plain objective, a cache can appear to be:

```text
candidate key -> fitness
```

That is unsafe for a generic hybrid pipeline.

A Lamarckian evaluation can modify the candidate representation after local search.
A decoded solution may also be mutable.

The cache therefore stores a complete owned outcome:

```text
EvaluationCacheEntry
    fitness
    cloned evaluated solution
    cloned Lamarckian candidate, when applicable
    repair/improvement/feedback flags
```

Every cache hit clones the cached snapshots before exposing them to the caller.

## Thread safety

`ConcurrentEvaluationCache<TKey,TValue>` stores `Lazy<TValue>` in a
`ConcurrentDictionary`.

When several workers request the same key simultaneously:
- one stored `Lazy<TValue>` wins;
- all callers share its result;
- the expensive evaluation executes once.

A faulted/cancelled lazy value is removed so a later request can retry.

## Cache keys

The platform intentionally does not guess equality for generic candidates.

A problem supplies:

```csharp
IEvaluationCacheKeySelector<TCandidate,TKey>
```

The selected key must represent all candidate state that affects the pipeline outcome.

For mutable arrays, object reference identity is usually NOT a valid scientific cache
key. A domain adapter should use a stable immutable key or a verified structural hash
with equality checking.

## Ownership

Reference-type snapshots require a real `IEvaluationSnapshotCloner<T>`.

`ImmutableEvaluationSnapshotCloner<T>` is an identity cloner and is only correct for
genuinely immutable values.