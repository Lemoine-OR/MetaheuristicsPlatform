# Changelog

All notable changes to MetaheuristicsPlatform will be documented in this file.

## [0.54.0] - 2026-08-23

### Added

- Advanced ALNS component layer without creating a second algorithm identity.
- Pair-coupled segmented roulette selection with joint destroy/repair weights.
- Alpha-UCB destroy/repair pair selection following Hendel (2022), DOI `10.1007/s12532-021-00209-7`.
- Generic trajectory-acceptance adapter exposing Threshold Accepting and Record-to-Record Travel to ALNS.
- Advanced ALNS component catalog, documentation, focused tests and benchmarks.

### Changed

- `AdaptiveLargeNeighborhoodSearchOptimizer<TSolution,TRemoved>` now accepts an optional operator-selection strategy while preserving every v0.53 constructor and default behavior.
- The canonical v0.53 selector is implemented as `IndependentSegmentedRouletteOperatorSelectionStrategy`.
- Public algorithm count remains 44; v0.54 is a component/API release.

## [0.53.0] - 2026-08-22

### Added

- Canonical Adaptive Large Neighborhood Search layer following Ropke-Pisinger 2006.
- Stable ID `adaptive-large-neighborhood-search-ropke-pisinger-2006`, primary DOI `10.1287/trsc.1050.0135`.
- Independently weighted destroy and repair pools selected by roulette-wheel probabilities.
- Novel-solution sigma reward tiers and segmented reaction-factor weight updates.
- Geometric simulated-annealing acceptance compatible with the v0.52 LNS acceptance contract.
- Adaptive LNS scientific component catalog, tests, benchmark and complete mathematical documentation.

### Changed

- Public algorithm count increases from 43 to 44; trajectory-based count increases from 20 to 21.
- LNS documentation now points to the separate public ALNS identity instead of describing adaptation as merely planned.
- The v0.52 test-helper provenance and historical README-count isolation guards remain mandatory release-pack rules.

## [0.52.0] - 2026-08-22

### Added

- Generic Large Neighborhood Search foundation following Shaw's destroy-and-repair large-neighborhood principle.
- Stable ID `large-neighborhood-search-shaw-1998` with primary DOI `10.1007/3-540-49481-2_30`.
- Public destroy, repair and acceptance contracts plus delegate-backed composition adapters.
- Strict-improvement default acceptance, complete-candidate-only evaluation and exact incomplete-cycle budget accounting.
- LNS scientific component catalog separating generic executable contracts from Shaw-specific routing operators and future ALNS adaptation.
- Focused tests, benchmark coverage and full mathematical documentation.

### Changed

- Public algorithm count increases from 42 to 43; trajectory-based count increases from 19 to 20.
- README trajectory count is now validated dynamically from the canonical algorithm catalog.
- Historical README compatibility no longer freezes aggregate algorithm-family counts.
- Pack automation advances to v5.9: CI monitoring identifies workflows by stable path and treats an already-published exact-version release as terminal success.

## [0.51.0] - 2026-08-22

### Added

- Canonical bounded-continuous Firefly Algorithm with distance-decaying attractiveness and additive stochastic exploration.
- Stable ID `firefly-algorithm-yang-2009` with Yang 2009/2010 DOI provenance.
- Objective-sense symmetric sequential pairwise attraction, exact partial-sweep budget handling, determinism tests and benchmark coverage.
- Dedicated Firefly scientific validator and mathematical documentation.

### Changed

- Public algorithm count increases from 41 to 42; swarm-intelligence count increases from 5 to 6.
- README swarm count validation is now derived dynamically from the algorithm catalog.
- v0.50 release-gate hotfix is the mandatory base: workflow runs are identified by stable workflow paths rather than display/run names.

## [0.50.0] - 2026-08-22

### Added

- Continuous Cross-Entropy Method with diagonal normal sampling, elite maximum-likelihood updates and dynamic standard-deviation smoothing.
- Stable ID `cross-entropy-continuous-kroese-porotsky-rubinstein-2006` with Rubinstein/de Boer/Kroese scientific provenance.
- Exact partial-iteration evaluation-budget semantics, deterministic tests and benchmark coverage.
- Permanent release-workflow topology validation.

### Changed

- Serialize the automatic CI/release path as Build and Test -> Build Documentation -> Create Release.
- Remove the redundant direct Create Release trigger from Build and Test while preserving exact-SHA/idempotent release gates.
- Make README public/evolutionary counts dynamic in the quality validator.

## [0.49.0] - 2026-08-22

### Added

- Canonical continuous Artificial Bee Colony (ABC) with employed, onlooker and scout phases.
- Stable ID `artificial-bee-colony-karaboga-basturk-2007` with peer-reviewed DOI provenance.
- Exact partial-cycle evaluation-budget handling, deterministic seeds, scout abandonment tests and benchmark coverage.
- Repository-level README quality validator.

### Changed

- Rebuilt the GitHub README into balanced, fully clickable algorithm and scientific-component grids.
- Removed empty table cells and the internal `Documentation contract` section from the public README.
- Fixed the swarm-family documentation, including the malformed ACS stable-ID text.

## [0.48.0] - 2026-08-22

### Added

- Public IPOP-CMA-ES with canonical geometrically increasing population restarts.
- Public BIPOP-CMA-ES with evaluation-budget-balanced large and randomized small population regimes.
- A single global `OptimizationContext` across every restart, preserving exact evaluations, best-so-far, callbacks, stopping and seed ownership.
- Restart CMA-ES parameters/state, focused tests, benchmarks and mathematical documentation.
- Historical Advanced CMA validator decoupled from future restart-component statuses.

## [0.47.0] - 2026-08-22

### Added

- Public weighted Active CMA-ES with negative covariance information from unsuccessful ranked offspring.
- Public sep-CMA-ES with diagonal covariance, accelerated Ros-Hansen covariance learning and linear internal time/space complexity.
- Exact DOI provenance for Active CMA, sep-CMA, IPOP-CMA-ES and BIPOP-CMA-ES.
- Advanced CMA-ES focused tests, benchmarks and scientific documentation.
- Immutable-patch release packaging with `git apply --check` before mutation and disposable-worktree transaction validation.

## [0.46.0] - 2026-08-22

### Added

- Canonical full-covariance CMA-ES with positive logarithmic recombination, evolution-path cumulation, cumulative step-size adaptation, and rank-one/rank-mu covariance updates.
- Dependency-free deterministic standard-normal sampling and symmetric Jacobi eigendecomposition.
- CMA-ES scientific component catalog with six executable components and active/separable/restart variants explicitly reviewed/deferred.
- Focused evaluation-budget, determinism, stable-ID/factory and parameter tests plus a continuous benchmark.

## [0.45.0] - 2026-08-22

### Added

