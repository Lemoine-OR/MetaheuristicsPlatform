@page family_trajectory_based_methods Trajectory-based methods

# Trajectory-based methods

Single-solution methods that explore a neighborhood trajectory through explicit acceptance,
memory and state-transition policies.

## Methods

- **[Simulated Annealing](../algorithms/simulated-annealing-metropolis.md)** —
  `simulated-annealing-metropolis` — O(C_move + C_eval) per attempted transition; O(C_delta)
  when an exact differential evaluator is available.
- **[Threshold Accepting](../algorithms/threshold-accepting-dueck-scheuer-1990.md)** —
  `threshold-accepting-dueck-scheuer-1990` — deterministic threshold acceptance with
  exact-delta/reversible trajectory execution.
- **[Great Deluge Algorithm](../algorithms/great-deluge-dueck-1993.md)** —
  `great-deluge-dueck-1993` — classical Dueck absolute water-level acceptance.
- **[Record-to-Record Travel](../algorithms/record-to-record-travel-dueck-1993.md)** —
  `record-to-record-travel-dueck-1993` — deviation around the best accepted record.
- **[Late Acceptance Hill Climbing](../algorithms/late-acceptance-hill-climbing-burke-bykov-2017.md)** —
  `late-acceptance-hill-climbing-burke-bykov-2017` — final Burke-Bykov circular-history acceptance.
- **[Demon-Based Acceptance](../algorithms/demon-based-acceptance-talbi-2009.md)** —
  `demon-based-acceptance-talbi-2009` — conserved non-negative credit/energy acceptance.
- **[Iterated Greedy](../algorithms/iterated-greedy-ruiz-stutzle-2007.md)** —
  `iterated-greedy-ruiz-stutzle-2007` — generic destruction/reconstruction with optional local improvement and pluggable acceptance.
- **[Large Neighborhood Search](../algorithms/large-neighborhood-search-shaw-1998.md)** —
  `large-neighborhood-search-shaw-1998` — generic owned-clone destroy/repair large-neighborhood trajectory with explicit acceptance.
- **[Adaptive Large Neighborhood Search](../algorithms/adaptive-large-neighborhood-search-ropke-pisinger-2006.md)** —
  `adaptive-large-neighborhood-search-ropke-pisinger-2006` — performance-weighted destroy/repair pools with segmented reaction-factor learning and Metropolis acceptance.
- **[Tabu Search](../algorithms/tabu-search-glover.md)** — `tabu-search-glover` —
  best-admissible neighborhood search with short-term memory and aspiration.
- **[Reactive Tabu Search](../algorithms/reactive-tabu-search-battiti-tecchiolli-1994.md)** —
  `reactive-tabu-search-battiti-tecchiolli-1994` — repetition-aware adaptive tenure and
  reactive diversification.

## Scientific components

- @ref simulated_annealing_cooling_schedules "Simulated Annealing Scientific Cooling Catalog".
- @ref threshold_accepting_schedules "Threshold Accepting Schedule Catalog".
- @ref acceptance_based_trajectory_methods "Acceptance-Based Trajectory Methods".
- @ref tabu_search_memory_control_strategies "Tabu Search Memory and Reactive Control Catalog".
- @ref large_neighborhood_search_components "Large Neighborhood Search Components".
- @ref adaptive_large_neighborhood_search_components "Adaptive Large Neighborhood Search Components".

## v0.23.0 Local Search Core

- `local-search-best-improvement` - Local Search, best improvement.
- `local-search-first-improvement` - Local Search, first improvement.
## Navigation

Return to @ref method_families "method families".
