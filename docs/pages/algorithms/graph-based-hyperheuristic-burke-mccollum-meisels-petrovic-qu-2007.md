@page graph_based_hyperheuristic_burke_mccollum_meisels_petrovic_qu_2007 Graph-Based Hyper-Heuristic

# Graph-Based Hyper-Heuristic

## General description

Graph-Based Hyper-Heuristic (`GraphBasedHyperHeuristic`) is the public scientific identity associated with
Burke, McCollum, Meisels, Petrovic & Qu (2007), *A graph-based hyper-heuristic for educational timetabling problems*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `graph-based-hyperheuristic-burke-mccollum-meisels-petrovic-qu-2007`
- Class: `GraphBasedHyperHeuristicOptimizer`
- Parameters: `GraphBasedHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.144.0
- Primary DOI/permanent identifier: `10.1016/j.ejor.2005.08.012`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

A tabu search explores permutations of domain-provided low-level heuristics; each sequence is evaluated as a high-level heuristic ordering.

## Parameters

`GraphBasedHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new GraphBasedHyperHeuristicOptimizer().Optimize(
        domain,
        new GraphBasedHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`graph-based-hyperheuristic-burke-mccollum-meisels-petrovic-qu-2007`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}\pi'&=\operatorname{swap}(\pi,i,j),\\\pi_{t+1}&=\arg\min_{\pi'\notin T_t}\widetilde f(H_{\pi'}(x_0)).\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Burke, McCollum, Meisels, Petrovic & Qu (2007), *A graph-based hyper-heuristic for educational timetabling problems*, European Journal of Operational Research 176(1), 177-192.
DOI/permanent identifier: `10.1016/j.ejor.2005.08.012`.