- Public Ant Colony System (ACS) with pseudo-random proportional transition, local trail update and best-so-far global reinforcement.
- Public MAX-MIN Ant System (MMAS) with bounded pheromone trails, best-only reinforcement and optional stagnation restart.
- Advanced ACO scientific component catalog, documentation, tests and benchmarks.
- Automation v5 release closure: after exact-SHA Build and Test + Build Documentation success, the Create Release workflow publishes the matching GitHub Release automatically.

## [Unreleased]

## [0.44.0]

### Added
- Add the public generic `ant-system-dorigo-maniezzo-colorni-1996` Ant System foundation.
- Add typed constructive feasibility, heuristic-information and pheromone-key contracts instead of a TSP-specific public model.
- Add canonical pheromone/heuristic proportional transition sampling implemented through the exactly equivalent log-space Gumbel-max identity.
- Add sparse lazy pheromone memory: global evaporation is O(1), while newly encountered keys receive the exact accumulated decay of the initial trail.
- Add classical all-ant global reinforcement and explicit deposit-policy composition.
- Add a positive-minimization `Q/L` deposit with strict domain guards plus a representation-independent constant deposit policy.
- Add focused tests for exact evaluation accounting, mid-colony stopping, deterministic seeds, invalid heuristics/objective scales and typed factory registration.
- Add Ant System benchmark coverage, scientific page and a six-entry ACO review catalog.
- Upgrade pack automation to v4: version-aware resume, full documentation/build validation, exact Git scope validation, commit/push, CI monitoring and automatic diagnostics.

### Scientific basis
- Dorigo, Maniezzo & Colorni (1996), *Ant System: Optimization by a Colony of Cooperating Agents*. DOI `10.1109/3477.484436`.
- Dorigo & Gambardella (1997), *Ant Colony System: A Cooperative Learning Approach to the Traveling Salesman Problem*. DOI `10.1109/4235.585892` — reviewed/deferred.
- Stützle & Hoos (2000), *MAX-MIN Ant System*. DOI `10.1016/S0167-739X(00)00043-1` — reviewed/deferred.

### Scope and compatibility
- Public algorithm count increases from 31 to 32; swarm-intelligence count increases from 1 to 2.
- Existing public algorithms and stable IDs are unchanged.
- Continuous-domain ACO, ACS local updates and MAX-MIN pheromone bounds are not approximated under the Ant System identity.


## [0.43.0]

### Added
- Add `MemeticAlgorithmOptimizer<TSolution>` with stable composition ID `memetic-algorithm-moscato-1989`.
- Add a non-public generation extension point to the existing generational GA engine so memetic search reuses selection, crossover, mutation, elitism, ownership and the common optimization lifecycle instead of duplicating them.
- Add five executable local-improvement application policies: every offspring, periodic, probabilistic, objective-ranked top fraction and stagnation-adaptive pressure.
- Add executable Lamarckian and Baldwinian learning policies.
- Add `MemeticAlgorithmState` statistics for local-search invocations, successful improvements, accepted local moves, cumulative gain, stagnation and current local-search probability.
- Add focused ownership, budget, deterministic-seed, periodic/ranked-policy, Lamarckian/Baldwinian and factory-registration tests.
- Add end-to-end GA / disabled-memetic / active-memetic benchmark coverage.
- Add the Memetic Algorithm scientific component catalog, Doxygen page, rendered portal builder and dedicated documentation validator.

### Scientific basis
- Moscato (1989), *On Evolution, Search, Optimization, Genetic Algorithms and Martial Arts: Towards Memetic Algorithms*, Caltech Concurrent Computation Program Report 826.
- Krasnogor & Smith (2005), *A Tutorial for Competent Memetic Algorithms: Model, Taxonomy, and Design Issues*, IEEE Transactions on Evolutionary Computation 9(5), 474-488. DOI `10.1109/TEVC.2005.850260`.

### Architecture and compatibility
- The public v0.42 generational GA constructors and stable ID remain unchanged.
- With a zero-probability local-improvement policy, the memetic layer performs no local-search evaluations and retains GA evaluation accounting.
- Local search uses the same `OptimizationContext<TSolution>` as the population engine, so objective budgets, callbacks, best-so-far ownership, cancellation and stopping criteria remain globally consistent.
- Self-adaptive meme choice and additional population-engine adapters are scientifically cataloged as reviewed/deferred rather than approximated in this release.


## [0.42.0]

### Added
- Add 18 executable Advanced Genetic Algorithm components under stable ga.* component IDs while preserving the single public genetic-algorithm-generational ID.
- Add truncation, linear-ranking, exponential-ranking and explicit-weight fitness-proportionate parent selection alongside the existing tournament selector.
- Add one-point, two-point and uniform crossover for equal-length arrays; PMX and OX1 for validated permutations; bounded SBX for real vectors.
- Add bit-flip, bounded integer random-reset, swap, inversion, bounded Gaussian and bounded polynomial mutation.
- Add a machine-readable operator catalog, generated portal/Doxygen component page, mathematical models and dedicated validator.
- Add focused tests for min/max rank selection, explicit fitness weights, representation legality, real bounds and exact scientific reference metadata.

### Reviewed / deferred
- Keep true steady-state replacement reviewed/deferred because it changes the live parent population between offspring events; it cannot be represented faithfully by only changing the end-of-generation replacement step in the v0.41 runtime.

### Scientific basis
- Goldberg & Deb (1991), DOI 10.1016/B978-0-08-050684-5.50008-2.
- Blickle & Thiele (1996), DOI 10.1162/EVCO.1996.4.4.361.
- Syswerda (1989), DOI 10.5555/645512.657265.
- Syswerda (1991), DOI 10.1016/B978-0-08-050684-5.50009-4 for the generational/steady-state distinction.
- Goldberg & Lingle (1985), DOI 10.5555/645511.657095.
- Davis (1985), DOI 10.5555/1625135.1625164.
- Deb & Agrawal (1995), original SBX paper; no DOI asserted.
- Deb et al. (2002), DOI 10.1109/4235.996017.
- Deb & Deb (2014), DOI 10.1504/IJAISC.2014.059280.

### Compatibility
- Public algorithm count remains 30 and evolutionary-method count remains 7.
- Existing v0.41 GA constructors, stable algorithm ID and generational lifecycle remain source compatible.
- v0.42 adds stable component identities only; no representation-specific operator is misrepresented as a new top-level algorithm.
## [0.41.0]

### Added
- Add the public `genetic-algorithm-generational` Genetic Algorithm foundation.
- Add generic complete-solution population initialization, parent-selection, crossover and mutation contracts with delegate-backed adapters.
- Add objective-sense symmetric tournament parent selection with sampling with replacement.
- Add fixed-size generational replacement with configurable crossover/mutation invocation probabilities and optional non-reevaluated elitism.
- Add strict solution-cloning boundaries for initial population members, parent snapshots, offspring and elites.
- Add 15 focused GA tests covering minimization/maximization selection symmetry, exact evaluation accounting, odd population sizes, variation probabilities, elitism, reproducibility, ownership and typed factory registration.
- Add complete ULSAlgorithms-style mathematical/scientific documentation and a dedicated GA validator.

