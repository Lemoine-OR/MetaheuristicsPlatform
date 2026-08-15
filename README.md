<p align="center">
  <img src="docs/assets/metaheuristicsplatform-logo.svg" alt="MetaheuristicsPlatform" width="650">
</p>

<p align="center">
  <strong>Fast, scientific and reusable C# / .NET metaheuristics with a common high-performance architecture.</strong>
</p>

<p align="center">
  <a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform/actions/workflows/build.yml"><img alt="Build and Test" src="https://github.com/Lemoine-OR/MetaheuristicsPlatform/actions/workflows/build.yml/badge.svg"></a>
  <a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform/actions/workflows/documentation.yml"><img alt="Documentation" src="https://github.com/Lemoine-OR/MetaheuristicsPlatform/actions/workflows/documentation.yml/badge.svg"></a>
  <a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform/releases/latest"><img alt="Latest release" src="https://img.shields.io/github/v/release/Lemoine-OR/MetaheuristicsPlatform?display_name=tag&sort=semver"></a>
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512BD4">
  <img alt="MIT" src="https://img.shields.io/badge/license-MIT-0B7285">
  <img alt="Stable IDs" src="https://img.shields.io/badge/catalog-stable%20IDs-15803D">
</p>

<p align="center">
  <a href="https://lemoine-or.github.io/MetaheuristicsPlatform/"><strong>Project & Documentation</strong></a>
  ·
  <a href="https://lemoine-or.github.io/MetaheuristicsPlatform/#algorithms"><strong>Algorithms</strong></a>
  ·
  <a href="https://lemoine-or.github.io/MetaheuristicsPlatform/api/getting_started.html"><strong>Getting started</strong></a>
  ·
  <a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform/releases/latest"><strong>Latest release</strong></a>
  ·
  <a href="https://github.com/Lemoine-OR/MetaheuristicsPlatform/tree/main/src/MetaheuristicsPlatform"><strong>Source</strong></a>
</p>

---

MetaheuristicsPlatform is a research-grade high-performance library for reusable
metaheuristics. Public methods share a common optimization lifecycle and are indexed by
stable catalog IDs.

<table>
<tr>
<td width="25%"><strong>1 swarm method</strong><br><sub>PSO with topology/social/dynamics specializations.</sub></td>
<td width="25%"><strong>5 DE methods</strong><br><sub>DE, jDE, JADE, SHADE and L-SHADE.</sub></td>
<td width="25%"><strong>3 trajectory methods</strong><br><sub>Scientific SA, Glover Tabu Search and Reactive Tabu Search.</sub></td>
<td width="25%"><strong>Generic foundations</strong><br><sub>Evaluation pipelines, neighborhoods, reversible moves and hybrid composition.</sub></td>
</tr>
</table>

<p align="center"><strong>9 public algorithms · one lifecycle · stable catalog IDs</strong></p>

## Start in 30 seconds

For a parameterless built-in method, use the stable factory ID:

```csharp
using MetaheuristicsPlatform.Catalog;
using MetaheuristicsPlatform.Algorithms.DE;

DifferentialEvolutionOptimizer algorithm =
    MetaheuristicFactory.Create<DifferentialEvolutionOptimizer>(
        MetaheuristicAlgorithmIds.DifferentialEvolution);
```

For a composed generic algorithm, keep the same stable ID and register the typed
composition once:

```csharp
MetaheuristicFactory.Register(
    MetaheuristicAlgorithmIds.SimulatedAnnealing,
    () => configuredSimulatedAnnealing,
    replace: true);
```

