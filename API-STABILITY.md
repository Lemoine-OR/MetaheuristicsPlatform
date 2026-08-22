# API stability

MetaheuristicsPlatform follows the same repository discipline as ULSAlgorithms.

## Stable contract now

Even during the 0.x development line, the following are treated as compatibility
contracts:
- public algorithm catalog IDs;
- public Simulated Annealing cooling-schedule IDs (`sa.cooling.*`);
- public Threshold Accepting ID (`threshold-accepting-dueck-scheuer-1990`) introduced in v0.33.0;
- public Great Deluge ID (`great-deluge-dueck-1993`) introduced in v0.34.0;
- public Record-to-Record Travel ID (`record-to-record-travel-dueck-1993`) introduced in v0.34.0;
- public Late Acceptance Hill Climbing ID (`late-acceptance-hill-climbing-burke-bykov-2017`) introduced in v0.35.0;
- public Demon-Based Acceptance ID (`demon-based-acceptance-talbi-2009`) introduced in v0.36.0;
- public Iterated Greedy ID (`iterated-greedy-ruiz-stutzle-2007`) introduced in v0.37.0;
- Advanced Iterated Greedy component IDs (`ig.*`) introduced in v0.38.0; the canonical public IG algorithm ID remains unchanged;
- public Tabu Search stable ID (`tabu-search-glover`);
- public Reactive Tabu Search stable ID (`reactive-tabu-search-battiti-tecchiolli-1994`);
- public Tabu Search component IDs (`ts.*`);
- public Local Search Foundation algorithm IDs introduced in v0.23.0;
- public Multi-Start Local Search ID (`multi-start-local-search`) introduced in v0.24.0;
- public Iterated Local Search ID (`iterated-local-search-lourenco-martin-stutzle`) introduced in v0.24.0;
- public Variable Neighborhood Descent ID (`variable-neighborhood-descent`) introduced in v0.25.0;
- public canonical Variable Neighborhood Search ID (`variable-neighborhood-search-mladenovic-hansen`) introduced in v0.25.0;
- public Guided Local Search ID (`guided-local-search-voudouris-tsang-1999`) introduced in v0.26.0;
- public Reduced Variable Neighborhood Search ID (`reduced-variable-neighborhood-search`) introduced in v0.27.0;
- public General Variable Neighborhood Search ID (`general-variable-neighborhood-search`) introduced in v0.27.0;
- public Skewed Variable Neighborhood Search ID (`skewed-variable-neighborhood-search-hansen-mladenovic-2001`) introduced in v0.27.0;
- public canonical GRASP ID (`grasp-feo-resende-1995`) introduced in v0.28.0;
- public Reactive GRASP ID (`reactive-grasp-prais-ribeiro-2000`) introduced in v0.29.0;
- public GRASP with Path Relinking ID (`grasp-path-relinking`) introduced in v0.30.0;
- public Ant System stable ID (`ant-system-dorigo-maniezzo-colorni-1996`) introduced in v0.44.0;
- scientific method identity;
- serialized/reproducibility-facing identifiers;
- documentation URLs generated from stable IDs.

## Before 1.0

Type-level APIs may still evolve while the generic architecture is being completed.
Breaking changes must be documented in `CHANGELOG.md` and must not silently reuse a
stable ID for a different scientific method.

## 1.x target

The 1.x line will freeze the public common lifecycle and factory/catalog conventions in
the same spirit as ULSAlgorithms.

### v0.39.0

- public Scatter Search ID (`scatter-search-marti-laguna-glover-2006`);
- generic five-method contracts for diversification, improvement, RefSet update, subset generation and solution combination;
- `ClassicalScatterSearchReferenceSetUpdateMethod<TSolution>` quality/diversity RefSet semantics;
- `PairwiseNewScatterSearchSubsetGenerationMethod<TSolution>` canonical simple subset policy.

### v0.40.0

- canonical public Scatter Search ID remains `scatter-search-marti-laguna-glover-2006`;
- stable Advanced Scatter Search component IDs use the `ss.*` namespace;
- `RoundSnapshot` remains the compatibility default for subset refresh;
- RefSet rebuilding is opt-in and disabled by default;
- three-tier good-generator, hash-assisted duplicate control and specialized
  combination designs remain reviewed/deferred until their required typed
  semantics exist.
### v0.41.0