### Scientific basis
- Eiben & Smith (2003), *Genetic Algorithms*, DOI `10.1007/978-3-662-05094-1_3`.
- Whitley (1994), *A genetic algorithm tutorial*, DOI `10.1007/BF00175354`.
- Blickle & Thiele (1996), *A Comparison of Selection Schemes used in Evolutionary Algorithms*, DOI `10.1162/EVCO.1996.4.4.361`.

### Scope
- v0.41.0 intentionally keeps representation-specific crossover/mutation catalogs, alternative selection schemes and alternative replacement policies for the advanced GA release instead of attaching unsupported operators to the generic foundation.

### Compatibility
- Public algorithm count increases from 29 to 30.
- Evolutionary-method count increases from 6 to 7.
- Existing algorithms and stable IDs are unchanged.
- GA construction uses typed `MetaheuristicFactory.Register(...)` because representation-specific evolutionary operators are required.

## [0.40.0]

### Added
- Add Advanced Scatter Search scientific components under stable `ss.*` component IDs while preserving the canonical public algorithm ID `scatter-search-marti-laguna-glover-2006`.
- Add `ScatterSearchReferenceSetRefreshMode.DynamicImmediate`, which refreshes the subset schedule after an accepted RefSet admission so the new reference solution participates in the next combination round.
- Add `TwoTierScatterSearchReferenceSetUpdateMethod<TSolution>` with separate quality and max-min diversity admission criteria.
- Add an explicit minimum-diversity threshold for quality-tier construction and replacement.
- Add `MaxMinScatterSearchReferenceSetRebuildingMethod<TSolution>` and opt-in stagnation-triggered partial rebuilding.
- Add `GloverScatterSearchSubsetGenerationMethod<TSolution>` implementing representative Subset Types 1-4.
- Add complete scientific component catalog, portal/Doxygen page and dedicated validation.

### Reviewed / deferred
- Keep the 3-tier good-generator RefSet design separate because faithful implementation requires historical generator performance g(x).
- Keep hash-assisted duplication control representation-specific rather than manufacturing a universal collision-safe hash.
- Keep variable-cardinality and binary specialized combination methods deferred to representation-specific contracts.
- Keep explicit evaluated-solution memory and deeper Scatter Search / Path Relinking integration deferred until their identity/memory/trajectory contracts are explicit.

### Scientific basis
- Martí, Laguna & Glover (2006), *Principles of scatter search*, DOI `10.1016/j.ejor.2004.08.004`.
- Laguna & Martí (2003), *Scatter Search: Methodology and Implementations in C*, DOI `10.1007/978-1-4615-0337-8`.
- Glover, Laguna & Martí (2004), *Scatter Search and Path Relinking: Foundations and Advanced Designs*, DOI `10.1007/978-3-540-39930-8_4`.

### Compatibility
- Public algorithm count remains 29.
- Evolutionary-method count remains 6.
- Existing v0.39.0 Scatter Search constructors remain source compatible.
- `ReferenceSetRefreshMode = RoundSnapshot` and `MaximumReferenceSetRebuilds = 0` preserve the v0.39.0 lifecycle by default.

## [0.39.0]

### Added
- Add canonical generic Scatter Search foundation under stable ID `scatter-search-marti-laguna-glover-2006`.
- Add explicit contracts for the five classical Scatter Search methods: diversification generation, improvement, reference-set update, subset generation and solution combination.
- Add `ClassicalScatterSearchReferenceSetUpdateMethod<TSolution>` with initial quality + max-min diversity construction and strict distinct quality replacement.
- Add `PairwiseNewScatterSearchSubsetGenerationMethod<TSolution>` generating all unordered RefSet pairs containing at least one newly admitted reference solution.
- Add objective-sense symmetric RefSet logic for minimization and maximization.
- Add complete ULSAlgorithms-parity documentation, mathematical model, DOI provenance and dedicated validation.

### Scientific basis
- Martí, Laguna & Glover (2006), *Principles of scatter search*, DOI `10.1016/j.ejor.2004.08.004`.
- Laguna & Martí (2003), *Scatter Search: Methodology and Implementations in C*, DOI `10.1007/978-1-4615-0337-8`.
- Glover, Laguna & Martí (2004), *Scatter Search and Path Relinking: Foundations and Advanced Designs*, DOI `10.1007/978-3-540-39930-8_4`.

### Scope boundary
- v0.39.0 implements the scientifically canonical foundation.
- Dynamic RefSet updates, rebuilding, 2-tier/3-tier RefSets, advanced diversity controls, advanced subset families and specialized combination/memory designs are reserved for v0.40.0.

### Compatibility
- Public algorithm count: 29.
- Evolutionary-method count: 6.
- Existing v0.38.0 public IDs and runtime APIs remain unchanged.
## [0.38.0]

### Added
- Add Advanced Iterated Greedy scientific component catalog under stable component IDs `ig.*`.
- Add generic stagnation-escalating destruction-size control while preserving the canonical fixed-size behavior.
- Add a typed partial-solution improvement hook executed strictly between destruction and reconstruction.
- Add generated portal/Doxygen component documentation for 5 executable generic controls and 9 reviewed complete variants.
- Review bounded-search IG, Tabu-reconstruction IG, partial-solution optimization, Iterated Reference Greedy, distributed IG, best-of-breed IG, due-window IG, Adaptive IG and the 2026 two-stage distributed blocking-flowshop IG.

### Audit fixes
- Replace placeholder bibliographic author strings left in the v0.37 advanced-reference placeholders with exact authors.
- Refresh the Iterated Greedy algorithm state after complete-candidate evaluation so algorithm-specific stopping criteria receive a finite `LastCandidateObjective`.

### Architecture
- Keep `iterated-greedy-ruiz-stutzle-2007` as the unique public canonical IG algorithm ID.
- Preserve the original v0.37 constructor and fixed-destruction behavior.
- Expose representation-independent advanced mechanisms as components instead of manufacturing paper-inaccurate top-level algorithms.
- Keep problem-specific complete variants reviewed/deferred until every required domain abstraction exists.

### Scientific basis
- Fernandez-Viagas & Framinan (2015), DOI `10.1080/00207543.2014.948578`.
- Ding et al. (2015), DOI `10.1016/j.asoc.2015.02.006`.
- Dubois-Lacoste, Pagnozzi & Stützle (2017), DOI `10.1016/j.cor.2016.12.021`.
- Ying, Lin, Cheng & He (2017), DOI `10.1016/j.cie.2017.06.025`.
- Ruiz, Pan & Naderi (2019), DOI `10.1016/j.omega.2018.03.004`.
- Fernandez-Viagas & Framinan (2019), DOI `10.1016/j.cor.2019.104767`.
- Jing, Pan, Gao & Wang (2020), DOI `10.1016/j.asoc.2020.106629`.
- Li, Pan, Li, Gao & Tasgetiren (2021), DOI `10.1016/j.swevo.2021.100874`.
- Zhang, Qian, Hu, Li & Yang (2026), DOI `10.1016/j.eswa.2025.130422`.

