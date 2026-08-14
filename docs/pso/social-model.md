# PSO social model

## Four separate layers

The platform deliberately separates:

1. **Topology** — who is allowed to inform whom?
2. **Guide selection** — if a policy needs one guide, which informer is selected?
3. **Influence aggregation** — how are one or several informers converted into stochastic attraction?
4. **Movement dynamics** — how is attraction combined with previous velocity, inertia or constriction?

Only the first three exist after v0.5.0.
Movement dynamics arrive in the next PSO pack.

## Why this decomposition matters

A ring topology can be combined with:
- canonical best-neighbor influence;
- FIPS;
- generic weighted fully-informed influence.

The same is true for Von Neumann, fully connected, DCluster and later dynamic topologies.

This avoids duplicated PSO implementations.

## Buffer ownership

`IPsoInfluencePolicy.ComputeAttraction` writes into a caller-owned `Span<double>`.

The future PSO engine will allocate/reuse work buffers once per worker or particle,
rather than allocate an attraction vector on every update.

## Best-neighborhood selection

`BestNeighborhoodGuideSelector` evaluates personal-best fitness and uses the shared
`OptimizationSense`.

It therefore works correctly for both minimization and maximization.

## Generic weighted influence

`WeightedFullyInformedInfluencePolicy` is infrastructure, not a named publication
implementation.

Exact algorithms must receive their own class/preset and full source metadata.