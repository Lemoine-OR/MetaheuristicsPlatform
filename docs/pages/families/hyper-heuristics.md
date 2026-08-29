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
- **[Extreme-Value Dynamic Multi-Armed Bandit AOS](../algorithms/extreme-value-dmab-fialho-da-costa-schoenauer-sebag-2009.md)** - `extreme-value-dmab-fialho-da-costa-schoenauer-sebag-2009` - Operator credit is the extreme recent improvement in a bounded reward window and is combined with upper-confidence exploration.
- **[Bandit-Based Adaptive Operator Selection](../algorithms/bandit-aos-fialho-da-costa-schoenauer-sebag-2010.md)** - `bandit-aos-fialho-da-costa-schoenauer-sebag-2010` - Each low-level heuristic is a bandit arm whose empirical mean reward is balanced against an upper-confidence exploration bonus.
- **[Fitness-Rate-Rank Multi-Armed Bandit](../algorithms/frrmab-li-fialho-kwong-zhang-2014.md)** - `frrmab-li-fialho-kwong-zhang-2014` - A sliding window accumulates fitness-improvement rewards, ranks operators by recent fitness-rate credit and combines rank credit with bandit exploration.
- **[Reinforcement Learning Great-Deluge Hyper-Heuristic](../algorithms/reinforcement-learning-great-deluge-hh-ozcan-misir-ochoa-burke-2010.md)** - `reinforcement-learning-great-deluge-hh-ozcan-misir-ochoa-burke-2010` - Reinforcement-learning utility values adapt low-level heuristic selection online, while Great Deluge supplies move acceptance through a decreasing water level.
- **[ILS Hyper-Heuristic with Effective Heuristic Subset](../algorithms/ils-hyperheuristic-soria-alcaraz-ochoa-sotelo-burke-2017.md)** - `ils-hyperheuristic-soria-alcaraz-ochoa-sotelo-burke-2017` - A bandit model identifies an effective low-level heuristic subset; the search iterates within that subset and periodically perturbs/refines it.
- **[Late-Acceptance Cross-Domain Selection Hyper-Heuristic](../algorithms/late-acceptance-selection-hh-jackson-ozcan-drake-2013.md)** - `late-acceptance-selection-hh-jackson-ozcan-drake-2013` - A choice-function score balances learned performance and recency, while late acceptance provides move acceptance.
- **[Fuzzy Adaptive Late-Acceptance Hyper-Heuristic](../algorithms/fuzzy-adaptive-late-acceptance-hh-jackson-ozcan-john-2014.md)** - `fuzzy-adaptive-late-acceptance-hh-jackson-ozcan-john-2014` - The late-acceptance history length adapts online from improvement and stagnation signals through a portable rule-based fuzzy-control adaptation.

## Navigation

Return to @ref method_families "method families".