### Compatibility
- Public algorithm count remains 28.
- Trajectory-method count remains 19.
- Existing v0.37 Iterated Greedy construction remains source compatible.

## [0.37.0]

### Added
- Add canonical Iterated Greedy as `iterated-greedy-ruiz-stutzle-2007`.
- Add typed generic destruction and reconstruction contracts with delegate adapters.
- Add strict-improvement and constant-temperature Metropolis IG acceptance policies.
- Reuse the existing `ILocalSearchProcedure<TSolution>` as an optional initial and post-reconstruction improvement phase.
- Add focused tests proving destruction-before-reconstruction order and that partial solutions are never objective-evaluated.
- Add current Handbook provenance alongside the canonical Ruiz-Stutzle 2007 article.
- Review advanced two-stage, reference-based and adaptive-destruction IG lineages for v0.38.0 without reducing them to configuration flags.

### Architecture
- Candidate solutions are owned clones of the incumbent.
- Destruction may leave the candidate temporarily incomplete; the common objective evaluator is called only after reconstruction.
- Best-so-far ownership remains independent of incumbent acceptance, matching the canonical IG search pattern.
- The generic core does not hard-code NEH, flowshop job representations or processing-time temperature normalization.

### Scientific basis
- Ruiz & Stutzle (2007), DOI `10.1016/j.ejor.2005.12.009`.
- Stutzle & Ruiz (2025), DOI `10.1007/978-3-032-00385-0_10`.
- Advanced lineages reviewed for v0.38.0: DOI `10.1016/j.omega.2018.03.004`, DOI `10.1016/j.cie.2017.06.025`, DOI `10.1016/j.asoc.2020.106629`.

### Performance
- O(1) framework bookkeeping per cycle outside domain destruction, reconstruction, objective evaluation and optional local search.
- No evaluation or cloning of the temporary partial representation beyond the single owned candidate clone.

## [0.36.0]

### Added
- Add Demon-Based Acceptance as `demon-based-acceptance-talbi-2009`.
- Add deterministic conserved credit/energy acceptance with minimization/maximization symmetry.
- Add exact-delta reversible fast path, observable Demon credit state and focused energy-invariant tests.
- Promote `acceptance.demon.budget` from reviewed/deferred to implemented in the acceptance trajectory catalog.
- Add `acceptance.demon.credit-reset-ils` as a separately reviewed/deferred ILS-oriented lineage.
- Review and document the Wood-Downs (1998) BD, RBD, AD and RAD optimization variants separately without collapsing them into flags; no unverified DOI is inserted into the machine-readable catalog.

### Architecture
- Reuse `IAcceptanceTrajectoryInitialSolutionGenerator`, `ReversibleTrajectoryStepExecutor` and `TrajectoryStepEvaluationAccounting` without introducing a parallel trajectory engine.
- Demon acceptance consumes no acceptance RNG draw and stores O(1) scalar state.
- Preserve the Zimmermann-Salamon ensemble Demon Algorithm as a distinct reviewed/deferred method rather than reducing it to a one-point credit rule.

### Scientific basis
- Creutz (1983), DOI `10.1103/PhysRevLett.50.1411`, origin of Demon energy exchange.
- Talbi (2009), Chapter 2, DOI `10.1002/9780470496916.ch2`, generic single-solution metaheuristic presentation.
- Wood & Downs (1998), Demon algorithms for optimization: Bounded, Randomized Bounded, Annealed and Randomized Annealed variants reviewed/deferred separately.
- Zimmermann & Salamon (1992), DOI `10.1080/00207169208804047`, explicitly distinct ensemble Demon Algorithm.

### Performance
- O(1) deterministic acceptance and update: oriented subtraction/comparison plus one subtraction on acceptance.
- Exact-delta rejection never applies a move; no exponential and no acceptance random draw.

## [0.35.0]

### Added
- Add final-form Late Acceptance Hill Climbing as `late-acceptance-hill-climbing-burke-bykov-2017`.
- Add a circular O(L) objective-history controller implementing the Burke-Bykov 2017 acceptance and monotone-quality history-update rules.
- Reuse the reversible trajectory executor, exact-delta fast path, visited-state evaluation accounting, callbacks, stopping and deterministic seeded neighborhood infrastructure.
- Add observable LAHC state, focused xUnit tests, a dedicated scientific page and documentation-parity validation.
- Promote LAHC to executable in the Acceptance-Based Trajectory Methods catalog.
- Preserve Demon-based budget acceptance and the distinct Zimmermann-Salamon ensemble Demon Algorithm as separate reviewed/deferred identities for future faithful implementation.

### Scientific basis
- Burke & Bykov (2008), *A Late Acceptance Strategy in Hill-Climbing for Exam Timetabling Problems*.
- Burke & Bykov (2017), *The late acceptance Hill-Climbing heuristic*, DOI `10.1016/j.ejor.2016.07.012`.
- Zimmermann & Salamon (1992), *The demon algorithm*, DOI `10.1080/00207169208804047`, reviewed/deferred and explicitly distinguished from one-point Demon-like credit acceptance.

### Performance
- Acceptance and circular-history update are O(1) per attempted transition.
- LAHC-specific state is O(L) scalar objective values and stores no solution snapshots.
- Exact-delta rejection does not apply a move; accepted moves are applied exactly once.
- Acceptance itself consumes no random draw; stochasticity comes from the configured neighborhood.

## [0.34.0]

### Added
- Add classical Great Deluge as `great-deluge-dueck-1993`.
- Add classical Record-to-Record Travel as `record-to-record-travel-dueck-1993`.
- Add shared acceptance-trajectory initial-solution composition and visited-candidate evaluation accounting.
- Add exact-delta reversible fast paths, observable states, parameters and DOI-backed descriptors for both methods.
- Add the Acceptance-Based Trajectory Methods scientific catalog and generated portal component.
- Review Extended Great Deluge and Adaptive Flex-Deluge separately from the Dueck canonical rules.
- Add focused tests for sense symmetry, absolute-level semantics, record semantics, exact-delta rejection and probe-versus-visited best accounting.

### Architecture
- `TrajectoryStepEvaluationAccounting` counts every candidate as a probe and promotes best-so-far only for accepted visited states.
- Simulated Annealing and Threshold Accepting now reuse that accounting path without changing their acceptance semantics.
- The acceptance-policy foundation is ready for Late Acceptance and Demon-based methods.

### Scientific basis
- Dueck (1993), DOI `10.1006/jcph.1993.1010`.
- Burke, Bykov, Newall & Petrovic (2003), DOI `10.2298/YJOR0302139B`, Extended Great Deluge reviewed/deferred.
- Burke & Bykov (2016), DOI `10.1287/ijoc.2015.0680`, Adaptive Flex-Deluge reviewed/deferred.

