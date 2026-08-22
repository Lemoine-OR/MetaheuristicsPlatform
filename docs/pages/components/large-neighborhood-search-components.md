@page large_neighborhood_search_components Large Neighborhood Search Components

# Large Neighborhood Search Components

The v0.52 foundation separates the generic LNS lifecycle from problem-specific destroy and
repair semantics. This is the architectural base on which Adaptive Large Neighborhood Search
(ALNS) can later select among multiple competing operators without changing the meaning of
the canonical Shaw LNS identity.

## Executable generic components

### `lns.destroy.operator`

`ILargeNeighborhoodDestroyOperator<TSolution,TRemoved>` receives an owned candidate clone,
the configured destruction size, the optimization problem and the deterministic platform RNG.
It may mutate the candidate into a partial representation and returns the domain-owned token
required for repair.

### `lns.repair.operator`

`ILargeNeighborhoodRepairOperator<TSolution,TRemoved>` restores the destroyed candidate to a
complete evaluable solution. The generic objective evaluator is intentionally not exposed
between destruction and repair.

### `lns.acceptance.improving-only`

`ImprovingOnlyLargeNeighborhoodAcceptancePolicy` accepts only strict incumbent improvement
under the configured optimization sense.

## Reviewed but deliberately deferred

### Shaw related removal

Shaw's original vehicle-routing removal operator uses a problem-specific notion of related
customer visits. An exact generic implementation would falsely universalize routing distance,
time and demand semantics, so it remains a domain composition rather than a built-in operator.

### Constraint-based reinsertion with Limited Discrepancy Search

The original reconstruction uses constraint-programming tree search and Limited Discrepancy
Search. It is scientifically important but requires a first-class constraint-search model,
not a Boolean option on the generic LNS core.

### Adaptive operator selection

Ropke & Pisinger (2006) use multiple competing destroy and repair subheuristics with
performance-dependent usage frequencies. This is the planned ALNS layer and remains distinct
from v0.52 LNS.

## Scientific references

- Shaw (1998), DOI `10.1007/3-540-49481-2_30`.
- Pisinger & Ropke (2010), DOI `10.1007/978-1-4419-1665-5_13`.
- Ropke & Pisinger (2006), DOI `10.1287/trsc.1050.0135`.
