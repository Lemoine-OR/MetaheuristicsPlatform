@page adaptive_large_neighborhood_search_components Adaptive Large Neighborhood Search Components

# Adaptive Large Neighborhood Search Components

The v0.53 adaptive layer reuses the generic LNS destroy and repair contracts and adds four
canonical ALNS control mechanisms: independent roulette selection, segmented reaction-factor
weight learning, novelty-aware outcome scoring and geometric Metropolis acceptance.

## Executable components

### `alns.selection.roulette-independent`

Destroy and repair operators are selected independently with probabilities proportional to
their current weights.

### `alns.adaptation.segmented-reaction-factor`

At each segment boundary, every operator used in the segment receives the Ropke-Pisinger
weight update. An unused operator keeps its previous weight.

### `alns.scoring.novel-outcome`

The selected destroy and repair operators receive the same reward tier. Previously visited
solutions receive no adaptive reward. Solution identity is provided explicitly through the
optimizer's equality comparer.

### `alns.acceptance.geometric-metropolis`

The canonical default acceptance policy uses geometric simulated annealing. Because objective
scales are generic in the platform, the starting temperature is explicit rather than inferred
from an assumed positive routing cost.

## Advanced ALNS composition

v0.54 publishes executable advanced components separately under @ref advanced_adaptive_large_neighborhood_search_components "Advanced Adaptive Large Neighborhood Search Components". They remain outside the canonical Ropke-Pisinger identity described on this page.

## Mechanisms still deferred from the canonical identity

### Pair-coupled operator weights

A joint destroy-repair pair controller changes the learning state from two independent weight
vectors to a matrix. v0.54 implements it as a separate advanced component.

### Alternative acceptance criteria

Replacing the canonical simulated-annealing default belongs to advanced ALNS composition. v0.54 exposes Threshold Accepting and Record-to-Record Travel through the common trajectory-acceptance adapter. Santini, Ropke & Hvattum (2018), DOI `10.1007/s10732-018-9377-x`, provides the comparative reference.

### Contextual and learned operator selection

Bandit and learned policies are modern extensions. They are not retroactively folded into the
2006 stable identity.

## Scientific references

- Ropke & Pisinger (2006), DOI `10.1287/trsc.1050.0135`.
- Pisinger & Ropke (2007), DOI `10.1016/j.cor.2005.09.012`.
- Santini, Ropke & Hvattum (2018), DOI `10.1007/s10732-018-9377-x`.