### Performance
- GDA and RRT acceptance are deterministic O(1) comparisons with no acceptance random draw or transcendental function.
- Exact-delta rejection never applies a move; accepted moves are applied exactly once.
- Both acceptance controllers use O(1) state.

## [0.33.0]

### Added
- Add canonical Dueck-Scheuer Threshold Accepting as public stable ID `threshold-accepting-dueck-scheuer-1990`.
- Add `ThresholdAcceptancePolicy` with deterministic sense-aware acceptance of worsening transitions whose degradation does not exceed the active threshold.
- Reuse `ReversibleTrajectoryStepExecutor` so exact move-objective deltas reject moves without mutation and accepted moves are applied exactly once.
- Add linear, geometric and explicit non-increasing threshold schedules plus a custom schedule contract.
- Add common trajectory degradation computation to `TrajectoryObjectiveComparison`.
- Add complete runtime state, callback/stopping integration, neighborhood-exhaustion handling and exact external-evaluation accounting.
- Add a scientific Threshold Accepting schedule catalog and generated portal component page.
- Add focused tests for minimization/maximization acceptance, zero-threshold behavior, schedules, delta fast path, threshold-level stopping and stable catalog identity.

### Scientific basis
- Dueck & Scheuer (1990), *Threshold accepting: A general purpose optimization algorithm appearing superior to simulated annealing*, DOI `10.1016/0021-9991(90)90201-B`.
- Winker & Fang (1997), *Application of Threshold-Accepting to the Evaluation of the Discrepancy of a Set of Points*, DOI `10.1137/S0036142995286076`.
- Hu, Kahng & Tsao (1995), *Old Bachelor Acceptance*, DOI `10.1287/ijoc.7.4.417`, reviewed/deferred because its self-tuning non-monotone and potentially negative threshold semantics require a distinct controller.

### Performance
- Threshold acceptance is O(1), deterministic and requires neither a Metropolis exponential nor an acceptance random draw.
- The exact-delta fast path is O(C_delta) for rejected transitions and applies accepted moves once.
- No per-transition solution clone is required on the reversible trajectory path.
- Threshold control uses O(1) state for linear/geometric schedules; explicit schedules retain only their configured finite sequence.

## [0.32.0]

### Added
- Add `EvolutionaryPathRelinkingProcedure<TSolution>` implementing generational all-pairs elite-population evolution.
- Add the Resende-Werneck EvPR population admission rule: best-improving override, otherwise worst-quality improvement plus diversity, followed by replacement of the closest dominated elite.
- Add opt-in EvPR post-optimization to `GraspPathRelinkingOptimizer<TSolution>` while preserving the existing `grasp-path-relinking` stable ID.
- Add independent evolutionary pairwise controls with efficient defaults: mixed direction, greedy-randomized adaptive move selection and full path.
- Add optional local improvement of every EvPR offspring before elite admission.
- Extend `GraspPathRelinkingState` with non-breaking init-only evolutionary generation, pairing, path, evaluation, local-search and elite-update statistics.
- Promote `pr.evolutionary` from reviewed/deferred to executable in the scientific Path Relinking catalog.

### Scientific basis
- Resende & Werneck (2004), *A Hybrid Heuristic for the p-Median Problem*, DOI `10.1023/B:HEUR.0000019986.96257.50`.
- Resende, Marti, Gallego & Duarte (2010), *GRASP and path relinking for the max-min diversity problem*, DOI `10.1016/j.cor.2008.05.011`.
- Ribeiro & Resende (2012), *Path-relinking intensification methods for stochastic local search algorithms*, DOI `10.1007/s10732-011-9167-1`.

### Compatibility and performance
- `EvolutionaryPathRelinkingEnabled` defaults to `false`; v0.31.0 behavior is unchanged unless EvPR is explicitly enabled.
- The elite population remains bounded by `ElitePoolSize`.
- One EvPR generation performs at most `E(E-1)/2` pairwise relinkings for elite capacity `E`.
- Mixed PR is the default evolutionary direction to explore both endpoint regions without the approximately doubled traversal cost of back-and-forward.
- Greedy-randomized adaptive PR is the default evolutionary move policy to reduce deterministic path replay across recurring elite pairs.
## [0.31.0]

### Added
- Add `AdvancedPathRelinkingProcedure<TSolution,TMove,TUndo,TEnumerator>` with forward, backward, back-and-forward and mixed trajectory policies.
- Add orthogonal truncated path relinking through `PathFraction` and greedy-randomized adaptive path move selection through a GRASP-style RCL.
- Add `IAdvancedPathRelinkingProcedure<TSolution>` without breaking the existing `IPathRelinkingProcedure<TSolution>` compatibility contract.
- Return stored elite guide fitness to advanced relinking so backward and mixed policies avoid duplicate objective evaluations.
- Use pooled candidate-probe buffers for randomized RCL selection while keeping greedy scans allocation-free.
- Add a machine-readable advanced path-relinking strategy catalog and canonical scientific component page.

### Scientific scope
- Implement the pairwise strategies reviewed by Ribeiro & Resende (2012), DOI `10.1007/s10732-011-9167-1`.
- Keep evolutionary path relinking explicitly reviewed/deferred because it requires a population-level elite evolution contract rather than a pairwise path flag.

## [0.30.1]

### Fixed
- HTML-encode catalog TeX before inserting formulas into portal HTML, preventing `<`, `>` and `&` from being interpreted as markup and truncating mathematical content.
- Replace pseudo-mathematical catalog text with explicit MathJax-compatible TeX for all 22 public algorithms.
- Replace the generated `MetaheuristicFactory.Create<...>` placeholder on every portal algorithm page with the canonical C# API example from that algorithm's Markdown documentation.
- Add horizontal overflow protection for long MathJax equations in both the portal and Doxygen output.
- Correct the GRASP Path Relinking bibliography to the published Resende-Ribeiro 2005 chapter, DOI `10.1007/0-387-25383-1_2`.
- Reformat all 22 primary update equations as aligned display mathematics to avoid excessive single-line widths.
- Component catalog formulas now carry an explicit `formulaMode` so reviewed qualitative controllers are rendered as prose rather than fake mathematics.
- Replace duplicated portal "Detailed operation" content with a direct link to the canonical full scientific Doxygen page.
- Pin portal and Doxygen rendering to MathJax `3.2.2` instead of the rolling `@3` CDN alias.

### Validation
- Add `Test-ScientificFormulaQuality.ps1` to reject pseudo-math, malformed delimiter ownership, unbalanced braces and unsynchronized API examples.
- Add `Test-RenderedPortalQuality.ps1` to validate the generated HTML itself: two safe formula blocks per algorithm, exact catalog-to-HTML formula parity and canonical API-example parity.

## [0.30.0]

