# PSO topology API

## Separation of responsibilities

A PSO topology answers:

> Which particles can exchange social information?

It does **not** answer:

> How is information from those neighbors aggregated into a velocity update?

That second responsibility belongs to a social-influence policy and will be implemented
with the PSO movement engine.

This distinction is essential for combinations such as:

- Ring + best-of-neighborhood;
- Ring + fully-informed influence;
- Von Neumann + fully-informed influence;
- Scale-Free + weighted fully-informed influence.

## Main interface

```csharp
public interface IPsoTopology
{
    PsoTopologyDescriptor Descriptor { get; }

    NeighborhoodGraph CreateGraph(
        PsoTopologyContext context,
        IRandomSource random);
}
```

`PsoTopologyContext` contains only state that topology construction may need:
- swarm size;
- iteration;
- optimization sense;
- current fitness;
- personal-best fitness;
- positions.

Each topology declares the required fields using `PsoTopologyRequiredData`.

## Graph representation

`NeighborhoodGraph` is immutable and stored in CSR format.

This makes neighbor iteration:
- allocation-free;
- compact;
- cache-friendly;
- independent from `List<List<int>>` or object-heavy graph nodes.

Graph diagnostics are deliberately separated from hot PSO iteration code.

## Published exact versus generalized

`PsoTopologyDescriptor.IsPublishedExactVariant` distinguishes:
- an implementation that intentionally reproduces a published structure;
- a generalized or inspired reusable graph.

For example:
- `DClusterTopology` is exact and enforces its regular swarm-size relation;
- `ClusteredTopology` is generic and is **not** claimed to be the exact FourClusters
  adjacency matrix from Mendes et al.