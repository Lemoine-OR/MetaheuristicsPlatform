@page case_based_heuristic_selection_burke_petrovic_qu_2006 Case-Based Heuristic Selection

# Case-Based Heuristic Selection

## General description

Case-Based Heuristic Selection (`CaseBasedHyperHeuristic`) is the public scientific identity associated with
Burke, Petrovic & Qu (2006), *Case Based Heuristic Selection for Timetabling Problems*. It operates above a domain-provided pool of
low-level heuristics.

## Reproduction mode

`mechanism-preserving-platform-adaptation`. The named high-level selection, credit, memory or
acceptance mechanism is preserved. `IHyperHeuristicDomain`, deterministic random plumbing,
cancellation, factory/catalog integration and the benchmark/test harness are explicit platform
adaptations and are not claimed to reproduce the authors' experimental software verbatim.

## Technical specifications

- Stable ID: `case-based-heuristic-selection-burke-petrovic-qu-2006`
- Class: `CaseBasedHyperHeuristicOptimizer`
- Parameters: `CaseBasedHyperHeuristicParameters`
- Family: Hyper-heuristics and algorithm selection
- Domain contract: `IHyperHeuristicDomain`
- Low-level heuristic contract: `ILowLevelHeuristic`
- Result: `HyperHeuristicOptimizationResult`
- Public since: v0.143.0
- Primary DOI/permanent identifier: `10.1007/s10951-006-6775-y`

## Complexity

Each high-level iteration applies at least one low-level heuristic and evaluates the resulting
solution. Selector overhead depends on the named memory, case, bandit or acceptance policy.

## Applicability

Reusable cross-domain optimization where a problem domain exposes a finite low-level heuristic
pool and a clonable solution state.

## Detailed operation

A feature description of the current search state retrieves the most similar stored case and reuses its associated low-level heuristic.

## Parameters

`CaseBasedHyperHeuristicParameters` validates high-level learning, exploration, memory and acceptance controls.

## API example

```csharp
IHyperHeuristicDomain domain = GetDomain();

var result =
    new CaseBasedHyperHeuristicOptimizer().Optimize(
        domain,
        new CaseBasedHyperHeuristicParameters(),
        new OptimizationOptions { Seed = 123456UL });
```

## Stable factory ID

`case-based-heuristic-selection-burke-petrovic-qu-2006`

## Mathematical details

### Problem formulation

\f[
\operatorname{HH}(\mathcal H,\phi,x_0):
\quad h_t\in\mathcal H,\quad
x_{t+1}\leftarrow h_t(x_t).
\f]

### Update equations / iterations

\f[
\begin{aligned}c^\star&=\arg\min_{c\in\mathcal C}\lVert\phi(x_t)-\phi(c)\rVert_2,\\h_t&=h(c^\star).\end{aligned}
\f]

### Assumptions

Finite objective values, non-empty uniquely identified low-level heuristics, clonable domain
states and deterministic replay for a fixed platform seed.

### Convergence conditions

No universal finite-time global-convergence claim is asserted. Performance depends on the
quality/complementarity of the low-level heuristic pool and the named high-level policy.

### Scientific references

Burke, Petrovic & Qu (2006), *Case Based Heuristic Selection for Timetabling Problems*, Journal of Scheduling 9(2), 115-132.
DOI/permanent identifier: `10.1007/s10951-006-6775-y`.