### Added
- GRASP with Path Relinking (`grasp-path-relinking`) as the first public constructive/hybrid method.
- Explicit `IPathRelinkingDistance<TSolution>`, target-directed path-neighborhood and reusable path-procedure contracts.
- `GreedyForwardPathRelinkingProcedure` with allocation-free path cursors, exact move-objective delta fast path and reversible full-evaluation fallback.
- Fixed-capacity `EliteSolutionPool<TSolution>` with owned snapshots, duplicate suppression, minimum-distance diversity and quality-based worst replacement.
- Uniform allocation-free elite-guide selection by reservoir sampling.
- Common-runtime probe accounting: all path candidates consume objective-evaluation budget, while only actually visited selected path states may promote global best.
- Focused GRASP-PR tests and full documentation-parity validation.

### Scientific basis
- Resende & Ribeiro (2003), *GRASP and path-relinking: Recent advances and applications*.
- Aiex, Resende, Pardalos & Toraldo (2005), *GRASP with Path Relinking for Three-Index Assignment*, DOI `10.1287/ijoc.1030.0059`.
- Feo & Resende (1995), *Greedy Randomized Adaptive Search Procedures*, DOI `10.1007/BF01096763`.

### Performance
- Elite memory is bounded by `ElitePoolSize`; guide selection allocates no temporary list.
- Target-directed candidate scans use value-type cursors.
- Exact candidate objectives avoid apply/evaluate/undo when `IMoveObjectiveDeltaEvaluator` is supplied.
- Unvisited candidates use probe accounting and therefore do not require solution snapshots.

## [0.29.0]

### Added
- Reactive GRASP (`reactive-grasp-prais-ribeiro-2000`) following Prais & Ribeiro (2000).
- Per-run `PraisRibeiroReactiveAlphaController` with a discrete alpha set, uniform initial probabilities, online per-alpha objective means and periodic probability updates.
- Canonical minimization quality rule `q_i = z_best / A_i`, followed by `p_i = q_i / sum(q_j)`.
- Sense-consistent maximization mirror `q_i = A_i / z_best`.
- Explicit runtime protection of the canonical ratio assumption: objective observations used for adaptation must be strictly positive.
- Reactive GRASP documentation, stable catalog ID and focused validation.
- Eleven focused tests including probability learning, positivity guards, stable catalog identity and common stopping lifecycle.

### Fixed
- Canonical GRASP now calls `OptimizationContext.CompleteIteration` after each completed construction + local-search cycle, so common iteration statistics, callbacks and `MaxIterationsStoppingCriterion` work correctly.

### Scientific basis
- Prais & Ribeiro (2000), *Reactive GRASP: An Application to a Matrix Decomposition Problem in TDMA Traffic Assignment*, DOI `10.1287/ijoc.12.3.164.12639`.
- Feo & Resende (1995), *Greedy Randomized Adaptive Search Procedures*, DOI `10.1007/BF01096763`.
- GRASP with Path Relinking remains reviewed/deferred until an explicit elite-set/path contract is introduced.

### Performance
- Reactive state is O(m) for m configured alpha values.
- Alpha selection and periodic probability recomputation are O(m).
- Running per-alpha means avoid retaining historical objective samples.
- Construction keeps the v0.28 allocation-free threshold-RCL reservoir-selection fast path.
## [0.28.0]

### Added
- Canonical GRASP (`grasp-feo-resende-1995`) following Feo & Resende's two-phase construction + local-search framework.
- Generic allocation-free `IGraspCandidateEnumerator` and `IGraspConstructionModel` contracts for domain-owned constructive components and greedy scores.
- `CanonicalGraspConstructionProcedure` with adaptive threshold RCL, sense-aware greedy scoring and uniform reservoir sampling without materializing the RCL.
- Reuse of the existing `ILocalSearchProcedure<TSolution>` contract, preserving common objective accounting, callbacks, deterministic RNG, best-so-far ownership and stopping.
- First-class `Constructive methods` documentation family.
- Scientific GRASP catalog recording canonical GRASP as executable while Reactive GRASP and GRASP with Path Relinking remain reviewed/deferred for later advanced contracts.
- Focused GRASP validation and unit tests.

### Scientific basis
- Feo & Resende (1989), *A probabilistic heuristic for a computationally difficult set covering problem*, DOI `10.1016/0167-6377(89)90002-3`.
- Feo & Resende (1995), *Greedy Randomized Adaptive Search Procedures*, DOI `10.1007/BF01096763`.
- Prais & Ribeiro (2000), *Reactive GRASP*, DOI `10.1287/ijoc.12.3.164.12639` (reviewed/deferred).

### Performance
- The RCL is never allocated: construction uses two restartable candidate scans and O(1) reservoir state.
- Greedy scores are recomputed after every accepted construction component, preserving canonical adaptivity.
- Local improvement reuses the already optimized local-search engine and exact-delta/reversible fast paths when the composed procedure supports them.
## [0.27.1]

### Fixed
- Doxygen warnings and internal parser diagnostics are now release-blocking.
- Markdown mathematical displays use Doxygen-native `\f[ ... \f]` delimiters; legacy `\[ ... \]` / `\( ... \)` source delimiters are rejected by validation.
- Modern C# record/required/init syntax is transformed only in Doxygen's input stream through `Doxygen-CSharpCompatibilityFilter.ps1`; compiled source and public API remain unchanged.
- Doxygen output is captured in `Documentation/doxygen-build.log` for reproducible diagnostics.
- Added complete PSO communication-topology documentation for all ten implemented topology classes.
- Added machine-readable `pso-topology-catalog.json` with exact/generic provenance, construction parameters, information-flow semantics and graph rebuild behavior.
- Documented exact dynamic DCluster, including `N = p(p+1)`, worst-to-best current-fitness ranking, clique construction and per-iteration fitness-dynamic rebuild.
- Added the PSO topology catalog to the generated portal `Scientific components` section and linked it from the Particle Swarm algorithm page.
- Generated the existing Advanced VNS component catalog in the public documentation portal, eliminating the previously source-only component page.
- Reorganized README scientific components and fixed missing blank lines after raw HTML tables so all `All algorithms` family headings render uniformly on GitHub.
- Added validation that fails when PSO topology documentation becomes incomplete or README family-heading rendering regresses.

### Compatibility
- No algorithm behavior, stable algorithm ID, topology implementation or public optimization API changed.
- The release is documentation/build-validation only; the validated algorithm count remains 19.
## [0.27.0]

### Added
- Reduced Variable Neighborhood Search (`reduced-variable-neighborhood-search`).
- General Variable Neighborhood Search (`general-variable-neighborhood-search`) using the reusable VND procedure as its improvement phase.
- Skewed Variable Neighborhood Search (`skewed-variable-neighborhood-search-hansen-mladenovic-2001`) with a domain-owned solution-distance contract and sense-consistent skewed recentering.
- Advanced VNS catalog with three executable variants and Variable Neighborhood Decomposition Search (VNDS) explicitly reviewed/deferred rather than falsely reduced to ordinary shaking.
- Stable IDs, runtime/documentation catalog entries, mathematical pages, focused tests and dedicated validation.

