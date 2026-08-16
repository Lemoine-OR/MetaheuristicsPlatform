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
<td width="20%"><strong>1 swarm method</strong><br><sub>PSO with topology/social/dynamics specializations.</sub></td>
<td width="20%"><strong>5 DE methods</strong><br><sub>DE, jDE, JADE, SHADE and L-SHADE.</sub></td>
<td width="20%"><strong>13 trajectory methods</strong><br><sub>SA, Tabu Search, Local Search, ILS, VNS variants and GLS.</sub></td>
<td width="20%"><strong>2 constructive methods</strong><br><sub>Canonical GRASP with adaptive randomized greedy construction and local search.</sub></td>
<td width="20%"><strong>Generic foundations</strong><br><sub>Evaluation pipelines, neighborhoods, reversible moves and hybrid composition.</sub></td>
</tr>
</table>

<p align="center"><strong>21 public algorithms · one lifecycle · stable catalog IDs</strong></p>

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
<td width="20%"><strong>Swarm intelligence</strong><br><sub>Collective motion and social information.</sub></td>
<td width="20%"><strong>Evolutionary methods</strong><br><sub>Population variation, selection and adaptive DE.</sub></td>
<td width="20%"><strong>Trajectory-based</strong><br><sub>Single-solution neighborhood trajectories.</sub></td>
<td width="20%"><strong>Constructive methods</strong><br><sub>Adaptive randomized construction followed by improvement.</sub></td>
<td width="20%"><strong>Hybrid / memetic</strong><br><sub>Compositions with local search, decoders and subsolvers.</sub></td>
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
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/simulated-annealing-metropolis.html"><strong>Simulated Annealing</strong></a><br><code>simulated-annealing-metropolis</code></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/tabu-search-glover.html"><strong>Tabu Search</strong></a><br><code>tabu-search-glover</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/reactive-tabu-search-battiti-tecchiolli-1994.html"><strong>Reactive Tabu Search</strong></a><br><code>reactive-tabu-search-battiti-tecchiolli-1994</code></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/local-search-best-improvement.html"><strong>Local Search - Best Improvement</strong></a><br><code>local-search-best-improvement</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/local-search-first-improvement.html"><strong>Local Search - First Improvement</strong></a><br><code>local-search-first-improvement</code></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/multi-start-local-search.html"><strong>Multi-Start Local Search</strong></a><br><code>multi-start-local-search</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/iterated-local-search-lourenco-martin-stutzle.html"><strong>Iterated Local Search</strong></a><br><code>iterated-local-search-lourenco-martin-stutzle</code></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/variable-neighborhood-descent.html"><strong>Variable Neighborhood Descent</strong></a><br><code>variable-neighborhood-descent</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/variable-neighborhood-search-mladenovic-hansen.html"><strong>Basic Variable Neighborhood Search</strong></a><br><code>variable-neighborhood-search-mladenovic-hansen</code></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/reduced-variable-neighborhood-search.html"><strong>Reduced Variable Neighborhood Search</strong></a><br><code>reduced-variable-neighborhood-search</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/general-variable-neighborhood-search.html"><strong>General Variable Neighborhood Search</strong></a><br><code>general-variable-neighborhood-search</code></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/skewed-variable-neighborhood-search-hansen-mladenovic-2001.html"><strong>Skewed Variable Neighborhood Search</strong></a><br><code>skewed-variable-neighborhood-search-hansen-mladenovic-2001</code></td></tr>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/guided-local-search-voudouris-tsang-1999.html"><strong>Guided Local Search</strong></a><br><code>guided-local-search-voudouris-tsang-1999</code></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-variable-neighborhood-search-variants.html"><strong>Advanced VNS catalog</strong></a><br><sub>RVNS / GVNS / SVNS executable; VNDS reviewed/deferred pending a decomposition contract.</sub></td></tr>
</table>

### Constructive methods

<table>
<tr><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/grasp-feo-resende-1995.html"><strong>GRASP - Feo-Resende</strong></a><br><sub>Adaptive randomized greedy threshold-RCL construction + reusable local search; allocation-free RCL selection.</sub><br><code>grasp-feo-resende-1995</code><br><sub><code>GraspOptimizer&lt;TSolution&gt;</code></sub></td><td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/algorithms/reactive-grasp-prais-ribeiro-2000.html"><strong>Reactive GRASP - Prais-Ribeiro</strong></a><br><sub>Self-tuning discrete alpha probabilities learned from per-alpha solution quality.</sub><br><code>reactive-grasp-prais-ribeiro-2000</code><br><sub><code>ReactiveGraspOptimizer&lt;TSolution&gt;</code></sub></td></tr>
</table>

### Hybrid / memetic methods

<table>
<tr><td><strong>Foundation ready</strong><br><sub>No public hybrid algorithm yet; the generic evaluation and trajectory contracts are designed for memetic/hybrid composition.</sub></td></tr>
</table>

## Scientific components

<table>
<tr>
<td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/components/pso-communication-topologies.html"><strong>PSO Communication Topology Catalog</strong></a><br><sub>10 implemented topology classes &middot; exact-vs-generic provenance &middot; static, random-static and dynamic rebuild semantics &middot; exact DCluster documented in detail</sub><br><code>pso.topology.*</code></td>
<td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/components/simulated-annealing-cooling-schedules.html"><strong>Simulated Annealing Scientific Cooling Catalog</strong></a><br><sub>10 executable cooling schedules &middot; broader controllers reviewed without false reduction</sub><br><code>sa.cooling.*</code></td>
</tr>
<tr>
<td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/components/tabu-search-memory-control-strategies.html"><strong>Tabu Search Memory &amp; Reactive Control Catalog</strong></a><br><sub>10 executable components &middot; memory, reactive tenure, intensification and diversification</sub><br><code>ts.*</code></td>
<td width="50%"><a href="https://lemoine-or.github.io/MetaheuristicsPlatform/components/advanced-variable-neighborhood-search-variants.html"><strong>Advanced Variable Neighborhood Search Variants</strong></a><br><sub>RVNS / GVNS / SVNS executable &middot; VNDS reviewed/deferred pending a decomposition contract</sub><br><code>vns.variants</code></td>
</tr>
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
