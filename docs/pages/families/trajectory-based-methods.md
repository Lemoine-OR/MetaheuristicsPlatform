@page family_trajectory_based_methods Trajectory-based methods

# Trajectory-based methods

Single-solution methods that explore a neighborhood trajectory through explicit acceptance and state-transition policies.

## Methods

- **[Simulated Annealing](../algorithms/simulated-annealing-metropolis.md)** — `simulated-annealing-metropolis` — O(C_move + C_eval) per attempted transition; O(C_delta) when an exact differential evaluator is available
- **[Tabu Search](../algorithms/tabu-search-glover.md)** — `tabu-search-glover` — best-admissible memory-guided neighborhood search with an exact-delta fast path

## Navigation

Return to @ref method_families "method families".