> **New to the library?** Open the [Getting Started guide](https://lemoine-or.github.io/MetaheuristicsPlatform/api/getting_started.html).
> **Looking for a method?** Browse the panels below or the [project documentation](https://lemoine-or.github.io/MetaheuristicsPlatform/).
> **Need reproducibility?** Use stable algorithm IDs, deterministic seeds and versioned releases.

## Why MetaheuristicsPlatform?

<table>
<tr>
<td width="25%"><strong>Fast</strong><br><sub>Flat memory, deterministic RNG streams, calibrated coarse parallelism and fused built-in fast paths.</sub></td>
<td width="25%"><strong>Scientific</strong><br><sub>Explicit provenance, mathematical details, applicability and DOI metadata.</sub></td>
<td width="25%"><strong>Uniform</strong><br><sub>Common lifecycle, stable IDs, canonical catalog and typed factory.</sub></td>
<td width="25%"><strong>Extensible</strong><br><sub>Generic evaluation pipelines, neighborhoods, reversible moves, subsolvers and hybrid composition.</sub></td>
</tr>
</table>

## Choose a family

<table>
<tr>
<td width="25%"><strong>Swarm intelligence</strong><br><sub>Collective motion and social information.</sub></td>
<td width="25%"><strong>Evolutionary methods</strong><br><sub>Population variation, selection and adaptive DE.</sub></td>
<td width="25%"><strong>Trajectory-based</strong><br><sub>Single-solution neighborhood trajectories.</sub></td>
<td width="25%"><strong>Hybrid / memetic</strong><br><sub>Compositions with local search, decoders and subsolvers.</sub></td>
</tr>
</table>

## All algorithms

Click a method name to open its dedicated documentation page. Every panel shows the
stable ID used by the canonical catalog/factory.

### Swarm intelligence

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/particle-swarm.html"><strong>Particle Swarm Optimization</strong></a><br><sub>Swarm intelligence · O(ND) per iteration for the canonical graphless fast path; topology/social policies may add overhead</sub><br><code>particle-swarm</code><br><sub><code>ParticleSwarmOptimizer</code></sub></td><td width="50%"></td></tr>
</table>
### Evolutionary methods

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/differential-evolution.html"><strong>Differential Evolution</strong></a><br><sub>Evolutionary methods · O(ND) per generation for classical mutation/crossover, plus objective-evaluation cost</sub><br><code>differential-evolution</code><br><sub><code>DifferentialEvolutionOptimizer</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/jde-brest-2006.html"><strong>jDE — Self-Adaptive Differential Evolution</strong></a><br><sub>Evolutionary methods · O(ND) per generation plus objective-evaluation cost</sub><br><code>jde-brest-2006</code><br><sub><code>SelfAdaptiveDifferentialEvolutionOptimizer</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/jade-2009.html"><strong>JADE</strong></a><br><sub>Evolutionary methods · O(ND + N log N) per generation plus objective-evaluation cost</sub><br><code>jade-2009</code><br><sub><code>JadeOptimizer</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/shade-2013.html"><strong>SHADE</strong></a><br><sub>Evolutionary methods · O(ND + N log N) per generation plus objective-evaluation cost</sub><br><code>shade-2013</code><br><sub><code>ShadeOptimizer</code></sub></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/lshade-2014.html"><strong>L-SHADE</strong></a><br><sub>Evolutionary methods · O(N_kD + N_k log N_k) at generation k plus objective-evaluation cost</sub><br><code>lshade-2014</code><br><sub><code>LShadeOptimizer</code></sub></td><td width="50%"></td></tr>
</table>
### Trajectory-based methods

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/simulated-annealing-metropolis.html"><strong>Simulated Annealing</strong></a><br><sub>Trajectory-based methods · O(C_move + C_eval) per attempted transition; O(C_delta) when an exact differential evaluator is available</sub><br><code>simulated-annealing-metropolis</code><br><sub><code>SimulatedAnnealingOptimizer<TSolution,TMove,TUndo></code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/tabu-search-glover.html"><strong>Tabu Search</strong></a><br><sub>Trajectory-based methods - best-admissible memory-guided neighborhood search with exact-delta fast path</sub><br><code>tabu-search-glover</code><br><sub><code>TabuSearchOptimizer&lt;...&gt;</code></sub></td></tr>
</table>
<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/reactive-tabu-search-battiti-tecchiolli-1994.html"><strong>Reactive Tabu Search</strong></a><br><sub>Trajectory-based methods &middot; repetition-aware adaptive tenure and reactive diversification</sub><br><code>reactive-tabu-search-battiti-tecchiolli-1994</code><br><sub><code>ReactiveTabuSearchOptimizer&lt;...&gt;</code></sub></td><td width="50%"></td></tr>
</table>
### Simulated Annealing scientific cooling catalog

<table>
<tr><td><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/components/simulated-annealing-cooling-schedules.html"><strong>Scientific Cooling Catalog</strong></a><br><sub>10 executable laws &middot; geometric, Lundy-Mees, linear, logarithmic/Hajek, Szu-Hartley, Ingber, Tsallis-Stariolo, Aarts-van Laarhoven, Huang and Triki &middot; stable <code>sa.cooling.*</code> IDs &middot; broader controllers reviewed without false reduction.</sub></td></tr>
</table>

### Tabu Search memory and reactive-control catalog

<table>
<tr><td><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/components/tabu-search-memory-control-strategies.html"><strong>Memory &amp; Reactive Control Catalog</strong></a><br><sub>10 executable components &middot; short/long-term memory &middot; reactive tenure &middot; intensification &middot; diversification &middot; stable <code>ts.*</code> IDs.</sub></td></tr>
</table>
### Hybrid / memetic methods

<table>
<tr><td><strong>Foundation ready</strong><br><sub>No public hybrid algorithm yet; the generic evaluation and trajectory contracts are designed for memetic/hybrid composition.</sub></td></tr>
</table>


## Documentation contract

Every public algorithm page must contain:
- general description;
- technical specifications;
- time and space complexity;
- applicability;
- detailed operation;
- generic and specific parameters;
- API example;
- stable factory ID;
- complete **Mathematical details** with LaTeX problem formulation, update equations,
  assumptions and convergence conditions;
- scientific references and DOI.

The documentation build fails when this contract is not satisfied.