### Scientific basis
- Hansen & Mladenovic (2001), *Variable neighborhood search: Principles and applications*, DOI 10.1016/S0377-2217(00)00100-4.
- Hansen, Mladenovic, Todosijevic & Hanafi (2017), *Variable neighborhood search: basics and variants*, DOI 10.1007/s13675-016-0075-x.

### Compatibility
- All v0.26.0 public IDs and behavior remain unchanged.
- RVNS/GVNS/SVNS reuse established shaking, local-search, VND, cloning and OptimizationContext contracts.
- VNDS remains non-executable until a truthful generic decomposition/subproblem abstraction is introduced.
## [0.26.0]

### Added
- Canonical Guided Local Search (`guided-local-search-voudouris-tsang-1999`).
- Allocation-free domain feature cursors and generic feature-cost modeling.
- Canonical utility `c_i / (1 + p_i)` with all maximum-utility ties penalized together.
- Sense-consistent augmented objective for minimization and maximization.
- Optional exact penalty-sum delta evaluator complementing the existing exact objective-delta path.
- Original-objective best-so-far promotion for probed candidates even when augmented guidance rejects them.
- Stable ID, runtime/documentation catalogs, mathematical documentation, focused tests and dedicated validation.

### Scientific basis
- Tsang & Voudouris (1997), *Fast local search and guided local search and their application to British Telecom's workforce scheduling problem*, DOI 10.1016/S0167-6377(96)00042-9.
- Voudouris & Tsang (1999), *Guided local search and its application to the traveling salesman problem*, DOI 10.1016/S0377-2217(98)00099-X.

### Performance
- Move and feature enumeration are allocation-free.
- Exact objective and exact penalty-sum deltas can eliminate full candidate objective evaluation and full active-feature rescans.
- Candidate solution cloning is reserved for original-objective best-so-far promotion rather than every neighborhood probe.

### Compatibility
- All v0.25.0 public IDs and behavior remain unchanged.
- GLS reuses the established reversible-move, neighborhood, delta-evaluation and common OptimizationContext contracts.
## [0.25.0]

### Added
- Variable Neighborhood Descent (`variable-neighborhood-descent`) as both a standalone optimizer and reusable `ILocalSearchProcedure<TSolution>`.
- Canonical basic Variable Neighborhood Search (`variable-neighborhood-search-mladenovic-hansen`).
- Ordered shaking neighborhoods through the existing `ISolutionPerturbation<TSolution>` abstraction.
- Direct VNS/VND composition without duplicating the v0.23 local-search engine.
- Stable IDs, runtime/documentation catalog entries, mathematical documentation, focused tests and dedicated validation.

### Scientific basis
- Mladenovic & Hansen (1997), *Variable neighborhood search*, DOI 10.1016/S0305-0548(97)00031-2.
- Hansen & Mladenovic (2001), *Variable neighborhood search: Principles and applications*, DOI 10.1016/S0377-2217(00)00100-4.

### Compatibility
- All v0.24.1 public IDs and behavior remain unchanged.
- VND and VNS reuse `ILocalSearchProcedure<TSolution>`, `ISolutionPerturbation<TSolution>` and the common `OptimizationContext`.
- Existing exact-delta and reversible local-search fast paths remain available inside VND/VNS compositions.
## [0.24.1]

### Fixed
- Repaired UTF-8 mojibake in the README, changelog, machine-readable algorithm catalog and generated documentation shell.
- Corrected the README public-algorithm count and integrated all seven trajectory algorithms into one coherent catalog section.
- Added deterministic text-encoding validation so common UTF-8/Windows-1252 corruption fails CI.
- Added a project favicon and consistent favicon injection across generated portal, component and Doxygen HTML pages.
- Added release notes sourced from the matching CHANGELOG section instead of relying only on generic generated notes.
- Added a repository metadata helper for description, homepage and topics.
## [0.24.0]

### Added
- Multi-Start Local Search (`multi-start-local-search`) composed from the reusable v0.23 local-search procedure.
- Iterated Local Search (`iterated-local-search-lourenco-martin-stutzle`) with initial descent, domain-owned perturbation, repeated local improvement and configurable incumbent acceptance.
- `ISolutionPerturbation<TSolution>`, delegate-backed perturbations and `NeighborhoodAcceptanceKind`.
- Dedicated parameter types, stable IDs, runtime catalog entries, machine-readable catalog, mathematical documentation, validator and focused tests.

### Scientific basis
- Lourenço, Martin & Stützle (2003), *Iterated Local Search*, DOI 10.1007/0-306-48056-5_11.
- Martí (2003), *Multi-Start Methods*, DOI 10.1007/0-306-48056-5_12.
- Talbi (2009), DOI 10.1002/9780470496916.

### Compatibility
- The v0.23 first- and best-improvement implementations and stable IDs are unchanged.
- Restart and ILS reuse `ILocalSearchProcedure<TSolution>`; neighborhood scan logic is not duplicated.
- v0.23 validation now checks the foundation on later releases without forbidding legitimate v0.24 extensions.
## [0.23.0]

### Added
- Generic allocation-free local-search procedure with first- and best-improvement selection.
- Exact objective-delta fast path and reversible apply/evaluate/undo fallback.
- Best-Improvement Local Search (`local-search-best-improvement`).
- First-Improvement Local Search (`local-search-first-improvement`).
- Stable-ID, runtime-catalog, documentation, validation and focused-test coverage for the Local Search core.

### Scientific basis
- Talbi (2009), DOI 10.1002/9780470496916.


## [0.22.0]

### Added
- Reactive Tabu Search as a distinct public algorithm with stable ID `reactive-tabu-search-battiti-tecchiolli-1994`.
- Hash-based configuration-repetition memory with cycle-length observation and domain-owned 64-bit solution signatures.
- Reactive prohibition-period controller: tenure grows on detected repetition, decreases after sustained non-repetition, and requests diversification when repetitions persist.
- Allocation-free random-walk escape using reservoir sampling; only the selected escape move is objectively evaluated.
- Long-term attribute-frequency memory and optional frequency-guided candidate ranking.
- Optional elite-restart intensification after configurable stagnation.
- Runtime and machine-readable `ts.*` component catalogs with 10 executable components and 3 reviewed advanced strategies.
- Full documentation-parity page, family panel, scientific component page, validator and dedicated tests.

### Scientific basis
- Glover (1989), DOI 10.1287/ijoc.1.3.190.
- Glover (1990), DOI 10.1287/ijoc.2.1.4.
- Battiti & Tecchiolli (1994), DOI 10.1287/ijoc.6.2.126.
- Glover & Laguna (1997), DOI 10.1007/978-1-4615-6089-0.

### Compatibility
- `tabu-search-glover` remains the stable Glover short-term-memory foundation introduced in v0.21.
- Reactive Tabu Search has a separate stable algorithm identity rather than changing v0.21 semantics.
- Tabu Search component IDs use the stable `ts.*` namespace.

## [0.21.0]

