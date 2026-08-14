# Changelog

All notable changes to MetaheuristicsPlatform will be documented in this file.

## [Unreleased]

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