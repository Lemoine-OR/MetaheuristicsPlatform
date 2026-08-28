@page tabu_search_hyperheuristic_burke_kendall_soubeiga_2003 Tabu-Search Hyper-Heuristic

# Tabu-Search Hyper-Heuristic

## General description

Tabu-Search Hyper-Heuristic (`TabuSearchHyperHeuristic`) is the public scientific identity associated with
Burke, Kendall & Soubeiga (2003), *A Tabu-Search Hyperheuristic for Timetabling and Rostering*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `tabu-search-hyperheuristic-burke-kendall-soubeiga-2003`
- Class: `TabuSearchHyperHeuristicOptimizer`
- Parameters: `TabuSearchHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.142.0
- Primary DOI/permanent identifier: `10.1023/B:HEUR.0000012446.94732.B6`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

Heuristic-level reinforcement scores compete under a tabu list; non-improving low-level heuristics become temporarily unavailable.

## Parameters

`TabuSearchHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new TabuSearchHyperHeuristicOptimizer().Optimize(
        domain,
        new TabuSearchHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`tabu-search-hyperheuristic-burke-kendall-soubeiga-2003`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}h_t&=\arg\max_{h\notin T_t}S_h(t),\\S_h(t+1)&=(1-\alpha)S_h(t)+\alpha r_t.\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Burke, Kendall & Soubeiga (2003), *A Tabu-Search Hyperheuristic for Timetabling and Rostering*, Journal of Heuristics 9(6), 451-470.
DOI/permanent identifier: `10.1023/B:HEUR.0000012446.94732.B6`.