- public Generational Genetic Algorithm ID: `genetic-algorithm-generational`;
- stable generic contracts for population initialization, parent selection, crossover and mutation;
- `TournamentGeneticParentSelectionMethod<TSolution>` is the canonical foundation selector;
- `GeneticAlgorithmParameters` defines population size, generation bound, crossover/mutation invocation probabilities and optional elitism;
- composed GA construction uses typed `MetaheuristicFactory.Register(...)`;
- no representation-specific crossover/mutation identity is claimed by the v0.41 foundation.

### v0.42.0

- canonical public GA ID remains genetic-algorithm-generational;
- stable Advanced Genetic Algorithm component IDs use the ga.* namespace;
- sequence crossover IDs do not imply permutation feasibility; PMX/OX1 are the permutation-preserving components;
- bounded SBX/Gaussian/polynomial operators require explicit finite bounds;
- fitness-proportionate selection requires explicit non-negative user weights; raw objectives are not silently converted;
- true steady-state replacement remains reviewed/deferred because it requires a different live-population lifecycle.

### v0.43.0

- Stable public algorithm ID: `memetic-algorithm-moscato-1989`.
- Public composition: `MemeticAlgorithmOptimizer<TSolution>` and `MemeticAlgorithmParameters`.
- Public policy contracts: `IMemeticLocalSearchPolicy` and `IMemeticLearningPolicy`.
- Executable component IDs: `ma.local-search.every-offspring`, `ma.local-search.periodic`, `ma.local-search.probabilistic`, `ma.local-search.top-fraction`, `ma.local-search.adaptive-stagnation`, `ma.learning.lamarckian`, `ma.learning.baldwinian`.
- The generation-extension hook added to `GenerationalGeneticAlgorithmOptimizer<TSolution>` is internal; existing public GA construction, ID and parameter contracts remain source-compatible.

### v0.44.0

- Stable public Ant System ID: `ant-system-dorigo-maniezzo-colorni-1996`.
- Public composition contracts: `IAntColonyConstructionModel<...>`, `IAntColonyCandidateEnumerator<TComponent>` and `IAntSystemDepositPolicy<TSolution>`.
- Stable ACO component IDs use the `aco.*` namespace.
- The v0.44 identity is canonical Ant System with proportional transition, global evaporation and all-ant reinforcement.
- Ant Colony System and MAX-MIN Ant System remain separate reviewed/deferred identities; their local-update, exploitation and pheromone-bound semantics are not silently folded into Ant System.
- Generic ACO construction is discrete/constructive in v0.44; continuous-domain ACO requires a distinct future sampling contract.


## v0.45.0 Advanced Ant Colony Optimization

Stable public IDs introduced:

- `ant-colony-system-dorigo-gambardella-1997`
- `max-min-ant-system-stutzle-hoos-2000`

The v0.44 Ant System ID and public contracts remain stable. ACS and MMAS reuse the same typed construction model and deposit-policy contracts.


## v0.46.0 CMA-ES

Stable public ID introduced:

- `cma-es-hansen-ostermeier-2001`

`CmaEsOptimizer`, `CmaEsParameters`, `CmaEsState`, component IDs and scientific references are public API. The algorithm uses the existing bounded continuous problem/search-space contracts.

## v0.47.0 Advanced CMA-ES

Stable public IDs introduced:

- `active-cma-es-hansen-ros-2010`
- `separable-cma-es-ros-hansen-2008`

The existing `CmaEsParameters` contract is reused. Stable component IDs
`cma.covariance.active` and `cma.variant.separable` become executable.
IPOP/BIPOP restart IDs remain reviewed/deferred until restart orchestration
can preserve one exact global evaluation lifecycle.

## v0.48.0 Restart CMA-ES

Stable public IDs introduced:

- `ipop-cma-es-auger-hansen-2005`
- `bipop-cma-es-hansen-2009`

`RestartCmaEsParameters`, `RestartCmaEsState`, `RestartCmaEsRegime`,
`IpopCmaEsOptimizer` and `BipopCmaEsOptimizer` are public API.
Component IDs `cma.restart.ipop` and `cma.restart.bipop` become executable.
Both algorithms own exactly one common `OptimizationContext` across all restarts.

## v0.49.0 Artificial Bee Colony

Stable public ID introduced:

- `artificial-bee-colony-karaboga-basturk-2007`

`ArtificialBeeColonyOptimizer`, `ArtificialBeeColonyParameters`,
`ArtificialBeeColonyState`, `ArtificialBeeColonyPhase` and
`ArtificialBeeColonyReferences` are public API. The implementation uses the existing
bounded continuous problem/search-space contracts and the common `OptimizationContext`.
