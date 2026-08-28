@page family_hyper_heuristics Hyper-heuristics and algorithm selection

# Hyper-heuristics and algorithm selection

Hyper-heuristics operate above problem-specific search operators. They select, sequence, adapt
or generate lower-level heuristics through a domain interface rather than hard-coding one
problem-specific neighborhood.

## Scientific scope

The family contains selection hyper-heuristics, adaptive operator selection, heuristic-memory
policies, bandit/credit mechanisms and move-acceptance combinations.

## Platform contract

- `IHyperHeuristicDomain` provides the objective, initial solution, features and low-level pool.
- `ILowLevelHeuristic` applies one domain-level search operation.
- `HyperHeuristicOptimizationResult` reports the best solution and the selected-heuristic trace.
- Every scientific page separates the paper's mechanism from platform adaptation.

## Methods
- **[Tabu-Search Hyper-Heuristic](../algorithms/tabu-search-hyperheuristic-burke-kendall-soubeiga-2003.md)** - `tabu-search-hyperheuristic-burke-kendall-soubeiga-2003` - Heuristic-level reinforcement scores compete under a tabu list; non-improving low-level heuristics become temporarily unavailable.
- **[Case-Based Heuristic Selection](../algorithms/case-based-heuristic-selection-burke-petrovic-qu-2006.md)** - `case-based-heuristic-selection-burke-petrovic-qu-2006` - A feature description of the current search state retrieves the most similar stored case and reuses its associated low-level heuristic.
- **[Graph-Based Hyper-Heuristic](../algorithms/graph-based-hyperheuristic-burke-mccollum-meisels-petrovic-qu-2007.md)** - `graph-based-hyperheuristic-burke-mccollum-meisels-petrovic-qu-2007` - A tabu search explores permutations of domain-provided low-level heuristics; each sequence is evaluated as a high-level heuristic ordering.
- **[Late Acceptance Hyper-Heuristic](../algorithms/late-acceptance-hyperheuristic-ozcan-bykov-birben-burke-2009.md)** - `late-acceptance-hyperheuristic-ozcan-bykov-birben-burke-2009` - A low-level heuristic generates a candidate and late acceptance compares it with both the current objective and a historical objective.
- **[Dynamic Multi-Armed Bandit Adaptive Operator Selection](../algorithms/dynamic-mab-aos-da-costa-fialho-schoenauer-sebag-2008.md)** - `dynamic-mab-aos-da-costa-fialho-schoenauer-sebag-2008` - Low-level heuristics are arms of a dynamic multi-armed bandit; UCB selection is coupled to a change statistic that can reset stale credit.

## Navigation

Return to @ref method_families "method families".
