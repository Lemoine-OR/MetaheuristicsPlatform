# Changelog

All notable changes to MetaheuristicsPlatform will be documented in this file.

## [Unreleased]

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