### Added
- Generic Glover-style Tabu Search short-term-memory engine with stable ID `tabu-search-glover`.
- Allocation-free full-neighborhood scan through the existing value-type neighborhood cursor contract.
- Attribute-based expiration memory with expected O(1) lookup and min-heap expiration ordering for varying tenures.
- Best-so-far aspiration criterion plus a zero-evaluation fast rejection path when aspiration is disabled.
- Fixed and uniformly varying tabu-tenure policies, with custom tenure and aspiration extension points.
- Exact objective-delta fast path and reversible apply/evaluate/undo fallback without per-candidate solution cloning.
- Probe-evaluation lifecycle in `OptimizationContext` so candidate scans preserve exact evaluation accounting without promoting unvisited neighbors.
- Full ULSAlgorithms-parity algorithm page, catalog metadata, README panel, validator, and unit tests from the first public TS version.

### Scientific basis
- Glover (1986), DOI 10.1016/0305-0548(86)90048-1.
- Glover (1989), DOI 10.1287/ijoc.1.3.190.
- Glover (1990), DOI 10.1287/ijoc.2.1.4.
- Glover & Laguna (1997), DOI 10.1007/978-1-4615-6089-0.
- Battiti & Tecchiolli (1994), DOI 10.1287/ijoc.6.2.126, reviewed as a later reactive-search extension rather than falsely claimed as part of the fixed short-term core.

### Scope
- v0.21 implements the generic short-term-memory TS foundation.
- Intermediate/long-term intensification-diversification memory and Reactive Tabu Search remain explicit future controllers because they require additional problem/state memory beyond a scalar tenure rule.
## [0.20.0]

### Added
- Scientific Simulated Annealing cooling-schedule catalog with stable `sa.cooling.*` IDs.
- Ten executable built-in cooling laws: geometric, Lundy-Mees, linear finite-horizon, normalized Hajek logarithmic, Szu-Hartley inverse-linear, Ingber very-fast, Tsallis-Stariolo generalized, Aarts-van Laarhoven statistical, Huang statistical and Triki adaptive.
- Runtime cooling-schedule descriptors and discovery catalog.
- Optional `CustomCoolingSchedule` extension point.
- Allocation-free Welford objective statistics activated only for statistical cooling schedules.
- Scientific catalog JSON, dedicated documentation page and repository validator.
- Explicit reviewed-composite treatment of Otten-van Ginneken adaptive control, Lam-Delosme and constant-thermodynamic-speed annealing rather than scientifically incomplete approximations.

### Scientific basis
- Aarts & van Laarhoven (1985), *Statistical cooling: a general approach to combinatorial optimization problems*.
- Lundy & Mees (1986), DOI 10.1007/BF01582166.
- Huang, Romeo & Sangiovanni-Vincentelli (1986), IEEE ICCAD.
- Szu & Hartley (1987), DOI 10.1016/0375-9601(87)90796-1.
- Hajek (1988), DOI 10.1287/moor.13.2.311.
- Lam & Delosme (1988), DOI 10.1109/DAC.1988.14775.
- Salamon et al. (1988), DOI 10.1016/0010-4655(88)90003-3.
- Otten & van Ginneken (1989), *The Annealing Algorithm*, DOI 10.1007/978-1-4613-1627-5.
- Ingber (1989), DOI 10.1016/0895-7177(89)90202-1.
- Strenski & Kirkpatrick (1991), *Analysis of finite length annealing schedules*, Algorithmica 6(3), 346-366.
- Tsallis & Stariolo (1996), DOI 10.1016/S0378-4371(96)00271-3.
- Cohn & Fielding (1999), DOI 10.1137/S1052623497329683.
- Triki, Collette & Siarry (2005), DOI 10.1016/j.ejor.2004.03.035.

### Compatibility
- Existing `Geometric = 0` and `LundyMees = 1` enum numeric values are preserved.
- Existing five-position `SimulatedAnnealingCoolingContext` constructor is preserved; v0.20 adds init-only per-level statistics.
- Non-statistical schedules retain the previous hot path without objective-statistics accumulation.

## [0.16.0]

### Added
- Canonical L-SHADE optimizer.
- Linear Population Size Reduction driven by NFE/MAX_NFE.
- Dimension-scaled tuned initial population.
- Dynamic active population over fixed flat capacity.
- Allocation-free survivor compaction.
- Dynamic logical archive sizing and deterministic random trimming.
- SHADE 1.1 terminal CR-memory semantics for L-SHADE.
- Weighted Lehmer success-history updates for both CR and F.
- Tuned CEC2014 L-SHADE parameter defaults.
- LPSR, memory, archive and deterministic-parallel tests.
- SHADE/L-SHADE benchmark suite.

### Tuned defaults
- N_init = round(18 * D).
- N_min = 4.
- Archive ratio = 2.6.
- p = 0.11.
- H = 6.
- MAX_NFE = 10,000 * D unless overridden.

### Scientific basis
Tanabe & Fukunaga (2014),
IEEE CEC 2014, 1658-1665.
DOI: 10.1109/CEC.2014.6900380.

## [0.15.0]

### Added
- SHADE.

## [0.14.0]

### Added
- JADE.

## [0.13.0]

### Added
- jDE.
## [0.17.0]

### Added
- Generic stochastic-neighborhood contract.
- Allocation-free struct enumerator contract for enumerated neighborhoods.
- Generic move applicability contract.
- `ref TSolution` move application.
- Reversible move application with compact undo tokens.
- Exact move-objective delta evaluation contract.
- Sense-aware trajectory objective comparison.
- Generic trajectory acceptance policy.
- Greedy acceptance policy.
- Reversible zero-clone trajectory step executor.
- Clone-based fallback trajectory step executor.
- Exception-safe undo path.
- Allocation-free trajectory statistics accumulator.
- Neighborhood and trajectory architecture documentation.

### Architecture
This release starts the trajectory/local-search branch without changing PSO or DE.

### Scientific context
- Metropolis et al. (1953), DOI 10.1063/1.1699114.
- Kirkpatrick, Gelatt & Vecchi (1983), DOI 10.1126/science.220.4598.671.
## [0.18.0]

### Added
- Generic `SimulatedAnnealingOptimizer<TSolution,TMove,TUndo>`.
- Generic initial-solution generator contract and delegate adapter.
- Metropolis acceptance policy.
- Exact target-acceptance temperature inversion.
- Geometric cooling schedule.
- Lundy-Mees cooling schedule.
- Configurable temperature levels.
- Minimum-temperature algorithm-specific stop.
- Neighborhood-exhausted algorithm-specific stop.
- Common OptimizationContext lifecycle integration.
- External delta/full evaluation accounting.
- Deterministic generic SA tests.
- Delta-versus-full transition BenchmarkDotNet suite.

### Scientific basis
- Metropolis et al. (1953), DOI 10.1063/1.1699114.
- Kirkpatrick, Gelatt & Vecchi (1983), DOI 10.1126/science.220.4598.671.
- Lundy & Mees (1986), DOI 10.1007/BF01582166.
